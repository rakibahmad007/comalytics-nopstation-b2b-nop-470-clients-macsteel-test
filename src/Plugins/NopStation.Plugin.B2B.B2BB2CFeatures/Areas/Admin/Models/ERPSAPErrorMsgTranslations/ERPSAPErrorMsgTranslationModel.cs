using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ERPSAPErrorMsgTranslations
{
    public record ERPSAPErrorMsgTranslationModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ERPSAPErrorMsgTranslationModel.Fields.ErrorMsg")]
        public string ErrorMsg { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ERPSAPErrorMsgTranslationModel.Fields.UserTranslation")]
        public string UserTranslation { get; set; }

    }
}
