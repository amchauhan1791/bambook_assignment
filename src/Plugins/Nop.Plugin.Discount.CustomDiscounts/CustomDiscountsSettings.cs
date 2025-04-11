using Nop.Core.Configuration;

namespace Nop.Plugin.Discount.CustomDiscounts;

/// <summary>
/// Represents custom discounts plugin settings
/// </summary>
public class CustomDiscountsSettings : ISettings
{
    #region Properties

    public bool Enabled { get; set; }

    public decimal DiscountPercentage { get; set; }

    #endregion
}
