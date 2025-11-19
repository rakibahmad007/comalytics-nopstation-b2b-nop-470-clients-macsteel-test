using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Services.Localization;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ERPSAPErrorMsgTranslations;

public class ERPSAPErrorMsgTranslationService : IERPSAPErrorMsgTranslationService
{
    private readonly IRepository<ERPSAPErrorMsgTranslation> _erpSAPErrorMsgTranslationRepository;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;

    public ERPSAPErrorMsgTranslationService(IRepository<ERPSAPErrorMsgTranslation> erpSAPErrorMsgTranslationRepository,
        IWorkContext workContext,
        ILocalizationService localizationService)
    {
        _erpSAPErrorMsgTranslationRepository = erpSAPErrorMsgTranslationRepository;
        _workContext = workContext;
        _localizationService = localizationService;
    }

    public async Task<IPagedList<ERPSAPErrorMsgTranslation>> GetAllERPSAPErrorMsgTranslationAsync(
    string errorMsg = null,
    string userTranslation = null,
    int pageIndex = 0,
    int pageSize = int.MaxValue)
    {
        var query = _erpSAPErrorMsgTranslationRepository.Table;

        if (!string.IsNullOrEmpty(errorMsg))
            query = query.Where(b => b.ErrorMsg.Contains(errorMsg));

        if (!string.IsNullOrEmpty(userTranslation))
            query = query.Where(b => b.UserTranslation.Contains(userTranslation));

        // Materialize the query to a list
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task InsertERPSAPErrorMsgTranslationAsync(ERPSAPErrorMsgTranslation b2BSAPErrorMsgTranslation)
    {
        if (b2BSAPErrorMsgTranslation == null)
            throw new ArgumentNullException(nameof(b2BSAPErrorMsgTranslation));

        await _erpSAPErrorMsgTranslationRepository.InsertAsync(b2BSAPErrorMsgTranslation);
    }

    public async Task UpdateERPSAPErrorMsgTranslationAsync(ERPSAPErrorMsgTranslation b2BSAPErrorMsgTranslation)
    {
        if (b2BSAPErrorMsgTranslation == null)
            throw new ArgumentNullException(nameof(b2BSAPErrorMsgTranslation));

        await _erpSAPErrorMsgTranslationRepository.UpdateAsync(b2BSAPErrorMsgTranslation);
    }

    public async Task DeleteERPSAPErrorMsgTranslationAsync(ERPSAPErrorMsgTranslation b2BSAPErrorMsgTranslation)
    {
        if (b2BSAPErrorMsgTranslation == null)
            throw new ArgumentNullException(nameof(b2BSAPErrorMsgTranslation));

        await _erpSAPErrorMsgTranslationRepository.DeleteAsync(b2BSAPErrorMsgTranslation);
    }

    public async Task<ERPSAPErrorMsgTranslation> GetErrorMsgByIdAsync(int errorMsgId)
    {
        if (errorMsgId == 0)
            return null;

        return await _erpSAPErrorMsgTranslationRepository.GetByIdAsync(errorMsgId);
    }

    public async Task<ERPSAPErrorMsgTranslation> GetErrorMsgTranslationAsync(string errorMsg)
    {
        if (string.IsNullOrEmpty(errorMsg))
            return null;

        return await Task.FromResult(_erpSAPErrorMsgTranslationRepository.Table.FirstOrDefault(x => x.ErrorMsg.Equals(errorMsg)));
    }

    public async Task<IList<ERPSAPErrorMsgTranslation>> GetAllERPSAPErrorMsgTranslationAsync()
    {
        var query = _erpSAPErrorMsgTranslationRepository.Table;
        query = query.OrderByDescending(b => b.Id);

        return await Task.FromResult(query.ToList());
    }

    public async Task<string> GetTranslatedAndCompleteIntegrationErrorMsgAsync(ErpOrderType b2BOrderType, string integrationError)
    {
        var orderTypeString = (b2BOrderType == ErpOrderType.B2BSalesOrder || b2BOrderType == ErpOrderType.B2CSalesOrder)
            ? "Order"
            : "Quote";

        var defaultMsgString = await _localizationService.GetResourceAsync(
            "NopStation.Plugin.B2B.B2BB2CFeatures.PlaceB2BOrder.DefaultIntegrationErrorMessage",
            languageId: (await _workContext.GetWorkingLanguageAsync()).Id,
            defaultValue: "Your {0} is not placed at ERP due to an error {1}. No action required."
        );

        var translatedMsg = await GetErrorMsgTranslationAsync(integrationError);
        integrationError = translatedMsg != null ? translatedMsg.UserTranslation : integrationError;

        return string.Format(defaultMsgString, orderTypeString, integrationError);
    }
}
