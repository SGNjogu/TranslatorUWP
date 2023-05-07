using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using SpeechlyTouch.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.KeyVault
{
    public class KeyVaultService : IKeyVaultService
    {
        private readonly KeyVaultClient keyVaultClient;
        private readonly ICrashlytics _crashlytics;

        public KeyVaultService(ICrashlytics crashlytics)
        {
            _crashlytics = crashlytics;
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={Constants.VaultAppId};TenantId={Constants.TenantId};AppKey={Constants.KeygenClientSecret}");
                keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetSecretAsync(string keyVaultUri, string key)
        {
            try
            {
                var uri = $"{keyVaultUri}/secrets/{key}";
                var secret = await keyVaultClient.GetSecretAsync(uri);
                return secret.Value;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IDictionary<string, string>> GetAllSecretsAsync()
        {
            try
            {
                var secrets = new Dictionary<string, string>();
                var response = await keyVaultClient.GetSecretsAsync(Constants.KeyVaultUri);

                foreach (var item in response)
                {
                    var secret = await GetSecretAsync(Constants.KeyVaultUri, item.Identifier.Name);
                    secrets[item.Identifier.Name] = secret;
                }

                return secrets;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
