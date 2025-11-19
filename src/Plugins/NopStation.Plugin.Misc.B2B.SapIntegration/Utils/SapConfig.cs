using NopStation.Plugin.Misc.B2B.SapIntegration;
using SAP.Middleware.Connector;

public class SapConfig : IDestinationConfiguration
{
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    public SapConfig(SapIntegrationSettings sapIntegrationSettings)
    {
        _sapIntegrationSettings = sapIntegrationSettings;
    }

    public bool ChangeEventsSupported()
    {
        return true;
    }

    public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

    public RfcConfigParameters GetParameters(string destinationName)
    {
        var parms = new RfcConfigParameters();
        if (destinationName.Equals("HUBCLIENT"))
        {
            parms.Add(RfcConfigParameters.AppServerHost, _sapIntegrationSettings.AppServerHost ?? "");
            parms.Add(RfcConfigParameters.SystemNumber, _sapIntegrationSettings.SystemNumber ?? "");
            parms.Add(RfcConfigParameters.SystemID, _sapIntegrationSettings.SystemID ?? "");
            parms.Add(RfcConfigParameters.User, _sapIntegrationSettings.User ?? "");
            parms.Add(RfcConfigParameters.Password, _sapIntegrationSettings.Password ?? "");
            parms.Add(RfcConfigParameters.RepositoryPassword, _sapIntegrationSettings.RepositoryPassword ?? "");
            parms.Add(RfcConfigParameters.Client, _sapIntegrationSettings.Client ?? "");
            parms.Add(RfcConfigParameters.Language, _sapIntegrationSettings.Language ?? "");
            parms.Add(RfcConfigParameters.PoolSize, _sapIntegrationSettings.PoolSize ?? "");
            parms.Add(RfcConfigParameters.AliasUser, _sapIntegrationSettings.AliasUser ?? "");
        }
        return parms;
    }
}
