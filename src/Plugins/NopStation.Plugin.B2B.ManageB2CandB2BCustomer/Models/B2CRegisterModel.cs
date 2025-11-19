using System.Reflection.Metadata.Ecma335;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;
public record B2CRegisterModel : B2BRegisterModel
{

    [NopResourceDisplayName("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Latitude")]
    public string Latitude { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Longitude")]
    public string Longitude { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.HouseNumber")]
    public string HouseNumber { get; set; }

    [NopResourceDisplayName("Plugins.B2B.ManageB2CAndB2BCustomer.Account.Fields.Street")]
    public string Street { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Suburb")]
    public string Suburb { get; set; }

    [NopResourceDisplayName("Account.Fields.City")]
    public string CityName { get; set; }

    [NopResourceDisplayName("Account.Fields.StateProvince")]
    public string StateProvince { get; set; }
    public string StateProvinceCode { get; set; }

    [NopResourceDisplayName("Account.Fields.Country")]
    public string Country { get; set; }
    public string CountryCode { get; set; }

    [NopResourceDisplayName("Account.Fields.ZipPostalCode")]
    public string PostalCode { get; set; }

    public bool ShowNoShopZoneAddressPopup { get; set; }
}
