using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models.Auth;

public class IntegrationLoginModel
{
    [DataType(DataType.EmailAddress)]
    [NopResourceDisplayName("Account.Login.Fields.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Account.Login.Fields.Username")]
    public string Username { get; set; }

    [DataType(DataType.Password)]
    [NoTrim]
    [NopResourceDisplayName("Account.Login.Fields.Password")]
    public string Password { get; set; }
}
