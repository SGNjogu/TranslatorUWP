using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.KeyVault
{
    public class KeyVaultDataService : IKeyVaultDataService
    {
        public IDataService _dataService;

        public KeyVaultDataService(IDataService dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// Gets KeyVaultSecrets
        /// </summary>
        /// <returns></returns>
        public async Task<AzureKeyVaultSecrets> Get()
        {
            AzureKeyVaultSecrets secrets = await _dataService.GetOneKeyVaultSecretAsync(1);
            return secrets;
        }

        /// <summary>
        /// Adds a new KeyVault secrets
        /// Only add when secrets don't exist
        /// </summary>
        /// <returns></returns>
        public async Task Create(AzureKeyVaultSecrets secrets)
        {
            await _dataService.AddItemAsync<AzureKeyVaultSecrets>(secrets);
        }

        /// <summary>
        /// Updates KeyVault secrets
        /// </summary>
        /// <param name="setting"></param>
        public async Task Update(AzureKeyVaultSecrets secrets)
        {
            try
            {
                await _dataService.UpdateItemAsync<AzureKeyVaultSecrets>(secrets);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
