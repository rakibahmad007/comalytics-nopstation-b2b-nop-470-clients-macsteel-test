using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Customer;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Validators;
public class OverridenPasswordRecoveryConfirmValidator: AbstractValidator<PasswordRecoveryConfirmModel>
{
    public OverridenPasswordRecoveryConfirmValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
    {
        RuleFor(x => x.NewPassword)
           .NotEmpty()
           .WithMessageAwait(localizationService.GetResourceAsync("Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.Password.Required"))
           .IsPassword(localizationService, customerSettings);
        RuleFor(x => x.ConfirmNewPassword).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.PasswordRecovery.ConfirmNewPassword.Required"));
        RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessageAwait(localizationService.GetResourceAsync("Account.PasswordRecovery.NewPassword.EnteredPasswordsDoNotMatch"));
    }
}
