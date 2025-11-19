using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.ErpUserRegistrationInfoService;

public class ErpUserRegistrationInfoService : IErpUserRegistrationInfoService
{
    #region Fields

    private readonly IRepository<ErpUserRegistrationInfo> _erpUserRegistrationInfoRepository;

    #endregion

    #region Ctor

    public ErpUserRegistrationInfoService(IRepository<ErpUserRegistrationInfo> erpUserRegistrationInfoRepository)
    {
        _erpUserRegistrationInfoRepository = erpUserRegistrationInfoRepository;
    }

    #endregion

    #region Methods

    public async Task DeleteErpUserRegistrationInfoAsync(ErpUserRegistrationInfo erpUserRegistrationInfo)
    {
        ArgumentNullException.ThrowIfNull(erpUserRegistrationInfo);
        await _erpUserRegistrationInfoRepository.DeleteAsync(erpUserRegistrationInfo);
    }

    public async Task<ErpUserRegistrationInfo> GetErpUserRegistrationInfoByCustomerIdAsync(int customerId)
    {
        if (customerId == 0)
            return null;

        return await _erpUserRegistrationInfoRepository.Table.FirstOrDefaultAsync(x => x.NopCustomerId == customerId);
    }

    public async Task<ErpUserRegistrationInfo> GetErpUserRegistrationInfoByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _erpUserRegistrationInfoRepository.GetByIdAsync(id);
    }

    public async Task InsertErpUserRegistrationInfoAsync(ErpUserRegistrationInfo erpUserRegistrationInfo)
    {
        ArgumentNullException.ThrowIfNull(erpUserRegistrationInfo);

        await _erpUserRegistrationInfoRepository.InsertAsync(erpUserRegistrationInfo);
    }

    public async Task UpdateErpUserRegistrationInfoAsync(ErpUserRegistrationInfo erpUserRegistrationInfo)
    {
        ArgumentNullException.ThrowIfNull(erpUserRegistrationInfo);

        await _erpUserRegistrationInfoRepository.UpdateAsync(erpUserRegistrationInfo);
    }

    #endregion
}
