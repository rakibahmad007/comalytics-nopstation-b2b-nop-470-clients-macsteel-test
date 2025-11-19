using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Configuration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpLogsService : IErpLogsService
{
    #region Fields

    private readonly IRepository<ErpLogs> _erpLogsRepository;
    private readonly IWebHelper _webHelper;
    private readonly IRepository<Customer> _customerRepository;
    private readonly ERPIntegrationCoreSettings _eRPIntegrationCoreSettings;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public ErpLogsService(IRepository<ErpLogs> erpLogsRepository,
        IWebHelper webHelper,
        IRepository<Customer> customerRepository,
        ERPIntegrationCoreSettings eRPIntegrationCoreSettings,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _erpLogsRepository = erpLogsRepository;
        _webHelper = webHelper;
        _customerRepository = customerRepository;
        _eRPIntegrationCoreSettings = eRPIntegrationCoreSettings;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Gets a value indicating whether this message should not be logged
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    private bool IgnoreLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        return false;
    }

    private async Task<bool> IsLogEnabled(ErpLogLevel logLevel)
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var erpIntegrationCoreSettings = await _settingService.LoadSettingAsync<ERPIntegrationCoreSettings>(storeScope);
        if (logLevel == ErpLogLevel.Debug && erpIntegrationCoreSettings.ShowDebugLog || logLevel != ErpLogLevel.Debug)
            return true;
        return false;
    }

    public async Task ClearLogAsync(DateTime? olderThan = null)
    {
        if (olderThan == null)
            await _erpLogsRepository.TruncateAsync();
        else
            await _erpLogsRepository.DeleteAsync(p => p.CreatedOnUtc < olderThan.Value);
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task<ErpLogs> InsertErpLogAsync(ErpLogLevel logLevel, 
        ErpSyncLevel syncLevel, 
        string shortMessage, 
        string fullMessage = "", 
        Customer customer = null)
    {
        if (!await IsLogEnabled(logLevel) || IgnoreLog(shortMessage) || IgnoreLog(fullMessage))
            return null;

        var log = new ErpLogs
        {
            ErpLogLevelId = (int)logLevel,
            ShortMessage = shortMessage,
            FullMessage = fullMessage,
            ErpSyncLevelId = (int)syncLevel,
            IpAddress = _webHelper.GetCurrentIpAddress(),
            CustomerId = customer?.Id,
            PageUrl = _webHelper.GetThisPageUrl(true),
            ReferrerUrl = _webHelper.GetUrlReferrer() ?? string.Empty,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _erpLogsRepository.InsertAsync(log, false);

        return log;
    }

    public async Task InformationAsync(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null)
    {
        //don't log thread abort exception
        if (exception is System.Threading.ThreadAbortException)
            return;

        await InsertErpLogAsync(ErpLogLevel.Information, syncLevel, message, $"{exception}", customer);
    }

    public async Task WarningAsync(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null)
    {
        //don't log thread abort exception
        if (exception is System.Threading.ThreadAbortException)
            return;

        await InsertErpLogAsync(ErpLogLevel.Warning, syncLevel, message, $"{exception}", customer);
    }

    public async Task ErrorAsync(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null)
    {
        //don't log thread abort exception
        if (exception is System.Threading.ThreadAbortException)
            return;

        await InsertErpLogAsync(ErpLogLevel.Error, syncLevel, message, $"{exception}", customer);
    }

    #endregion

    #region Delete

    private async Task DeleteErpLogAsync(ErpLogs erpLog)
    {
        await _erpLogsRepository.DeleteAsync(erpLog);
    }

    public async Task DeleteErpLogByIdAsync(int id)
    {
        var erpLog = await GetErpLogByIdAsync(id);
        if (erpLog != null)
        {
            await DeleteErpLogAsync(erpLog);
        }
    }

    public async Task DeleteErpLogsAsync(IList<ErpLogs> erpLogs)
    {
        await _erpLogsRepository.DeleteAsync(erpLogs, false);
    }

    #endregion

    #region Read

    public async Task<ErpLogs> GetErpLogByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _erpLogsRepository.GetByIdAsync(id, cache => default);
    }

    public async Task<IPagedList<ErpLogs>> GetAllErpLogsAsync(string ipAddress, 
        string message, 
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool getOnlyTotalCount = false, 
        int logLevelId = 0, 
        int syncLevelId = 0, 
        string nopCustomerEmail = null, 
        DateTime? createdFrom = null, 
        DateTime? createdTo = null)
    {
        var erpLogs = await _erpLogsRepository.GetAllPagedAsync(query =>
        {
            if (createdFrom != null && createdFrom.HasValue)
                query = query.Where(w => w.CreatedOnUtc >= createdFrom.Value);

            if (createdTo != null && createdTo.HasValue)
                query = query.Where(w => w.CreatedOnUtc <= createdTo.Value);

            if (!string.IsNullOrWhiteSpace(message))
            {
                query = query.Where(x => x.ShortMessage.Contains(message) || x.FullMessage.Contains(message));
            }

            if (!string.IsNullOrEmpty(ipAddress))
                query = query.Where(x => x.IpAddress.Contains(ipAddress));

            if (logLevelId > 0)
                query = query.Where(x => x.ErpLogLevelId.Equals(logLevelId));

            if (syncLevelId > 0)
                query = query.Where(x => x.ErpSyncLevelId.Equals(syncLevelId));

            if (!string.IsNullOrEmpty(nopCustomerEmail))
            {
                query = query.Join(_customerRepository.Table, x => x.CustomerId, y => y.Id,
                        (x, y) => new { ErpActivityLogs = x, Customer = y })
                    .Where(z => z.Customer.Email.Contains(nopCustomerEmail))
                    .Select(z => z.ErpActivityLogs)
                    .Distinct();
            }

            query = query.OrderByDescending(ei => ei.Id);
            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpLogs;
    }

    public async Task<IList<ErpLogs>> GetErpLogsByIdsAsync(int[] erpLogIds)
    {
        return await _erpLogsRepository.GetByIdsAsync(erpLogIds);
    }

    #endregion

    #endregion
}
