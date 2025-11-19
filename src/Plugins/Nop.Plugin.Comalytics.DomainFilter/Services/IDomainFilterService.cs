using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Comalytics.DomainFilter.Domains;

namespace Nop.Plugin.Comalytics.DomainFilter.Services
{
    public interface IDomainFilterService
    {
        void DeleteDomain(Domain domain);
        bool DomainOrEmailExists(string domainOrEmailName, int typeId);
        Task<IPagedList<Domain>> GetAllDomainsAsync(string domainOrEmailName = null, int? typeId = null, bool? overrideActive = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Domain GetDomainById(int domainId);
        void InsertDomain(Domain domain);
        bool IsDomainOrEmailBlacklisted(string domainOrEmailName);
        void UpdateDomain(Domain domain);
    }
}
