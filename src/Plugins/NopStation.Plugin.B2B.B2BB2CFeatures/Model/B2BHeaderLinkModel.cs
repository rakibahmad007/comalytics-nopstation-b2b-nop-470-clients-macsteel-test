namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model;

public class B2BHeaderLinkModel
{
    public bool IsB2BSalesRepImpersonating { get; set; }
    public bool HasB2BCustomerAccountManagerRole { get; set; }
    public bool IsShowYearlySavings { get; set; }
    public bool IsShowAllTimeSavings { get; set; }
    public string CurrentYearOnlineSavings { get; set; }
    public string AllTimeOnlineSavings { get; set; }
    public int DefaultAddressChanged { get; set; }

    public bool IsSalesRep { get; set; }
}