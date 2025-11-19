namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;

public enum ProductKeyVerificationResult
{
    InvalidProductKey,
    InvalidForDomain,
    InvalidForNOPVersion,
    Valid
}

public interface ILicenseService
{
    ProductKeyVerificationResult VerifyProductKey(string key, string host, string nopVersion);

    #region License

    //bool IsLicensed(string hostUrl, string nopVersion);
    //bool AddProductKey(string key);

    #endregion
}
