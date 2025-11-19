using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Validators;

public partial class B2BB2CRegisterValidator : BaseNopValidator<B2BRegisterModel>
{
    public B2BB2CRegisterValidator(
        ILocalizationService localizationService,
        IStateProvinceService stateProvinceService,
        CustomerSettings customerSettings
    )
    {
        //RuleFor(x => x.AccountNumber)
        //    .NotEmpty()
        //    .When(x => x.IsB2BUser)
        //    .WithMessageAwait(
        //        localizationService.GetResourceAsync(
        //            "Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.AccountNumber.Required"
        //        )
        //    );
        RuleFor(x => x.B2CIdentificationNumber)
            .NotEmpty()
            .When(x => !x.IsB2BUser)
            .WithMessageAwait(
                localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.B2CIdentificationNumber.Required"
                )
            );
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .When(x => !x.IsB2BUser)
            .WithMessageAwait(
                localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.AccountName.Required"
                )
            );

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessageAwait(
                localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.Required"
                )
            );
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessageAwait(
                localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Email.ValidEmailRequired"
                )
            );

        if (customerSettings.EnteringEmailTwice)
        {
            RuleFor(x => x.ConfirmEmail)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.ConfirmEmail.Required")
                );
            RuleFor(x => x.ConfirmEmail)
                .IsEmailAddress()
                .WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
            RuleFor(x => x.ConfirmEmail)
                .Equal(x => x.Email)
                .WithMessageAwait(
                    localizationService.GetResourceAsync(
                        "Account.Fields.Email.EnteredEmailsDoNotMatch"
                    )
                );
        }

        if (customerSettings.UsernamesEnabled)
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.Username.Required")
                );
            RuleFor(x => x.Username)
                .IsUsername(customerSettings)
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.Username.NotValid")
                );
        }

        if (customerSettings.FirstNameEnabled && customerSettings.FirstNameRequired)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync(
                        "Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.FirstName.Required"
                    )
                );
        }
        if (customerSettings.LastNameEnabled && customerSettings.LastNameRequired)
        {
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync(
                        "Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.LastName.Required"
                    )
                );
        }

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.Password.Required"))
            .IsPassword(localizationService, customerSettings);
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessageAwait(
                localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.ConfirmPassword.Required"
                )
            );
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessageAwait(
                localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.Register.Fields.Password.EnteredPasswordsDoNotMatch"
                )
            );

        RuleFor(x => x.ErpSalesOrganisationIdsArray)
            .NotEmpty()
            .WithMessageAwait(
                localizationService.GetResourceAsync(
                    "B2B.Account.Fields.ErpSalesOrganisationIdsArray.Required"
                )
            );

        //form fields
        if (customerSettings.CountryEnabled && customerSettings.CountryRequired)
        {
            RuleFor(x => x.CountryId)
                .NotEqual(0)
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.Country.Required")
                );
        }
        if (
            customerSettings.CountryEnabled
            && customerSettings.StateProvinceEnabled
            && customerSettings.StateProvinceRequired
        )
        {
            RuleFor(x => x.StateProvinceId)
                .MustAwait(
                    async (x, context) =>
                    {
                        //does selected country have states?
                        var hasStates = (
                            await stateProvinceService.GetStateProvincesByCountryIdAsync(
                                x.CountryId
                            )
                        ).Any();
                        if (hasStates)
                        {
                            //if yes, then ensure that a state is selected
                            if (x.StateProvinceId == 0)
                                return false;
                        }

                        return true;
                    }
                )
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.StateProvince.Required")
                );
        }
        if (customerSettings.DateOfBirthEnabled && customerSettings.DateOfBirthRequired)
        {
            //entered?
            RuleFor(x => x.DateOfBirthDay)
                .Must(
                    (x, context) =>
                    {
                        var dateOfBirth = x.ParseDateOfBirth();
                        if (!dateOfBirth.HasValue)
                            return false;

                        return true;
                    }
                )
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.DateOfBirth.Required")
                );

            //minimum age
            RuleFor(x => x.DateOfBirthDay)
                .Must(
                    (x, context) =>
                    {
                        var dateOfBirth = x.ParseDateOfBirth();
                        if (
                            dateOfBirth.HasValue
                            && customerSettings.DateOfBirthMinimumAge.HasValue
                            && CommonHelper.GetDifferenceInYears(dateOfBirth.Value, DateTime.Today)
                                < customerSettings.DateOfBirthMinimumAge.Value
                        )
                            return false;

                        return true;
                    }
                )
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.DateOfBirth.MinimumAge"),
                    customerSettings.DateOfBirthMinimumAge
                );
        }
        if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
        {
            RuleFor(x => x.Company)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.Company.Required")
                );
        }
        if (customerSettings.StreetAddressRequired && customerSettings.StreetAddressEnabled)
        {
            RuleFor(x => x.StreetAddress)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.StreetAddress.Required")
                );
        }
        if (customerSettings.StreetAddress2Required && customerSettings.StreetAddress2Enabled)
        {
            RuleFor(x => x.StreetAddress2)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.StreetAddress2.Required")
                );
        }
        if (customerSettings.ZipPostalCodeRequired && customerSettings.ZipPostalCodeEnabled)
        {
            RuleFor(x => x.ZipPostalCode)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.ZipPostalCode.Required")
                );
        }
        if (customerSettings.CountyRequired && customerSettings.CountyEnabled)
        {
            RuleFor(x => x.County)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.County.Required")
                );
        }
        if (customerSettings.CityRequired && customerSettings.CityEnabled)
        {
            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.City.Required")
                );
        }
        if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
        {
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.Phone.Required")
                );
        }
        if (customerSettings.PhoneEnabled)
        {
            RuleFor(x => x.Phone)
                .IsPhoneNumber(customerSettings)
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.Phone.NotValid")
                );
        }
        if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
        {
            RuleFor(x => x.Fax)
                .NotEmpty()
                .WithMessageAwait(
                    localizationService.GetResourceAsync("Account.Fields.Fax.Required")
                );
        }
        var regex = new Regex(@"^2(\d{6})$");
        RuleFor(x => x.AccountNumber)
           .NotEmpty()
           .WithMessageAwait(localizationService.GetResourceAsync("Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.AccountNumber.Required"));
        RuleFor(x => x.AccountNumber).Matches(regex).WithMessageAwait(localizationService.GetResourceAsync("Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.AccountNumber.Invalid"));

        RuleFor(x => x.JobTitle)
            .NotEmpty()
            .WithMessageAwait(
                localizationService.GetResourceAsync("Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.JobTitle.Required")
            );

        RuleFor(x => x.AuthorisationFullName)
        .NotEmpty()
        .WithMessageAwait(localizationService.GetResourceAsync("Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.AuthorisationFullName.Required"));

        RuleFor(x => x.AuthorisationContactNumber)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.AuthorisationContactNumber.Required"));

        RuleFor(x => x.AuthorisationJobTitle)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.AuthorisationJobTitle.Required"));

    }
}
