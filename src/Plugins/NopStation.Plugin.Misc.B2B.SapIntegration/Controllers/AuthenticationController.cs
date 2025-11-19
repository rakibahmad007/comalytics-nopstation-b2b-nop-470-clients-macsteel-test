using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Misc.B2B.SapIntegration.Models.Auth;
using NopStation.Plugin.Misc.B2B.SapIntegration.Services.Auth;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Controllers;

[Route("integration")]
public class AuthenticationController : Controller
{
    #region Fields
    private readonly ICustomerRegistrationService _customerRegistrationService;
    private readonly ICustomerService _customerService;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILocalizationService _localizationService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IWorkContext _workContext;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly CustomerSettings _customerSettings;
    private readonly IIntegrationAuthService _integrationAuthService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    #endregion

    #region Ctor

    public AuthenticationController(
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IAuthenticationService authenticationService,
        ILocalizationService localizationService,
        ICustomerActivityService customerActivityService,
        IEventPublisher eventPublisher,
        IWorkContext workContext,
        IGenericAttributeService genericAttributeService,
        CustomerSettings customerSettings,
        IIntegrationAuthService integrationAuthService,
        SapIntegrationSettings sapIntegrationSettings
    )
    {
        _customerRegistrationService = customerRegistrationService;
        _customerService = customerService;
        _authenticationService = authenticationService;
        _localizationService = localizationService;
        _customerActivityService = customerActivityService;
        _eventPublisher = eventPublisher;
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
        _customerSettings = customerSettings;
        _integrationAuthService = integrationAuthService;
        _sapIntegrationSettings = sapIntegrationSettings;
    }

    #endregion

    #region Utilities

    private async Task<bool> HasAdminRoleAsync(Customer customer)
    {
        var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
        return customerRoles?.Any(x =>
                x.SystemName.Equals(NopCustomerDefaults.AdministratorsRoleName)
            ) ?? false;
    }

    #endregion

    #region Methods

    [HttpPost("login")]
    public virtual async Task<IActionResult> Login([FromBody] IntegrationLoginModel model)
    {
        var response = new GenericResponseModel<LogInResponseModel>();
        var responseData = new LogInResponseModel();

        if (string.IsNullOrWhiteSpace(_sapIntegrationSettings.IntegrationSecretKey))
            ModelState.AddModelError(
                "",
                await _localizationService.GetResourceAsync(
                    "Integration.InvalidIntegrationSecretKey"
                )
            );

        if (ModelState.IsValid)
        {
            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(
                _customerSettings.UsernamesEnabled ? model.Username : model.Email,
                model.Password
            );

            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                {
                    var customer = await _customerService.GetCustomerByEmailAsync(model.Email);
                    if (!await HasAdminRoleAsync(customer))
                    {
                        response.Message = await _localizationService.GetResourceAsync(
                            "Integration.Login.CustomerRole"
                        );
                        response.ErrorList.Add(
                            await _localizationService.GetResourceAsync(
                                "Integration.Login.Permission.Denied"
                            )
                        );
                        return StatusCode(StatusCodes.Status403Forbidden, response);
                    }

                    await _authenticationService.SignInAsync(customer, true);
                    await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));
                    await _customerActivityService.InsertActivityAsync(
                        customer,
                        "PublicStore.Login",
                        await _localizationService.GetResourceAsync(
                            "ActivityLog.PublicStore.Login"
                        ),
                        customer
                    );

                    responseData.Token = _integrationAuthService.GetToken(customer);
                    response.Data = responseData;
                    return Ok(response);
                }
                case CustomerLoginResults.CustomerNotExist:
                    ModelState.AddModelError(
                        "",
                        await _localizationService.GetResourceAsync(
                            "Account.Login.WrongCredentials.CustomerNotExist"
                        )
                    );
                    break;
                case CustomerLoginResults.Deleted:
                    ModelState.AddModelError(
                        "",
                        await _localizationService.GetResourceAsync(
                            "Account.Login.WrongCredentials.Deleted"
                        )
                    );
                    break;
                case CustomerLoginResults.NotActive:
                    ModelState.AddModelError(
                        "",
                        await _localizationService.GetResourceAsync(
                            "Account.Login.WrongCredentials.NotActive"
                        )
                    );
                    break;
                case CustomerLoginResults.NotRegistered:
                    ModelState.AddModelError(
                        "",
                        await _localizationService.GetResourceAsync(
                            "Account.Login.WrongCredentials.NotRegistered"
                        )
                    );
                    break;
                case CustomerLoginResults.LockedOut:
                    ModelState.AddModelError(
                        "",
                        await _localizationService.GetResourceAsync(
                            "Account.Login.WrongCredentials.LockedOut"
                        )
                    );
                    break;
                case CustomerLoginResults.WrongPassword:
                default:
                    ModelState.AddModelError(
                        "",
                        await _localizationService.GetResourceAsync(
                            "Account.Login.WrongCredentials"
                        )
                    );
                    break;
            }
        }

        foreach (var modelState in ModelState.Values)
            foreach (var error in modelState.Errors)
                response.ErrorList.Add(error.ErrorMessage);

        return BadRequest(response);
    }

    [HttpGet("logout")]
    public virtual async Task<IActionResult> Logout()
    {
        var response = new GenericResponseModel<CustomerInfoModel>();
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (_workContext.OriginalCustomerIfImpersonated != null)
        {
            await _customerActivityService.InsertActivityAsync(
                _workContext.OriginalCustomerIfImpersonated,
                "Impersonation.Finished",
                string.Format(
                    await _localizationService.GetResourceAsync(
                        "ActivityLog.Impersonation.Finished.StoreOwner"
                    ),
                    customer.Email,
                    customer.Id
                ),
                customer
            );

            await _customerActivityService.InsertActivityAsync(
                "Impersonation.Finished",
                string.Format(
                    await _localizationService.GetResourceAsync(
                        "ActivityLog.Impersonation.Finished.Customer"
                    ),
                    _workContext.OriginalCustomerIfImpersonated.Email,
                    _workContext.OriginalCustomerIfImpersonated.Id
                ),
                _workContext.OriginalCustomerIfImpersonated
            );

            await _genericAttributeService.SaveAttributeAsync<int?>(
                _workContext.OriginalCustomerIfImpersonated,
                NopCustomerDefaults.ImpersonatedCustomerIdAttribute,
                null
            );
        }

        await _customerActivityService.InsertActivityAsync(
            customer,
            "PublicStore.Logout",
            await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Logout"),
            customer
        );
        await _authenticationService.SignOutAsync();
        await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

        response = new GenericResponseModel<CustomerInfoModel>
        {
            Data = null,
            Message = "Logged out",
        };
        return Ok(response);
    }
    #endregion
}
