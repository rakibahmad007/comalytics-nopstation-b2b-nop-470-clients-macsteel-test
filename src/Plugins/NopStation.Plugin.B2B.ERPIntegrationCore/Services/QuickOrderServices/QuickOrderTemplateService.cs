using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices;

public class QuickOrderTemplateService : IQuickOrderTemplateService
{
    private readonly IRepository<QuickOrderTemplate> _quickOrderTemplateRepository;

    public QuickOrderTemplateService(IRepository<QuickOrderTemplate> quickOrderTemplateRepository)
    {
        _quickOrderTemplateRepository = quickOrderTemplateRepository;
    }

    public async Task<IPagedList<QuickOrderTemplate>> GetAllQuickOrderTemplatesAsync(string name = null, 
        int customerId = 0, 
        DateTime? createdOnUtc = null,
        int pageIndex = 0, 
        int pageSize = int.MaxValue)
    {
        var query = _quickOrderTemplateRepository.Table;

        if (customerId > 0)
            query = query.Where(q => q.CustomerId == customerId);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(q => q.Name.Contains(name));

        if (createdOnUtc.HasValue)
        {
            query = query.Where(q => q.CreatedOnUtc >= createdOnUtc.Value);
            query = query.Where(q => q.CreatedOnUtc <= createdOnUtc.Value.AddDays(1));
        }

        query = query.Where(q => !q.Deleted).OrderByDescending(q => q.Id);

        var result = query.ToPagedListAsync(pageIndex, pageSize);

        return await result;
    }

    public async Task<IList<QuickOrderTemplate>> GetAllQuickOrderTemplatesByCustomerIdAsync(int customerId = 0)
    {
        var query = _quickOrderTemplateRepository.Table;

        if (customerId > 0)
            query = query.Where(q => q.CustomerId == customerId);

        return await query.Where(q => !q.Deleted).OrderByDescending(q => q.Id).ToListAsync();
    }

    public async Task<QuickOrderTemplate> GetQuickOrderTemplateByIdAsync(int templateId)
    {
        if (templateId == 0)
            return null;

        return await _quickOrderTemplateRepository.GetByIdAsync(templateId);
    }

    public async Task<QuickOrderTemplate> GetQuickOrderTemplateByIdWithoutTrackingAsync(int templateId)
    {
        if (templateId == 0)
            return null;

        return await _quickOrderTemplateRepository.Table.Where(x => x.Id == templateId).FirstOrDefaultAsync();
    }

    public async Task InsertQuickOrderTemplateAsync(QuickOrderTemplate quickOrderTemplate)
    {
        if (quickOrderTemplate == null)
            return;
        await _quickOrderTemplateRepository.InsertAsync(quickOrderTemplate);
    }

    public async Task InsertQuickOrderTemplatesAsync(List<QuickOrderTemplate> quickOrderTemplates)
    {
        if (quickOrderTemplates == null || quickOrderTemplates.Count == 0)
            return;
        await _quickOrderTemplateRepository.InsertAsync(quickOrderTemplates);
    }

    public async Task UpdateQuickOrderTemplateAsync(QuickOrderTemplate quickOrderTemplate)
    {
        if (quickOrderTemplate == null)
            return;
        await _quickOrderTemplateRepository.UpdateAsync(quickOrderTemplate);
    }

    public async Task UpdateQuickOrderTemplatesAsync(List<QuickOrderTemplate> quickOrderTemplates)
    {
        if (quickOrderTemplates == null || quickOrderTemplates.Count == 0)
            return;
        await _quickOrderTemplateRepository.UpdateAsync(quickOrderTemplates);
    }

    public async Task DeleteQuickOrderTemplateAsync(QuickOrderTemplate quickOrderTemplate)
    {
        if (quickOrderTemplate == null)
            return;
        await _quickOrderTemplateRepository.DeleteAsync(quickOrderTemplate);
    }

    public async Task DeleteQuickOrderTemplatesAsync(List<QuickOrderTemplate> quickOrderTemplates)
    {
        if (quickOrderTemplates == null || quickOrderTemplates.Count == 0)
            return;
        await _quickOrderTemplateRepository.DeleteAsync(quickOrderTemplates);
    }
}
