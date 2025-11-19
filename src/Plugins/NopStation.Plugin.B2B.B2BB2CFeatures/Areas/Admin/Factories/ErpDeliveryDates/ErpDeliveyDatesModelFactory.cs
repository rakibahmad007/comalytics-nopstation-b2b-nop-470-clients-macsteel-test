using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpDeliveryDates;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpDeliveyDates;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories.ErpDeliveryDates;
public class ErpDeliveyDatesModelFactory(IErpDeliveryDatesService erpDeliveryDatesService) : IErpDeliveyDatesModelFactory
{
    #region Fields

    private readonly IErpDeliveryDatesService _erpDeliveryDatesService = erpDeliveryDatesService;

    #endregion

    #region Methods
    public virtual Task<ErpDeliveryDatesSearchModel> PrepareDeliveryDatesSearchModelAsync(ErpDeliveryDatesSearchModel searchModel)
    {
        if (searchModel is null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare page parameters
        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    public async Task<ErpDeliveryDatesListModel> PrepareDeliveryDatesListsModelAsync(ErpDeliveryDatesSearchModel searchModel)
    {
        if (searchModel is null)
            throw new ArgumentNullException(nameof(searchModel));


        var deliveryDates = await _erpDeliveryDatesService.SearchDeliveryDatesAsync(searchModel.SalesOrgOrPlant,
                                                                                        searchModel.City,
                                                                                      pageIndex: searchModel.Page - 1,
                                                                                      pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new ErpDeliveryDatesListModel().PrepareToGridAsync(searchModel, deliveryDates, () =>
        {
            return deliveryDates.SelectAwait(async deliveryDates =>
            {
                //fill in model values from the entity
                var deliveryDatesModel = deliveryDates.ToModel<ErpDeliveryDatesModel>();

                deliveryDatesModel.DelDate1 = deliveryDates.DelDate1 == null ? string.Empty : deliveryDates.DelDate1?.ToShortDateString();
                deliveryDatesModel.DelDate2 = deliveryDates.DelDate2 == null ? string.Empty : deliveryDates.DelDate2?.ToShortDateString();
                deliveryDatesModel.DelDate3 = deliveryDates.DelDate3 == null ? string.Empty : deliveryDates.DelDate3?.ToShortDateString();
                deliveryDatesModel.DelDate4 = deliveryDates.DelDate4 == null ? string.Empty : deliveryDates.DelDate4?.ToShortDateString();
                deliveryDatesModel.DelDate5 = deliveryDates.DelDate5 == null ? string.Empty : deliveryDates.DelDate5?.ToShortDateString();
                deliveryDatesModel.DelDate6 = deliveryDates.DelDate6 == null ? string.Empty : deliveryDates.DelDate6?.ToShortDateString();
                deliveryDatesModel.DelDate7 = deliveryDates.DelDate7 == null ? string.Empty : deliveryDates.DelDate7?.ToShortDateString();
                deliveryDatesModel.DelDate8 = deliveryDates.DelDate8 == null ? string.Empty : deliveryDates.DelDate8?.ToShortDateString();
                deliveryDatesModel.DelDate9 = deliveryDates.DelDate9 == null ? string.Empty : deliveryDates.DelDate9?.ToShortDateString();
                deliveryDatesModel.DelDate10 = deliveryDates.DelDate10 == null ? string.Empty : deliveryDates.DelDate10?.ToShortDateString();

                return deliveryDatesModel;
            });
        });

        return model;
    }

    #endregion


}
