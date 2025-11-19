using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nop.Plugin.Comalytics.DomainFilter.Domains;
using Nop.Plugin.Comalytics.DomainFilter.Models;

namespace Nop.Plugin.Comalytics.DomainFilter.Factories
{
    public interface IDomainModelFactory
    {
        Task<byte[]> ExportDomainsToXlsx(DomainSearchModel searchModel);
        Task<byte[]> ExportDomainsToXlsx(List<int> ids);
        Task ImportDomainListFromXlsx(Stream stream);
        Task<DomainListModel> PrepareDomainListModelAsync(DomainSearchModel domainSearchModel);
        Task<DomainModel> PrepareDomainModelAsync(DomainModel domainModel, Domain domain);
        Task<DomainSearchModel> PrepareDomainSearchModelAsync(DomainSearchModel domainSearchModel);
    }
}
