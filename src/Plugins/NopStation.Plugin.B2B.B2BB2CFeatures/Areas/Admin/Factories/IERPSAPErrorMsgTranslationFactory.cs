using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ERPSAPErrorMsgTranslations;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public interface IERPSAPErrorMsgTranslationFactory
    {
        Task<ERPSAPErrorMsgSearchModel> PrepareERPSAPErrorMsgSearchModelAsync(ERPSAPErrorMsgSearchModel searchModel);

        Task<ERPSAPErrorMsgListModel> PrepareERPSAPErrorMsgListModelAsync(ERPSAPErrorMsgSearchModel searchModel);
    }
}
