using System.Text.RegularExpressions;
using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Validators;
public partial class B2CRegisterValidator : BaseNopValidator<B2CRegisterModel>
{
    public B2CRegisterValidator(ILocalizationService localizationService,
        IStateProvinceService stateProvinceService,
        CustomerSettings customerSettings)
    {
        //for b2b user
        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .When(x => x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.AccountNumber.Required"));
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .When(x => x.IsB2BUser)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.AccountName.Required"));

        ////for b2c user
        //RuleFor(x => x.B2CIdentificationNumber)
        //    .NotEmpty()
        //    .When(x => !x.IsB2BUser)
        //    .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.B2CIdentificationNumber.Required"));
        //RuleFor(x => x.AccountName)
        //    .NotEmpty()
        //    .When(x => !x.IsB2BUser)
        //    .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.AccountName.Required"));
        //RuleFor(x => x.ZipPostalCode)
        //    .NotEmpty()
        //    .When(x => !x.IsB2BUser)
        //    .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.ZipPostalCodeRequired"));
        //RuleFor(x => x.Phone)
        //    .NotEmpty()
        //    .When(x => !x.IsB2BUser)
        //    .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.PhoneRequired"));
        //RuleFor(x => x.StreetAddress)
        //    .NotEmpty()
        //    .When(x => !x.IsB2BUser)
        //    .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.StreetAddressRequired"));
        //RuleFor(x => x.StreetAddress2)
        //    .NotEmpty()
        //    .When(x => !x.IsB2BUser)
        //    .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.StreetAddress2Required"));
        //RuleFor(x => x.CountryId)
        //    .GreaterThan(0)
        //    .When(x => !x.IsB2BUser)
        //    .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Country.Required"));
        //RuleFor(x => x.City)
        //    .NotEmpty()
        //    .When(x => !x.IsB2BUser)
        //    .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.City.Required"));

        RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.Required"));
        RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.ValidEmailRequired"));

        if (customerSettings.EnteringEmailTwice)
        {
            RuleFor(x => x.ConfirmEmail).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.ConfirmEmail.Required"));
            RuleFor(x => x.ConfirmEmail).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.ValidEmailRequired"));
            RuleFor(x => x.ConfirmEmail).Equal(x => x.Email).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Email.EnteredEmailsDoNotMatch"));
        }

        if (customerSettings.FirstNameEnabled && customerSettings.FirstNameRequired)
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.FirstName.Required"));
        }
        if (customerSettings.LastNameEnabled && customerSettings.LastNameRequired)
        {
            RuleFor(x => x.LastName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.LastName.Required"));
        }
        if (customerSettings.DateOfBirthEnabled && customerSettings.DateOfBirthRequired)
        {
            //entered?
            RuleFor(x => x.DateOfBirthDay).Must((x, context) =>
            {
                var dateOfBirth = x.ParseDateOfBirth();
                if (!dateOfBirth.HasValue)
                    return false;

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.DateOfBirthDay.Required"));
        }

        //Password rule

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.Password.Required"))
            .IsPassword(localizationService, customerSettings);
        RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.ConfirmPassword.Required"));
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Password.EnteredPasswordsDoNotMatch"));

        //form fields
        //if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
        //{
        //    RuleFor(x => x.Company).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Company.Required"));
        //}
        var phoneNumberValidatorRegex = new Regex(@"(^(\+27|0)[0-9]{9}$)|(^[0-9]{9}$)");
        if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
        {
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.Required"))
                .Matches(phoneNumberValidatorRegex).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.PhoneNumber.Invalid"));
        }
        if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
        {
            RuleFor(x => x.Fax).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Fax.Required"));
        }

        //if (customerSettings.CountryEnabled &&
        //    customerSettings.StateProvinceEnabled &&
        //    customerSettings.StateProvinceRequired)
        //{
        //    RuleFor(x => x.StateProvinceId).MustAwait(async (x, context) =>
        //    {
        //        //does selected country have states?
        //        var hasStates = (await stateProvinceService.GetStateProvincesByCountryIdAsync(x.CountryId)).Any();
        //        if (hasStates)
        //        {
        //            //if yes, then ensure that a state is selected
        //            if (x.StateProvinceId == 0)
        //                return false;
        //        }

        //        return true;
        //    }).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.StateProvince.Required"));
        //}
        //if (customerSettings.CountyRequired && customerSettings.CountyEnabled)
        //{
        //    RuleFor(x => x.County)
        //        .NotEmpty()
        //        .WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.County.Required"));
        //}

        RuleFor(x => x.Latitude).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Latitude.Required"));
        RuleFor(x => x.Longitude).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Longitude.Required"));
        RuleFor(x => x.HouseNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.HouseNumber.Required"));
        RuleFor(x => x.Street).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Street.Required"));
        RuleFor(x => x.Suburb).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Suburb.Required"));
        RuleFor(x => x.CityName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.City.Required"));
        RuleFor(x => x.StateProvince).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.StateProvince.Required"));
        RuleFor(x => x.PostalCode).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.PostalCode.Required"));

        RuleFor(x => x.Country).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Country.Required"));
        RuleFor(x => x.Country).Must(c => !string.IsNullOrEmpty(c) && c.ToLower().Trim() == "south africa")
            .WithMessageAwait(localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Country.MustBe.SouthAfrica"));
    }
}
