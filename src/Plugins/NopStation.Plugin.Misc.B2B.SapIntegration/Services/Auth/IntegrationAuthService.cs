using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.B2B.SapIntegration.Extensions;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services.Auth;

public class IntegrationAuthService : IIntegrationAuthService
{
    #region Fields

    private readonly SapIntegrationSettings _settings;

    #endregion

    #region Ctor

    public IntegrationAuthService(SapIntegrationSettings settings)
    {
        _settings = settings;
    }

    #endregion

    #region Mehods

    public string GetToken(Customer customer)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var now = Math.Round((DateTime.UtcNow.AddDays(180) - unixEpoch).TotalSeconds);
        var expiration = now + 30 * 60;

        var payload = new Dictionary<string, object>()
        {
            { SapIntegrationDefaults.CustomerId, customer.Id },
            { "createdon", now },
            { "exp", expiration },
        };

        return JwtHelper.JwtEncoder.Encode(
            payload,
            _settings.IntegrationSecretKey
        );
    }
    #endregion
}