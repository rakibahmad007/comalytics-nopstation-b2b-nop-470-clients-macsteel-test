using FluentValidation;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Validators;
public class CheckoutErpShippingAddressModelValidator : BaseNopValidator<CheckoutErpShippingAddressModel>
{
    public CheckoutErpShippingAddressModelValidator(
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IWorkContext workContext,
        ILocalizationService localizationService)
    {
        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(90)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payment.B2BCustomerAccount.B2BCheckout.Fields.SpecialInstructions.MaximumLength"));

        RuleFor(x => x.ErpShipToAddressId).MustAwait(async (x, context) =>
        {
            return !(await erpCustomerFunctionalityService.IsCustomerInB2BCustomerRoleAsync(await workContext.GetCurrentCustomerAsync()) &&
                !x.PickupInStoreOnly &&
                x.ErpShipToAddressId < 1);
        }).WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payment.B2BCustomerAccount.B2BCheckout.Fields.B2BShipToAddress.Required"));

        if (b2BB2CFeaturesSettings.PreventSpecialCharactersInCustomerReference)
        {
            var specialCharacters = b2BB2CFeaturesSettings.SpecialCharactersToPreventInCustomerReference.Trim();
            if (string.IsNullOrEmpty(specialCharacters) || specialCharacters.ToLower() == "all")
            {
                RuleFor(x => x.CustomerReference).Matches("^[a-zA-Z0-9\x20]+$").WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payment.B2BCustomerAccount.B2BCheckout.Fields.CustomerReferenceQO.Invalid"));
            }
            else
            {
                specialCharacters = specialCharacters.Replace("\"", "/\\\"");
                RuleFor(x => x.CustomerReference).Matches("^[^" + specialCharacters + "\r\n]+$").WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payment.B2BCustomerAccount.B2BCheckout.Fields.CustomerReferenceQO.Invalid"));
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
        }).WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payment.B2BCustomerAccount.B2BCheckout.Fields.CustomerReferenceQO.AlreadyExist"));
    }
}
