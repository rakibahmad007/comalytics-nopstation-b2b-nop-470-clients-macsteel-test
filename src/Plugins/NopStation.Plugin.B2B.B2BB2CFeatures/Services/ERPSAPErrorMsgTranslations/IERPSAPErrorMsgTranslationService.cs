using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ERPSAPErrorMsgTranslations
{
    public interface IERPSAPErrorMsgTranslationService
    {
        Task<ERPSAPErrorMsgTranslation> GetErrorMsgByIdAsync(int errorMsgId);

        Task<IPagedList<ERPSAPErrorMsgTranslation>> GetAllERPSAPErrorMsgTranslationAsync(
        string errorMsg = null,
        string userTranslation = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);
        /// <summary>
        /// Insert a ERPSAPErrorMsgTranslation
        /// </summary>
        /// <param name="b2BUserRegistrationInfo">B2BUserRegistrationInfo</param>
        Task InsertERPSAPErrorMsgTranslationAsync(ERPSAPErrorMsgTranslation b2BSAPErrorMsgTranslation);

        /// <summary>
        /// Update a ERPSAPErrorMsgTranslation
        /// </summary>
        /// <param name="b2BUserRegistrationInfo">B2BUserRegistrationInfo</param>
        Task UpdateERPSAPErrorMsgTranslationAsync(ERPSAPErrorMsgTranslation b2BSAPErrorMsgTranslation);

        /// <summary>
        /// Delete a ERPSAPErrorMsgTranslation
        /// </summary>
        /// <param name="b2BUserRegistrationInfo">B2BUserRegistrationInfo</param>
        Task DeleteERPSAPErrorMsgTranslationAsync(ERPSAPErrorMsgTranslation b2BSAPErrorMsgTranslation);

        Task<IList<ERPSAPErrorMsgTranslation>> GetAllERPSAPErrorMsgTranslationAsync();

        Task<ERPSAPErrorMsgTranslation> GetErrorMsgTranslationAsync(string errorMsg);

        //string GetTranslatedAndCompleteIntegrationErrorMsg(B2BOrderPerAccount b2BOrderPerAccount);
        //string GetTranslatedAndCompleteIntegrationErrorMsgForB2COrder(B2COrderPerUser b2COrderPer); 

        Task<string> GetTranslatedAndCompleteIntegrationErrorMsgAsync(ErpOrderType b2BOrderType, string integrationError);
    }
}
