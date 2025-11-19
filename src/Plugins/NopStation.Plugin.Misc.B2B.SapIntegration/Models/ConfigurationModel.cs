using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;

public class ConfigurationModel
{
    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.HttpCallMaxRetries")]
    public int HttpCallMaxRetries { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.HttpCallRestTimeInSeconds")]
    public int HttpCallRestTimeInSeconds { get; set; }

    public bool HideGeneralBlock { get; set; }

    // Customer creation
    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Salesperson")]
    public string Salesperson { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.TermsCode")]
    public string TermsCode { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.CustomerClass")]
    public string CustomerClass { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.ArStatementNo")]
    public string ArStatementNo { get; set; }

    // Order creation
    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.OrderType")]
    public string OrderType { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.NsProductClass_ShippingLine")]
    public string NsProductClass_ShippingLine { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.NsProductClass_Promotion")]
    public string NsProductClass_Promotion { get; set; }

    // Payment
    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.BankCode_Peach")]
    public string BankCode_Peach { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.BankCode_Loyalty")]
    public string BankCode_Loyalty { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultEmailToSyncErpOrders")]
    public string DefaultEmailToSyncErpOrders { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultFirstNameToSyncErpOrders")]
    public string DefaultFirstNameToSyncErpOrders { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultLastNameToSyncErpOrders")]
    public string DefaultLastNameToSyncErpOrders { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultPhoneNumberToSyncErpOrders")]
    public string DefaultPhoneNumberToSyncErpOrders { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultMobileNumberToSyncErpOrders")]
    public string DefaultMobileNumberToSyncErpOrders { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.SAPCompanyCode")]
    public string SAPCompanyCode { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.VendorName")]
    public string VendorName { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.ShippingCostSKU")]
    public string ShippingCostSKU { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.B2BOrderTypeMappings")]
    public string B2BOrderTypeMappings { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.B2COrderTypeMappings")]
    public string B2COrderTypeMappings { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.AppServerHost")]
    public string AppServerHost { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.SystemNumber")]
    public string SystemNumber { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.SystemID")]
    public string SystemID { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.User")]
    public string User { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Password")]
    public string Password { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.RepositoryPassword")]
    public string RepositoryPassword { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Client")]
    public string Client { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Language")]
    public string Language { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.PoolSize")]
    public string PoolSize { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.Misc.B2B.SapIntegration.Fields.AliasUser")]
    public string AliasUser { get; set; }

    [NopResourceDisplayName("Integration.IntegrationSecretKey")]
    public string IntegrationSecretKey { get; set; }
}
