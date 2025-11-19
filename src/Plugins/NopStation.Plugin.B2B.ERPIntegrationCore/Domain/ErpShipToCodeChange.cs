using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpShipToCodeChange : BaseEntity
{
    public string MatchField { get; set; }

    public string CustomerAccountNumber { get; set; }

    public string SalesOrg { get; set; }

    public string AsIsCustomerShiptoParty { get; set; }

    public string ToBeCustomerShiptoParty { get; set; }
}
