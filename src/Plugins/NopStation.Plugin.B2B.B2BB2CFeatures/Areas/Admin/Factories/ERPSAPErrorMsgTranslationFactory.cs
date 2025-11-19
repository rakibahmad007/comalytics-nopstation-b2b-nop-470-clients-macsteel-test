using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ERPSAPErrorMsgTranslations;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ERPSAPErrorMsgTranslations;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public class ERPSAPErrorMsgTranslationFactory : IERPSAPErrorMsgTranslationFactory
    {
        private readonly IERPSAPErrorMsgTranslationService _erpSAPErrorMsgTranslationService;

        public ERPSAPErrorMsgTranslationFactory(IERPSAPErrorMsgTranslationService erpSAPErrorMsgTranslationService)
        {
            _erpSAPErrorMsgTranslationService = erpSAPErrorMsgTranslationService;

        }

        public async Task<ERPSAPErrorMsgListModel> PrepareERPSAPErrorMsgListModelAsync(ERPSAPErrorMsgSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var errorMsgReps = await _erpSAPErrorMsgTranslationService.GetAllERPSAPErrorMsgTranslationAsync(
                searchModel.SearchErrorMsg,
                searchModel.SearchUserTranslation,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = await new ERPSAPErrorMsgListModel().PrepareToGridAsync(searchModel, errorMsgReps, () =>
            {
                return errorMsgReps.SelectAwait(async user =>
                {
                    var errorMsg = await _erpSAPErrorMsgTranslationService.GetErrorMsgByIdAsync(user.Id);

                    var repModel = new ERPSAPErrorMsgTranslationModel
                    {
                        Id = user.Id,
                        ErrorMsg = user.ErrorMsg,
                        UserTranslation = user.UserTranslation
                    };

                    return repModel;
                });
            });

            return model;
        }

        public async Task<ERPSAPErrorMsgSearchModel> PrepareERPSAPErrorMsgSearchModelAsync(ERPSAPErrorMsgSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();
            return await Task.FromResult(searchModel);
        }

    }
}
