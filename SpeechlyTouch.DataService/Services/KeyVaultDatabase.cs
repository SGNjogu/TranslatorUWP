using SpeechlyTouch.DataService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService
    {
        /// <summary>
        /// Method to get all Settingss
        /// </summary>
        /// <returns>All Settingss</returns>
        public async Task<List<AzureKeyVaultSecrets>> GetKeyVaultKeysAsync()
        {
            return await Dataservice.Table<AzureKeyVaultSecrets>().ToListAsync();
        }

        /// <summary>
        /// Method to get one Settings
        /// </summary>
        /// <returns>Single Settings</returns>
        public async Task<AzureKeyVaultSecrets> GetOneKeyVaultSecretAsync(int secretId)
        {
            return await Dataservice.Table<AzureKeyVaultSecrets>().Where(s => s.ID == secretId).FirstOrDefaultAsync();
        }
    }
}
