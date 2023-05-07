using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.KeyVault
{
    public interface IKeyVaultService
    {
        Task<string> GetSecretAsync(string keyVautUri, string key);
        Task<IDictionary<string, string>> GetAllSecretsAsync();
    }
}
