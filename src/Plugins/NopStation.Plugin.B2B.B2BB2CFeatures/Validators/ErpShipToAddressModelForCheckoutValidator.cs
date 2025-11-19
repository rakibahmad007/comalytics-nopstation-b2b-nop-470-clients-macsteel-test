using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Validators;
public class ErpShipToAddressModelForCheckoutValidator : BaseNopValidator<ErpShipToAddressModelForCheckout>
{
    public ErpShipToAddressModelForCheckoutValidator(
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IWorkContext workContext,
        ILocalizationService localizationService)
    {


        RuleFor(model => model.ShipToName)
           .NotEmpty()
           .MaximumLength(45)
           .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.ShipToName.MaximumLength"));

        RuleFor(x => x.Address1)
            .MaximumLength(60)
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.Address1.MaximumLength"));

        RuleFor(x => x.Address2)
            .MaximumLength(60)
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.Address2.MaximumLength"));

        RuleFor(x => x.Suburb)
            .MaximumLength(40)
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.Suburb.MaximumLength"));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30)
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.PhoneNumber.MaximumLength"));

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(90)
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.SpecialInstructions.MaximumLength"));

        RuleFor(x => x.ZipPostalCode)
            .NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.ZipPostalCode.NotNull"))
            .Matches(@"(?!0000)\d{4}")
            .WithMessageAwait(localizationService.GetResourceAsync("B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.ZipPostalCode.ExactLength.IsNumber"));


        if (b2BB2CFeaturesSettings.PreventSpecialCharactersInCustomerReference)
        {
            var specialCharacters = b2BB2CFeaturesSettings.SpecialCharactersToPreventInCustomerReference.Trim();
            if (string.IsNullOrEmpty(specialCharacters) || specialCharacters.ToLower() == "all")
            {
                RuleFor(x => x.CustomerReference)
                    .Matches("^[a-zA-Z0-9\x20]+$")
                    .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payment.B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.CustomerReferenceQO.Invalid"));
            }
            else
            {
                specialCharacters = specialCharacters.Replace("\"", "/\\\"");
                RuleFor(x => x.CustomerReference)
                    .Matches("^[^" + specialCharacters + "\r\n]+$")
                    .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payment.B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.CustomerReferenceQO.Invalid"));
            }
        }
        RuleFor(x => x.CustomerReference).MustAwait(async (x, context) =>
        {
            if (b2BB2CFeaturesSettings.MaintainUniqueCustomerReference)
            {
                if (await erpOrderAdditionalDataService.IfCustomerReferenceExistWithThisErpAccount(x.CustomerReference, x.ErpAccountId))
                {
                    return false;
                }
            }
            return true;
        }).WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payment.B2BB2CFeatures.ErpShipToAddressModelForCheckOut.Fields.CustomerReferenceQO.AlreadyExist"));
    }
}
