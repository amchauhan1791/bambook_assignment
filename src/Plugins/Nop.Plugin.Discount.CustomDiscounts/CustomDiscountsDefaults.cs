﻿namespace Nop.Plugin.Discount.CustomDiscounts;

/// <summary>
/// Represents defaults for the custom discount requirement rule
/// </summary>
public static class CustomDiscountsDefaults
{
    /// <summary>
    /// The system name of the discount requirement rule
    /// </summary>
    public static string SystemName => "DiscountRequirement.MustBeAssignedToRegularCustomer";

    /// <summary>
    /// The key of the settings to save restricted customer roles
    /// </summary>
    public static string SettingsKey => "DiscountRequirement.MustBeAssignedToCustomerRole-{0}";

    /// <summary>
    /// The HTML field prefix for discount name requirements
    /// </summary>
    public static string DiscountName => "Discount assigned to regular customer";

    /// <summary>
    /// The HTML field prefix for discount requirements
    /// </summary>
    public static string HtmlFieldPrefix => "DiscountRulesCustomerRoles{0}";

    /// <summary>
    /// The JWT authentication key
    /// </summary>
    public static string JwtKey => "amchauhan1791";

    /// <summary>
    /// The JWT authentication key
    /// </summary>
    public static string JwtIssuer => "Bamboo Cards";

    /// <summary>
    /// The JWT authentication key
    /// </summary>
    public static string JwtAudience => "All Company Memebers";

    /// <summary>
    /// The JWT authentication key
    /// </summary>
    public static int JwtExpiresInMinutes => 50;
}
