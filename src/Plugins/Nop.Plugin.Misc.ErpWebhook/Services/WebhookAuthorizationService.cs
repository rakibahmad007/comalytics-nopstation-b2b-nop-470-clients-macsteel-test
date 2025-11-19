using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.ErpWebhook.Extensions;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.ErpWebhook.Services
{
    public class WebhookAuthorizationService : IWebhookAuthorizationService
    {
        #region fields

        private readonly IEncryptionService _encryptionService;
        private readonly ErpWebhookSettings _erpWebhookSettings;

        #endregion

        #region ctor

        public WebhookAuthorizationService(IEncryptionService encryptionService,
            ErpWebhookSettings erpWebhookSettings)
        {
            _encryptionService = encryptionService;
            _erpWebhookSettings = erpWebhookSettings;
        }

        #endregion

        #region const

        public static class Constants
        {
            public static string LicenseKeySeed = "22cerfdZX8Uq9LrLHHhYssVD";
        }

        #endregion

        #region webhook api key

        public bool ValidateWebhookAPIKey(string endpoint, string token)
        {
            try
            {
                var data = Convert.FromBase64String(token);
                var decodedString = Encoding.UTF8.GetString(data);
                var decryptedToken = _encryptionService.DecryptText(decodedString, Constants.LicenseKeySeed);
                return (endpoint == decryptedToken);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string GenereateWebhookAPIKey(string endpoint)
        {
            try
            {
                var encryptedToken = _encryptionService.EncryptText(endpoint, Constants.LicenseKeySeed);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedToken));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region bearer token

        public string GenereateWebhookBearerToken()
        {
            try
            {
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Constants.LicenseKeySeed)))
                {
                    byte[] tokenBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(Constants.LicenseKeySeed));
                    return BitConverter.ToString(tokenBytes, 0, 16).Replace("-", "").ToLower();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool ValidateBearerToken(string token)
        {
            try
            {
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Constants.LicenseKeySeed)))
                {
                    byte[] expectedTokenBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(Constants.LicenseKeySeed));
                    string expectedToken = BitConverter.ToString(expectedTokenBytes, 0, 16).Replace("-", "").ToLower();

                    return token == expectedToken;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region HMAC

        public string ComputeHMAC(JToken data, string key)
        {
            // Convert JToken to its JSON representation
            string jsonData = data.ToString();

            // Convert key and data to byte arrays
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);

            // Create HMACSHA256 instance with the key
            using (HMACSHA256 hmacSha256 = new HMACSHA256(keyBytes))
            {
                // Compute the hash value for the data
                byte[] hashBytes = hmacSha256.ComputeHash(dataBytes);

                // Convert the hash to a hexadecimal string
                string hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return hashHex;
            }
        }

        public bool VerifyHMAC(JToken data, string receivedHMAC, string key)
        {
            // Compute HMAC for the received data using the same key
            string computedHMAC = ComputeHMAC(data, key);

            // Compare the computed HMAC with the received HMAC
            return string.Equals(receivedHMAC, computedHMAC, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region jwt

        public string GetToken(Customer customer)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now = Math.Round((DateTime.UtcNow.AddDays(180) - unixEpoch).TotalSeconds);
            var expiration = now + (30 * 60); // Set expiration to 30 minutes from now

            var payload = new Dictionary<string, object>()
                {
                    { ErpWebhookDefaults.CustomerId, customer.Id },
                    {"createdon",now },
                    { "exp", expiration },
                };

            return JwtHelper.JwtEncoder.Encode(payload, _erpWebhookSettings.WebhookSecretKey);
        }

        #endregion
    }
}
