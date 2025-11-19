using Newtonsoft.Json.Linq;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;

public interface IWebhookAuthorizationService
{
    #region secret key

    bool ValidateWebhookAPIKey(string endpoint, string token);
    string GenereateWebhookAPIKey(string endpoint);

    #endregion

    #region bearer token

    string GenereateWebhookBearerToken();
    bool ValidateBearerToken(string endpoint);

    #endregion

    #region hmac
    string ComputeHMAC(JToken data, string key);
    bool VerifyHMAC(JToken data, string receivedHMAC, string key);

    #endregion

    #region jwt
    string GetToken(Customer customer);

    #endregion
}
