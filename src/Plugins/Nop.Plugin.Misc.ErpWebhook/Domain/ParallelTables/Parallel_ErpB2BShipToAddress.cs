namespace Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

public class Parallel_ErpShipToAddress : BaseParallelEntity
{
    public string ShipToCode { get; set; }
    public string ShipToName { get; set; }
    public int AddressId { get; set; }
    public string Suburb { get; set; }
    public string ProvinceCode { get; set; }
    public string DeliveryNotes { get; set; }
    public string EmailAddresses { get; set; }
    public int B2BAccountId { get; set; }
    public int B2BSalesOrganisationId { get; set; }
    public string RepNumber { get; set; }
    public string RepFullName { get; set; }
    public string RepPhoneNumber { get; set; }
    public string RepEmail { get; set; }
    public int ShipToAddressCreatedByTypeId { get; set; }
    public int OrderId { get; set; }
}
