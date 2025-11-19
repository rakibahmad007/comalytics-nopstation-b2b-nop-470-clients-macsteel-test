using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.ErpWebhook.Services
{
    public class LicenseService : ILicenseService
    {
        #region fields

        private readonly IEncryptionService _encryptionService;
        private readonly ErpWebhookSettings _erpWebhookSettings;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        #endregion

        #region prop

        public int Order { get { return 1; } }

        #endregion

        #region ctor

        public LicenseService(
            IEncryptionService encryptionService,
            ErpWebhookSettings erpWebhookSettings,
            ISettingService settingService,
            ILogger logger
            )
        {
            _encryptionService = encryptionService;
            _erpWebhookSettings = erpWebhookSettings;
            _settingService = settingService;
            _logger = logger;
        }

        #endregion

        #region const

        public static class Constants
        {
            public static string LicenseKeySeed = "22cerfdZX8Uq9LrLHHhYssVD";
        }

        #endregion

        #region innerclass

        private class DecryptedLicense
        {
            public int[] NopVersion { get; set; }
            public string Domain { get; set; }
            public bool IncludesSubdomains { get; set; }
        }

        #endregion

        #region methods

        #region plugin license

        private DecryptedLicense DecryptProductKey(string productKey, string encryptionKey)
        {
            try
            {
                string decryptedText = _encryptionService.DecryptText(productKey, encryptionKey);

                IDictionary<string, string> decryptedKey = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedText);
                if (decryptedKey == null)
                {
                    return null;
                }

                DecryptedLicense result = new DecryptedLicense();

                // NOP version
                string nopVersion = null;
                if (decryptedKey.TryGetValue("NOPVersion", out nopVersion))
                {
                    result.NopVersion = ExtractVersionComponents(nopVersion);
                }
                else
                {
                    result.NopVersion = null;
                }

                // Domain
                if (!decryptedKey.ContainsKey("Domain") || string.IsNullOrWhiteSpace(decryptedKey["Domain"]))
                {
                    return null;
                }

                result.Domain = decryptedKey["Domain"];

                // IncludesSubdomains
                result.IncludesSubdomains =
                    decryptedKey.ContainsKey("IncludesSubdomains")
                    && (decryptedKey["IncludesSubdomains"].ToLower() == "true");

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                //do log 
            }
            return null;
        }

        private int[] ExtractVersionComponents(string version)
        {
            if (version == null)
                return null;
            string[] parts = version.Split('.');
            int major = 0, minor = 0;
            try
            {
                major = Convert.ToInt32(parts[0]);
            }
            catch { }
            try
            {
                // Round down minor version. Eg 3.91 should match 3.9
                minor = (int)(Convert.ToInt32(parts[1]) / Math.Pow(10, parts[1].Length - 1));
            }
            catch { }
            return new int[] { major, minor };
        }

        public ProductKeyVerificationResult VerifyProductKey(string key, string host, string nopVersion)
        {
            try
            {
                // Normalize host name
                //host = new Uri(host).Host;

                var decryptedKey = DecryptProductKey(key, Constants.LicenseKeySeed);
                if (decryptedKey == null)
                {
                    return ProductKeyVerificationResult.InvalidProductKey;
                }

                if (decryptedKey.NopVersion != null)
                {
                    // Check NOP version if present
                    int[] currentVersion = ExtractVersionComponents(nopVersion);
                    if (currentVersion[0] != decryptedKey.NopVersion[0] || currentVersion[1] != decryptedKey.NopVersion[1])
                    {
                        return ProductKeyVerificationResult.InvalidForNOPVersion;
                    }
                }

                // Check domain
                if (
                    host != decryptedKey.Domain &&
                    host != ("www." + decryptedKey.Domain) &&
                    ("www." + host) != decryptedKey.Domain
                  )
                {
                    // Check as well if it's not a valid subdomain
                    if (
                        !host.EndsWith("." + decryptedKey.Domain) ||
                        !decryptedKey.IncludesSubdomains
                      )
                    {
                        return ProductKeyVerificationResult.InvalidForDomain;
                    }
                }
                return ProductKeyVerificationResult.Valid;
            }
            catch (Exception ex)
            {
                //do log
            }
            return ProductKeyVerificationResult.InvalidProductKey;
        }


        #endregion

        #region License

        //public bool IsLicensed(string host, string nopVersion)
        //{

        //    var key = _erpWebhookSettings.LicenseString;
        //    if (!string.IsNullOrEmpty(key) && VerifyProductKey(key, host, nopVersion) == ProductKeyVerificationResult.Valid) // Need to remove || true in production 
        //    {
        //        return true;
        //    }


        //    // give trial access 
        //    if (!string.IsNullOrEmpty(_erpWebhookSettings.InstalledDateEncriptionString))
        //    {

        //        var installedOn = Convert.ToDateTime(_encryptionService.DecryptText(_erpWebhookSettings.InstalledDateEncriptionString));

        //        if (installedOn != null && installedOn.AddDays(ErpWebhookDefaults.TrailPeriodsInDays) > (DateTime.Now))
        //        {
        //            return true;
        //        }

        //    }

        //    return false;
        //}

        //public bool AddProductKey(string key)
        //{
        //    if (DecryptProductKey(key, Constants.LicenseKeySeed) == null)
        //    {
        //        return false;
        //    }

        //    _erpWebhookSettings.LicenseString = key;
        //    _settingService.SaveSetting(_erpWebhookSettings);
        //    return true;
        //}

        #endregion

        #endregion

    }
}
