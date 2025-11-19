using Nop.Core;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Domain
{
    public class AdditionalCategoryInfoData : BaseEntity
    {
        public int CategoryId { get; set; }

        public bool Active { get; set; }

        public string AdditionalInfoField { get; set; }
    }
}
