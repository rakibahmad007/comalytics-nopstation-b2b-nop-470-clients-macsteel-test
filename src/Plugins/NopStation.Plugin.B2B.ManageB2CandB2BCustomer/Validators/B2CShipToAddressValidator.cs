using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Validators;

public partial class B2CShipToAddressValidator : BaseNopValidator<B2CShipToAddressModel>
{
    public B2CShipToAddressValidator(ILocalizationService localizationService,
        IStateProvinceService stateProvinceService,
        CustomerSettings customerSettings)
    {

        RuleFor(x => x.Latitude).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Latitude.Required"));
        RuleFor(x => x.Longitude).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Longitude.Required"));
        RuleFor(x => x.HouseNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.HouseNumber.Required"));
        RuleFor(x => x.Street).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Street.Required"));
        RuleFor(x => x.Suburb).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Suburb.Required"));
        RuleFor(x => x.City).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.City.Required"));
        RuleFor(x => x.StateProvince).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.StateProvince.Required"));
        RuleFor(x => x.PostalCode).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.PostalCode.Required"));

        RuleFor(x => x.Country).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Country.Required"));
        RuleFor(x => x.Country).Must(c => !string.IsNullOrEmpty(c) && c.ToLower().Trim() == "south africa")
            .WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Country.MustBe.SouthAfrica"));
    }
}
