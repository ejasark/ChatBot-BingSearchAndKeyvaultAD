using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;

namespace EmptyBot1
{
    public class keyVault
    {
        public string Message { get; set; }
        public async Task GetAccessTokenAsync()
        {
            var clientID = ConfigurationManager.AppSettings["AzureAADClientID"];
            var clientSecret = ConfigurationManager.AppSettings["AzureAADClientSecret"];

            //Azure Key Vault object
            var keyClient = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var adCredential = new ClientCredential(clientID, clientSecret);
                var authenticationContext = new AuthenticationContext(authority);
                var resultAccessToken = await authenticationContext.AcquireTokenAsync(resource, adCredential);
                return resultAccessToken.AccessToken;
            });

            await GetSecretKeyFromVault(keyClient); //Gets the secret key value from the vault

            //await InsertKeysInKeyVault(keyClient); //Inserts the secret key value to the vault
        }
        private async Task GetSecretKeyFromVault(KeyVaultClient keyVaultClient)
        {
            // Get the Secret key details of SQL connection string
            var keyVaultIdentifierHelper = new KeyVaultIdentifierHelper(ConfigurationManager.AppSettings["KeyVaultUrl"]); //Get the Secret Vault URL
            var connectionStringIdentifier =
                keyVaultIdentifierHelper.GetSecretIdentifier(ConfigurationManager.AppSettings["BingSearchKey"]); //Our example is using secret identifier
            var getResultSecret = await keyVaultClient.GetSecretAsync(connectionStringIdentifier);
            Message =  getResultSecret.Value;
        }

        private async Task InsertKeysInKeyVault(KeyVaultClient keyVaultClient)
        {
            var keyVaultIdentifierHelper = ConfigurationManager.AppSettings["KeyVaultUrl"]; //Get the Secret Vault URL

            var getResultSecret = await keyVaultClient.SetSecretAsync(keyVaultIdentifierHelper, "AvinashSecKeyName", "AvinashSecKeyValue");

            Message = getResultSecret.Value;
        }

        public class KeyVaultIdentifierHelper
        {
            private const string KeyFormat = "{0}/keys/{1}";
            private const string SecretFormat = "{0}/secrets/{1}";
            private readonly string keyVaultUrl;

            public KeyVaultIdentifierHelper(string keyVaultUrl)
            {
                this.keyVaultUrl = keyVaultUrl;
            }

            public string GetKeyIdentifier(string keyName)
            {
                return string.Format(KeyFormat, this.keyVaultUrl, keyName);
            }

            public string GetSecretIdentifier(string secretName)
            {
                return string.Format(SecretFormat, this.keyVaultUrl, secretName);
            }
        }
    }
}
