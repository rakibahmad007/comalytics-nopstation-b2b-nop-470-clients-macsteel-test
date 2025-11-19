using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Customer;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Validators;
public class OverridenChangePasswordValidator : AbstractValidator<ChangePasswordModel>
{
    public OverridenChangePasswordValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
    {
        RuleFor(x => x.OldPassword).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.ChangePassword.Fields.OldPassword.Required"));
        RuleFor(x => x.NewPassword)
          .NotEmpty()
          .WithMessageAwait(localizationService.GetResourceAsync("Account.ChangePassword.Fields.NewPassword.Required"))
          .IsPassword(localizationService, customerSettings);
        RuleFor(x => x.ConfirmNewPassword).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.ChangePassword.Fields.ConfirmNewPassword.Required"));
        RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessageAwait(localizationService.GetResourceAsync("Account.ChangePassword.Fields.NewPassword.EnteredPasswordsDoNotMatch"));
    }
}
