using FluentValidation;
using Nop.Plugin.Comalytics.DomainFilter.Domains;
using Nop.Plugin.Comalytics.DomainFilter.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Comalytics.DomainFilter.Validators
{
    public class DomainValidator : BaseNopValidator<DomainModel>
    {
        public DomainValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DomainOrEmailName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.Domain.DomainOrEmailName.Required"));
            RuleFor(model => model.TypeId)
                .Must(x => x.Equals((int)DomainType.Domain) || x.Equals((int)DomainType.Email))
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.Domain.Type.Required.DomainOrEmail"));
        }
    }
}
