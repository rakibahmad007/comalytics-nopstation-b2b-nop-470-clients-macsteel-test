using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.B2CMacsteelExpressShops;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Validators.B2CMacsteelExpressShops;
public partial class B2CMacsteelExpressShopValidator : BaseNopValidator<B2CMacsteelExpressShopModel>
{
    public B2CMacsteelExpressShopValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.MacsteelExpressShopName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Fields.MacsteelExpressShopName.Required"));

        RuleFor(model => model.MacsteelExpressShopCode)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Fields.MacsteelExpressShopCode.Required"));


        SetDatabaseValidationRules<B2CMacsteelExpressShop>();
    }
}
