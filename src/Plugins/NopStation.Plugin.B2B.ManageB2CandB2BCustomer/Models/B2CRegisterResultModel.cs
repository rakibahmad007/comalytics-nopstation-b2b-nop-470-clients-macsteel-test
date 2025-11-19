using Nop.Core.Domain.Common;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;
public record B2CRegisterResultModel : BaseNopModel
{
    public string Result { get; set; }
    public string Title { get; set; }
    public bool IsCustomerOnDeliveryRoute { get; set; }
    public bool IsRegistrationSuccessful { get; set; }
    public string WarehouseName { get; set; }
    public string WarehousePhone { get; set; }
    public string WarehouseEmail { get; set; }
    public string WarehouseLatitude { get; set; }
    public string WarehouseLongitude { get; set; }
    public string WarehouseAdminComment { get; set; }
    public Address WarehouseAddress { get; set; }
    public string SalesOrganisationName { get; set; }
}