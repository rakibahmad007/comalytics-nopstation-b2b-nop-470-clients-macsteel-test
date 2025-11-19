using Nop.Core;

namespace Nop.Plugin.Comalytics.DomainFilter.Domains
{
    public class Domain : BaseEntity
    {
        public string DomainOrEmailName { get; set; }
        public bool IsActive { get; set; }
        public DomainType Type
        {
            get => (DomainType)TypeId;
            set => TypeId = (int)value;
        }
        public int TypeId { get; set; }
    }
}
