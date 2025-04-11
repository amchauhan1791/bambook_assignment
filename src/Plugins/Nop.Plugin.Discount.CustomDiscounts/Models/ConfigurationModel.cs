using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Discount.CustomDiscounts.Models;

/// <summary>
/// Represents a configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Discount.CustomDiscounts.Fields.Enabled")]
    public bool Enabled { get; set; }
    public bool Enabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Discount.CustomDiscounts.Fields.DiscountPercentage")]
    public decimal DiscountPercentage { get; set; }
    public bool DiscountPercentage_OverrideForStore { get; set; }

}
