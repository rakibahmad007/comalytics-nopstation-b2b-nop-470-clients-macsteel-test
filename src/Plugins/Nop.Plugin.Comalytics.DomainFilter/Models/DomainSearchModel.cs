using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Comalytics.DomainFilter.Models
{
    public record DomainSearchModel : BaseSearchModel
    {
        public DomainSearchModel()
        {
            AvailableActiveOptions = new List<SelectListItem>();
            AvailableTypeOptions = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableActiveOptions { get; set; }
        public IList<SelectListItem> AvailableTypeOptions { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive")]
        public int SearchActiveId { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType")]
        public int SearchTypeId { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchDomainOrEmailName")]
        public string SearchDomainOrEmailName { get; set; }
    }
}
