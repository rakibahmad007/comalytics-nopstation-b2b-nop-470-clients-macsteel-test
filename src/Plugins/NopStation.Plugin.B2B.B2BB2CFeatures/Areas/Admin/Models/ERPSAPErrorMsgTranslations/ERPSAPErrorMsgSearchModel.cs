using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ERPSAPErrorMsgTranslations;

public record ERPSAPErrorMsgSearchModel : ErpBaseSearchModel
{
    public ERPSAPErrorMsgSearchModel()
    {
        AddB2BSAPErrorMsgTranslationModel = new ERPSAPErrorMsgTranslationModel();
    }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.Fields.SearchErrorMsg")]
    public string SearchErrorMsg { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.Fields.SearchUserTranslation")]
    public string SearchUserTranslation { get; set; }

    public ERPSAPErrorMsgTranslationModel AddB2BSAPErrorMsgTranslationModel { get; set; }
}
