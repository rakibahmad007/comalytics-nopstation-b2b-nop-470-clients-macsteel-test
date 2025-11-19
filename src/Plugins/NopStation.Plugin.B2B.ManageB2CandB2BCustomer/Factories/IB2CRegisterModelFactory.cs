using System.Threading.Tasks;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Factories;

public interface IB2CRegisterModelFactory
{
    Task<B2CRegisterModel> PrepareB2CRegisterModelAsync(B2CRegisterModel model,
                                                        bool excludeProperties,
                                                        string overrideCustomCustomerAttributesXml = "",
                                                        bool setDefaultValues = false);

    Task PrepareModelAsync(ConfigurationModel model);

    Task<B2CRegisterResultModel> PrepareB2CRegisterResultModelAsync(int resultId);
}

