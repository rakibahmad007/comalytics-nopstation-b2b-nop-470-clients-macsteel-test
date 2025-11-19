using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Comalytics.DomainFilter.Domains;

namespace Nop.Plugin.Comalytics.DomainFilter.Services
{
    public class DomainFilterService : IDomainFilterService
    {
        #region Fields

        private readonly IRepository<Domain> _domainRepository;

        #endregion

        #region Ctor 

        public DomainFilterService(IRepository<Domain> domainRepository)
        {
            _domainRepository = domainRepository;
        }

        #endregion

        #region Methods

        public void DeleteDomain(Domain domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));

            _domainRepository.Delete(domain);
        }

        public bool DomainOrEmailExists(string domainOrEmailName, int typeId)
        {
            if (string.IsNullOrEmpty(domainOrEmailName))
                return false;

            var query = _domainRepository.Table;
            return query.Any(a => a.DomainOrEmailName.Equals(domainOrEmailName) && a.TypeId == typeId);
        }

        public async Task<IPagedList<Domain>> GetAllDomainsAsync(string domainOrEmailName = null, int? typeId = null, bool? overrideActive = null,
    int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _domainRepository.Table.AsQueryable(); // Ensure query is asynchronous

            if (!string.IsNullOrEmpty(domainOrEmailName))
                query = query.Where(b => b.DomainOrEmailName.Contains(domainOrEmailName));

            if (typeId != null)
                query = query.Where(b => b.TypeId == typeId);

            if (overrideActive != null)
                query = query.Where(b => b.IsActive == overrideActive);

            // Fetch the data asynchronously and use ToListAsync for async operation
            var domains = await query.ToListAsync();

            // Return the result wrapped in a PagedList
            return new PagedList<Domain>(domains, pageIndex, pageSize);
        }

        public Domain GetDomainById(int domainId)
        {
            if (domainId == 0)
                return null;

            return _domainRepository.GetById(domainId);
        }

        public void InsertDomain(Domain domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));
            //insert
            _domainRepository.Insert(domain);
        }

        public bool IsDomainOrEmailBlacklisted(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            // extract domain from email address
            var domain = email.Substring(email.LastIndexOf('@') + 1);

            // check if email address or domain is in the list and is active
            var query = _domainRepository.Table;
            var result = query.Any(x => x.IsActive && (
                                  (x.TypeId == (int)DomainType.Email && x.DomainOrEmailName.Equals(email)) ||
                                  (x.TypeId == (int)DomainType.Domain && x.DomainOrEmailName.Equals(domain))));

            return result;
        }

        public void UpdateDomain(Domain domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));

            //update
            _domainRepository.Update(domain);
        }

        #endregion
    }
}
