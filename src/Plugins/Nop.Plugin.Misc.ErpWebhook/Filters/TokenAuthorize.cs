using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.ErpWebhook.Domain.Enums;
using Nop.Plugin.Misc.ErpWebhook.Extensions;
using Nop.Plugin.Misc.ErpWebhook.Models.Common;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.ErpWebhook.Filters;

public class TokenAuthorizeAttribute : TypeFilterAttribute
{
    #region Ctor

    public TokenAuthorizeAttribute() : base(typeof(TokenAuthorizeAttributeFilter))
    {

    }

    #endregion

    #region Nested class

    public class TokenAuthorizeAttributeFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext actionContext)
        {
            var result = ParseAuthorizationHeader(actionContext);
            if (result != JWTAuthResult.Success)
            {
                Challenge(actionContext, result);
                return;
            }
        }

        protected virtual JWTAuthResult ParseAuthorizationHeader(AuthorizationFilterContext actionContext)
        {
            if (actionContext.HttpContext.Request.Headers.TryGetValue(ErpWebhookDefaults.Token, out var checkToken))
            {
                var erpWebhookSettings = EngineContext.Current.Resolve<ErpWebhookSettings>();
                var token = checkToken.FirstOrDefault();
                if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
                {
                    token = token.Substring("Bearer ".Length);
                }
                var secretKey = erpWebhookSettings.WebhookSecretKey;
                try
                {
                    var payload = JwtHelper.JwtDecoder.DecodeToObject(token, secretKey, true) as IDictionary<string, object>;

                    var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var now = Math.Round((DateTime.UtcNow.AddDays(180) - unixEpoch).TotalSeconds);
                    payload.TryGetValue("createdon", out var createdon);
                    payload.TryGetValue("exp", out var exp);

                    if (now >= (double)createdon && now <= (double)exp)
                        return JWTAuthResult.Success;

                    return JWTAuthResult.Expired;
                }
                catch (Exception e)
                {
                    return JWTAuthResult.Invalid;
                }
            }

            return JWTAuthResult.Invalid;
        }

        private void Challenge(AuthorizationFilterContext actionContext, JWTAuthResult result)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var response = new BaseResponseModel
            {
                ErrorList = new List<string>
                {
                    result == JWTAuthResult.Invalid ? localizationService.GetResourceAsync("Erpwebhook.Response.InvalidToken").Result :
                    localizationService.GetResourceAsync("Erpwebhook.Response.TokenExpired").Result
                }
            };

            actionContext.Result = new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };

            return;
        }
    }

    #endregion
}