using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Services.Customers;
using Nop.Services.Plugins;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public partial class ErpIntegrationPluginManager : PluginManager<IErpIntegrationPlugin>, IErpIntegrationPluginManager
{ 
    #region Fields

    private readonly ERPIntegrationCoreSettings _settings;

    #endregion

    #region Ctor

    public ErpIntegrationPluginManager(ICustomerService customerService,
        IPluginService pluginService,
        ERPIntegrationCoreSettings settings) : base(customerService, pluginService)
    {
        _settings = settings;
    }

    #endregion

    #region Methods

    public virtual bool IsPluginActive(IErpIntegrationPlugin IntegrationMethod)
    {
        if(string.IsNullOrEmpty(_settings.SelectedErpIntegrationPlugin))
            return false;
        return IsPluginActive(IntegrationMethod, new List<string> { _settings.SelectedErpIntegrationPlugin });
    }

    public virtual async Task<IErpIntegrationPlugin> LoadActiveERPIntegrationPlugin()
    {
        if (string.IsNullOrEmpty(_settings.SelectedErpIntegrationPlugin))
            return null;

      return  await LoadPluginBySystemNameAsync(_settings.SelectedErpIntegrationPlugin);  
    }

    #endregion
}