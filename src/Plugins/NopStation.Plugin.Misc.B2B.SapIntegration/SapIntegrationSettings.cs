using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.SapIntegration;

/// <summary>
/// Represents a plugin settings
/// </summary>
public class SapIntegrationSettings : ISettings
{
    /// <summary>
    /// Gets or sets the ErpCallTimeOut
    /// </summary>
    public int ErpCallTimeOut { get; set; }
    public int HttpCallMaxRetries { get; set; }
    public int HttpCallRestTimeInSeconds { get; set; }

    //for erp customer creation
    public string Salesperson { get; set; }
    public string TermsCode { get; set; }
    public string CustomerClass { get; set; }
    public string ArStatementNo { get; set; }

    //for erp order creation
    public string OrderType { get; set; }
    public string NsProductClass_ShippingLine { get; set; }
    public string NsProductClass_Promotion { get; set; }

    //default user for erp order sync
    public string DefaultEmailToSyncErpOrders { get; set; }
    public string DefaultFirstNameToSyncErpOrders { get; set; } = string.Empty;
    public string DefaultLastNameToSyncErpOrders { get; set; } = string.Empty;
    public string DefaultPhoneNumberToSyncErpOrders { get; set; }
    public string DefaultMobileNumberToSyncErpOrders { get; set; }
    public string GetDefaultUserFullNameToSyncErpOrders()
    {
        return $"{DefaultFirstNameToSyncErpOrders} {DefaultLastNameToSyncErpOrders}";
    }

    //for erp payment
    public string BankCode_Peach { get; set; }
    public string BankCode_Loyalty { get; set; }

    public string SAPCompanyCode { get; set; }
    public string VendorName { get; set; }
    public string ShippingCostSKU { get; set; }
    public string B2BOrderTypeMappings { get; set; }
    public string B2COrderTypeMappings { get; set; }

    public string AppServerHost { get; set; }
    public string SystemNumber { get; set; }
    public string SystemID { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string RepositoryPassword { get; set; }
    public string Client { get; set; }
    public string Language { get; set; }
    public string PoolSize { get; set; }
    public string AliasUser { get; set; }
    public string IntegrationSecretKey { get; set; }
}