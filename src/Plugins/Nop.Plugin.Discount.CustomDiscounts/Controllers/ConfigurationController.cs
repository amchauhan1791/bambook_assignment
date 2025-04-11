using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Discount.CustomDiscounts.Models;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Discount.CustomDiscounts.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class ConfigurationController : BasePluginController
{
    #region Fields
    protected readonly IDiscountService _discountService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;
    #endregion

    #region Ctor
    public ConfigurationController(IDiscountService discountService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext) 
    {
        _discountService = discountService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
    }
    #endregion

    #region Utilities

    /// <summary>
    /// Get errors message from model state
    /// </summary>
    /// <param name="modelState">Model state</param>
    /// <returns>Errors message</returns>
    protected IEnumerable<string> GetErrorsFromModelState(ModelStateDictionary modelState)
    {
        return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
    }

    #endregion

    #region Methods
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure()
    {
        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var customDiscountSettings = await _settingService.LoadSettingAsync<CustomDiscountsSettings>(storeScope);

        var model = new ConfigurationModel
        {
            Enabled = customDiscountSettings.Enabled,
            DiscountPercentage = customDiscountSettings.DiscountPercentage,
            ActiveStoreScopeConfiguration = storeScope
        };

        if (storeScope > 0)
        {
            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(customDiscountSettings, x => x.Enabled, storeScope);
            model.DiscountPercentage_OverrideForStore = await _settingService.SettingExistsAsync(customDiscountSettings, x => x.DiscountPercentage, storeScope);
        }

        return View("~/Plugins/Discount.CustomDiscounts/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var customDiscountSettings = await _settingService.LoadSettingAsync<CustomDiscountsSettings>(storeScope);

        customDiscountSettings.Enabled = model.Enabled;
        customDiscountSettings.DiscountPercentage = model.DiscountPercentage;

        /* We do not clear cache after each setting update.
         * This behavior can increase performance because cached settings will not be cleared 
         * and loaded from database after each update */
        await _settingService.SaveSettingOverridablePerStoreAsync(customDiscountSettings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(customDiscountSettings, x => x.DiscountPercentage, model.DiscountPercentage_OverrideForStore, storeScope, false);

        //in-active discount
        var discount = (await _discountService.GetAllDiscountsAsync()).FirstOrDefault(x => x.Name.Contains(CustomDiscountsDefaults.DiscountName));
        if (discount != null)
        {
            discount.IsActive = model.Enabled;
            discount.DiscountPercentage = model.DiscountPercentage;

            await _discountService.UpdateDiscountAsync(discount);
        }

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #region Discount Requirements
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public async Task<IActionResult> ConfigureRequirement(int discountId, int? discountRequirementId)
    {
        //load the discount
        var discount = await _discountService.GetDiscountByIdAsync(discountId)
                       ?? throw new ArgumentException("Discount could not be loaded");

        //check whether the discount requirement exists
        if (discountRequirementId.HasValue && await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value) is null)
            return Content("Failed to load requirement.");

        var model = new RequirementModel
        {
            RequirementId = discountRequirementId ?? 0,
            DiscountId = discountId,
        };

        //set the HTML field prefix
        ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(CustomDiscountsDefaults.HtmlFieldPrefix, discountRequirementId ?? 0);

        return View("~/Plugins/Discount.CustomDiscounts/Views/ConfigureRequirement.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public async Task<IActionResult> ConfigureRequirement(RequirementModel model)
    {
        if (ModelState.IsValid)
        {
            //load the discount
            var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
            if (discount == null)
                return NotFound(new { Errors = new[] { "Discount could not be loaded" } });

            //get the discount requirement
            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);

            //the discount requirement does not exist, so create a new one
            if (discountRequirement == null)
            {
                discountRequirement = new DiscountRequirement
                {
                    DiscountId = discount.Id,
                    DiscountRequirementRuleSystemName = CustomDiscountsDefaults.SystemName
                };

                await _discountService.InsertDiscountRequirementAsync(discountRequirement);
            }

            //save restricted customer role identifier
            await _settingService.SetSettingAsync(string.Format(CustomDiscountsDefaults.SettingsKey, discountRequirement.Id), 0);

            return Ok(new { NewRequirementId = discountRequirement.Id });
        }

        return Ok(new { Errors = GetErrorsFromModelState(ModelState) });
    }
    #endregion

    #endregion
}
