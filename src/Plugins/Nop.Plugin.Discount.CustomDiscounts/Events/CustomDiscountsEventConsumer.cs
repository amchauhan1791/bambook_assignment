using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Discounts;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Events;

namespace Nop.Plugin.Discount.CustomDiscounts.Events;

/// <summary>
/// Discount requirement rule event consumer (used for removing unused settings)
/// </summary>
public class CustomDiscountsEventConsumer : IConsumer<EntityDeletedEvent<DiscountRequirement>>
{
    #region Fields

    protected readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public CustomDiscountsEventConsumer(ISettingService settingService)
    {
        _settingService = settingService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handle discount requirement deleted event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(EntityDeletedEvent<DiscountRequirement> eventMessage)
    {
        var discountRequirement = eventMessage?.Entity;
        if (discountRequirement == null)
            return;

        //delete saved restricted customer role identifier if exists
        var setting = await _settingService.GetSettingAsync(string.Format(CustomDiscountsDefaults.SettingsKey, discountRequirement.Id));
        if (setting != null)
            await _settingService.DeleteSettingAsync(setting);
    }

    #endregion
}
