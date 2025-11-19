namespace NopStation.Plugin.Misc.B2B.SapIntegration;
public static class SapIntegrationDefaults
{
    public static string HideGeneralBlock => "SapIntegrationPage.HideGeneralBlock";
    public static int DefaultTimeOutPeriod => 1800;
    public static int AccountNoLengthLimit => 20;
    public static string SapProductPublishedStatus => "Yes";

    #region JWT

    public static readonly string Token = "Authorization";
    public static readonly string SecretKey = "SecretKey";
    public static readonly string CustomerId = "CustomerId";

    #endregion
}
