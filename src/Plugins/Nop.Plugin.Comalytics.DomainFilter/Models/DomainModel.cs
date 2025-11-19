using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Comalytics.DomainFilter.Models
{
    public record DomainModel : BaseNopEntityModel
    {
        public DomainModel()
        {
            AvailableTypeOptions = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableTypeOptions { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.DomainFilter.Domain.DomainOrEmailName")]
        public string DomainOrEmailName { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.DomainFilter.Domain.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.DomainFilter.Domain.Type")]
        public string Type { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.DomainFilter.Domain.Type")]
        public int TypeId { get; set; }
    }
}
