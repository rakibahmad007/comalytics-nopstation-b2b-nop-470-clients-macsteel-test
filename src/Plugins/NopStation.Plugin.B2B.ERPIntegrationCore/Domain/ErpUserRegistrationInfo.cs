using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpUserRegistrationInfo : BaseEntity
{
    public int NopCustomerId { get; set; }
    public int ErpSalesOrgId { get; set; }
    public int ErpUserId { get; set; }
    public int NearestWareHouseId { get; set; }
    public int AddressId { get; set; }
    public int DeliveryOptionId { get; set; }
    public decimal? DistanceToNearestWarehouse { get; set; }
    public string Longitude { get; set; }
    public string Latitude { get; set; }
    public string HouseNumber { get; set; }
    public string Street { get; set; }
    public string Suburb { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string ErrorMessage { get; set; }
    public string SpecialInstructions { get; set; }
    public string QueuedEmailInfo { get; set; }
    public string AuthorisationFullName { get; set; }
    public string AuthorisationContactNumber { get; set; }
    public string AuthorisationAlternateContactNumber { get; set; }
    public string PersonalAlternateContactNumber { get; set; }
    public string AuthorisationJobTitle { get; set; }
    public string AuthorisationAdditionalComment { get; set; }
    public int ErpAccountIdForB2C { get; set; }
    public string ErpSalesOrganisationIds { get; set; }
    public string ErpAccountNumber { get; set; }
    public int ErpUserTypeId { get; set; }
    public ErpUserType ErpUserType
    {
        get => (ErpUserType)ErpUserTypeId;
        set => ErpUserTypeId = (int)value;
    }

}
