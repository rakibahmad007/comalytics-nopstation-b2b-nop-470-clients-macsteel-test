namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Order;

public class B2BAdditionalOrderTotalsModel
{
    public bool IsVisibleWeight { get; set; }

    public string TotalWeight { get; set; }

    public decimal CustomerAccountSavings { get; set; }

    public string CustomerAccountSavingsValue { get; set; }

    public string TotalPriceWithOutSavings { get; set; }

    public string B2BOnlineOrderDiscount { get; set; }

    public bool IsShowPaymentTermsDescription { get; set; }

    public string PaymentTermsDescription { get; set; }
    public bool ProductForQuote { get; set; }
}
