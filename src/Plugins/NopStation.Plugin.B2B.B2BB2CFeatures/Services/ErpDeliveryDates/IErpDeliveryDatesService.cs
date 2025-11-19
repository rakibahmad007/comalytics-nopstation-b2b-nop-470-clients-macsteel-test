using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpDeliveyDates;
public interface IErpDeliveryDatesService
{
    Task<IPagedList<ErpDeliveryDates>> SearchDeliveryDatesAsync(string salesOrg,
                                                            string city,
                                                          int pageIndex = 0,
                                                          int pageSize = int.MaxValue);

    Task<ErpDeliveryDateResponseModel> GetDeliveryDatesForAreaAndWarehouseAsync(ErpAccount erpAccount, string suburb, string warehosueCode);

    Task<ErpDeliveryDateResponseModel> GetDeliveryDatesForSuburbOrCityAsync(ErpAccount erpAccount, string suburbOrCity);
}
