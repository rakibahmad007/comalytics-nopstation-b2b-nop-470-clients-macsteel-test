using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpDeliveryDates;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories.ErpDeliveryDates;
public interface IErpDeliveyDatesModelFactory
{
    Task<ErpDeliveryDatesSearchModel> PrepareDeliveryDatesSearchModelAsync(ErpDeliveryDatesSearchModel searchModel);
    Task<ErpDeliveryDatesListModel> PrepareDeliveryDatesListsModelAsync(ErpDeliveryDatesSearchModel searchModel);
}
