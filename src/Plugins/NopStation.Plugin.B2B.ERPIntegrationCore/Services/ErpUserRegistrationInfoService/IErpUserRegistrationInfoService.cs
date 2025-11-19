using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.ErpUserRegistrationInfoService;

public interface IErpUserRegistrationInfoService
{
    Task InsertErpUserRegistrationInfoAsync(ErpUserRegistrationInfo b2CRegistrationInfo);
    Task UpdateErpUserRegistrationInfoAsync(ErpUserRegistrationInfo b2CRegistrationInfo);
    Task DeleteErpUserRegistrationInfoAsync(ErpUserRegistrationInfo b2CRegistrationInfo);
    Task<ErpUserRegistrationInfo> GetErpUserRegistrationInfoByIdAsync(int id);
    Task<ErpUserRegistrationInfo> GetErpUserRegistrationInfoByCustomerIdAsync(int customerId);
}
