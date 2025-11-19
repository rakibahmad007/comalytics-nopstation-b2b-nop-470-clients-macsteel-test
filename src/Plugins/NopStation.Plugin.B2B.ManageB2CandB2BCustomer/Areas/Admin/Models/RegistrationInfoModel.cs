using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Areas.Admin.Models;
public record RegistrationInfoModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.B2B.ManageB2CAndB2BCustomer.Areas.Admin.B2CRegistrationInfo.Fields.CustomerEmail")]
    public string CustomerEmail { get; set; }

    public int NopCustomerId { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2CAndB2BCustomer.Areas.Admin.B2CRegistrationInfo.Fields.NearestWarehouse")]
    public int NearestWarehouseId { get; set; }

    public string WarehouseName { get; set; }
    public int ErpAccountId { get; set; }

    public double DistanceToNearestWarehouse { get; set; }

    public string DeliveryOption { get; set; }

    public string Longitude { get; set; }

    public string Latitude { get; set; }

    public int AddressId { get; set; }

    public string Suburb { get; set; }

    public string HouseNumber { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public string PostalCode { get; set; }

    public string Country { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2CAndB2BCustomer.Areas.Admin.B2CRegistrationInfo.Fields.B2BAccount")]
    public int B2BAccountIdForB2C { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2CAndB2BCustomer.Areas.Admin.B2CRegistrationInfo.Fields.B2BAccountNumber")]
    public string B2BAccountNumber { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2CAndB2BCustomer.Areas.Admin.B2CRegistrationInfo.Fields.B2BAccountName")]
    public string B2BAccountName { get; set; }

    public int B2CUserId { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2CAndB2BCustomer.Areas.Admin.B2CRegistrationInfo.Fields.B2BSalesOrganisation")]
    public int SalesOrganisationId { get; set; }

    //1980 || Added Job Title field
    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Account.Fields.JobTitle")]
    public string JobTitle { get; set; }

    public string ErrorMessage { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Areas.Admin.Fields.AccountNumber")]
    public string AccountNumber { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Areas.Admin.Fields.B2BSalesOrganisations")]
    public string B2BSalesOrganisationIds { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Areas.Admin.Fields.SalesOrganisationCode")]
    public string SalesOrganisationCode { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Areas.Admin.Fields.SalesOrganisationName")]
    public string SalesOrganisationName { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Areas.Admin.Fields.SpecialInstructions")]
    public string SpecialInstructions { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Areas.Customer.Fields.PersonalAlternateContactNumber")]
    public string PersonalAlternateContactNumber { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Account.Fields.AuthorisationFullName")]
    public string AuthorisationFullName { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Account.Fields.AuthorisationContactNumber")]
    public string AuthorisationContactNumber { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Account.Fields.AuthorisationAlternateContactNumber")]
    public string AuthorisationAlternateContactNumber { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Account.Fields.AuthorisationJobTitle")]
    public string AuthorisationJobTitle { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Account.Fields.AuthorisationAdditionalComment")]
    public string AuthorisationAdditionalComment { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2BCustomer.Areas.Admin.Fields.QueuedEmailInfo")]
    public string QueuedEmailInfo { get; set; }
    public bool IsB2BRegistrationInfo { get; set; }
}
