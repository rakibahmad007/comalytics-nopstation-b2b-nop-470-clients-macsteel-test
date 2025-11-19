using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Misc.ErpWebhook.Filters;

/// <summary>
/// Represents filter attribute that validates IP address
/// </summary>
public class ValidateWebhookManagerIpAddressAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    public ValidateWebhookManagerIpAddressAttribute()
        : base(typeof(ValidateIpAddressFilter))
    { }

    #endregion

    #region Nested filter

    /// <summary>
    /// Represents a filter that validates IP address
    /// </summary>
    private class ValidateIpAddressFilter : IActionFilter
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IAllowedWebhookManagerIpAddressesService _allowedWebhookManagerIpAddressesService;
        private readonly IErpLogsService _erpLogsService;

        #endregion

        #region Ctor

        public ValidateIpAddressFilter(
            IWebHelper webHelper,
            IAllowedWebhookManagerIpAddressesService allowedWebhookManagerIpAddressesService,
            IErpLogsService erpLogsService
        )
        {
            _webHelper = webHelper;
            _allowedWebhookManagerIpAddressesService = allowedWebhookManagerIpAddressesService;
            _erpLogsService = erpLogsService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called before the action executes, after model binding is complete
        /// </summary>
        /// <param name="context">A context for action filters</param>
        public async void OnActionExecuting(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.HttpContext.Request == null)
                return;

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            // Get action and controller names
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var actionName = actionDescriptor?.ActionName;
            var controllerName = actionDescriptor?.ControllerName;

            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                return;

            // Get allowed IP addresses (await the async method)
            var ipAddresses =
                await _allowedWebhookManagerIpAddressesService.GetAllowedWebhookManagerIpAddressesAsync();

            // There are no restrictions
            if (ipAddresses == null || !ipAddresses.Any())
                return;

            // Whether current IP is allowed
            var currentIp = _webHelper.GetCurrentIpAddress();

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.LoginLogout,
                $"Someone trying to log on to access webhook from {currentIp} address"
            );

            if (
                ipAddresses.Any(ip =>
                    ip.IpAddress.Equals(currentIp, StringComparison.InvariantCultureIgnoreCase)
                )
            )
                return;

            // Ensure that it's not 'Access denied' page
            if (
                !(
                    controllerName.Equals(
                        "Authentication",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                    && actionName.Equals(
                        "IpNotAllowed",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
            )
            {
                // Redirect to 'Access denied' page
                context.Result = new RedirectToActionResult(
                    "IpNotAllowed",
                    "Authentication",
                    context.RouteData.Values
                );
            }
        }

        /// <summary>
        /// Called after the action executes, before the action result
        /// </summary>
        /// <param name="context">A context for action filters</param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //do nothing
        }

        #endregion
    }

    #endregion
}
