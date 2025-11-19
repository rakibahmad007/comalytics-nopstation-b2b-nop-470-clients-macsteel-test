using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services.Auth;

public interface IIntegrationAuthService
{
    string GetToken(Customer customer);
}
