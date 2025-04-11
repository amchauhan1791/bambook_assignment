using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Identity.Client;
using Nop.Core;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;

namespace Nop.Plugin.Discount.CustomDiscounts;

public class CustomDiscountsProvider : BasePlugin, IDiscountRequirementRule
{
    #region Fields
    protected readonly IActionContextAccessor _actionContextAccessor;
    protected readonly IDiscountService _discountService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IRepository<Order> _orderRepository;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;
    protected readonly IUrlHelperFactory _urlHelperFactory;
    protected readonly IWebHelper _webHelper;
    #endregion

    #region Ctor
    public CustomDiscountsProvider(IActionContextAccessor actionContextAccessor,
        IDiscountService discountService,
        ILocalizationService localizationService,
        IRepository<Order> orderRepository,
        ISettingService settingService,
        IStoreContext storeContext,
        IUrlHelperFactory urlHelperFactory,
        IWebHelper webHelper)
    {
        _actionContextAccessor = actionContextAccessor;
        _discountService = discountService;
        _localizationService = localizationService;
        _orderRepository = orderRepository;
        _settingService = settingService;
        _storeContext = storeContext;
        _urlHelperFactory = urlHelperFactory;
        _webHelper = webHelper;
    }
    #endregion

    #region Methods

    #region Discount Rules
    /// <summary>
    /// Check discount requirement
    /// </summary>
    /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var customDiscountSettings = await _settingService.LoadSettingAsync<CustomDiscountsSettings>(storeScope);

        //invalid by default
        var result = new DiscountRequirementValidationResult();

        if (request.Customer == null || !customDiscountSettings.Enabled)
            return result;

        var customerOrdersCount = _orderRepository.Table.Count(x => x.CustomerId == request.Customer.Id);

        if (customerOrdersCount >= 3)
            result.IsValid = true;
        else
            result.IsValid = false;

        return result;
    }

    /// <summary>
    /// Get URL for rule configuration
    /// </summary>
    /// <param name="discountId">Discount identifier</param>
    /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
    /// <returns>URL</returns>
    public string GetConfigurationUrl(int discountId, int? discountRequirementId)
    {
        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

        return urlHelper.Action("ConfigureRequirement", "Configuration",
            new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
    }
    #endregion

    #region Contiguration URL
    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/Configuration/Configure";
    }
    #endregion

    #region Install / UnInstall
    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //check before discount create
        var isExists = (await _discountService.GetAllDiscountsAsync()).Any(x => x.Name.Contains(CustomDiscountsDefaults.DiscountName));
        if (!isExists)
        {
            var discount = new Core.Domain.Discounts.Discount
            {
                IsActive = true,
                Name = CustomDiscountsDefaults.DiscountName,
                DiscountType = DiscountType.AssignedToOrderSubTotal,
                UsePercentage = true,
                DiscountPercentage = 10,
                RequiresCouponCode = false,
                DiscountLimitation = DiscountLimitationType.Unlimited
            };

            await _discountService.InsertDiscountAsync(discount);

            var requirement = (await _discountService.GetAllDiscountRequirementsAsync()).FirstOrDefault(x => x.DiscountRequirementRuleSystemName.Contains(CustomDiscountsDefaults.SystemName));
            if (requirement == null)
            {
                requirement = new DiscountRequirement
                {
                    DiscountId = discount.Id,
                    IsGroup = true,
                    DiscountRequirementRuleSystemName = CustomDiscountsDefaults.SystemName,
                };

                await _discountService.InsertDiscountRequirementAsync(requirement);

                //save restricted customer role identifier
                await _settingService.SetSettingAsync(string.Format(CustomDiscountsDefaults.SettingsKey, requirement.Id), 0);
            }
        }

        //setting
        var setting = new CustomDiscountsSettings {
            Enabled = true,
            DiscountPercentage = 10
        };

        await _settingService.SaveSettingAsync(setting);

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Discount.CustomDiscounts.Fields.Enabled"] = "Enabled",
            ["Plugins.Discount.CustomDiscounts.Fields.Enabled.Hint"] = "Checked to enabled plugin to apply discount for regular customer during the checkout.",
            ["Plugins.Discount.CustomDiscounts.Fields.DiscountPercentage"] = "Discount percent(%)",
            ["Plugins.Discount.CustomDiscounts.Fields.DiscountPercentage.Hint"] = "Enter percentage you want to give as discount.",
            ["Plugins.Discount.CustomDiscounts.Settings"] = "Configuration settings."
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //if discount exists then delete
        var discount = (await _discountService.GetAllDiscountsAsync()).FirstOrDefault(x => x.Name.Contains(CustomDiscountsDefaults.DiscountName));
        if (discount != null)
            await _discountService.DeleteDiscountAsync(discount);

        var requirement = (await _discountService.GetAllDiscountRequirementsAsync()).FirstOrDefault(x => x.DiscountRequirementRuleSystemName.Contains(CustomDiscountsDefaults.SystemName));
        if (requirement != null)
            await _discountService.DeleteDiscountRequirementAsync(requirement, false);

        //setting
        await _settingService.DeleteSettingAsync<CustomDiscountsSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Discount.CustomDiscounts");

        await base.UninstallAsync();
    }
    #endregion
    #endregion
}
