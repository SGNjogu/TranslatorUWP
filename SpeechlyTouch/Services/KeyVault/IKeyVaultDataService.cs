using SpeechlyTouch.DataService.Models;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.KeyVault
{
    public interface IKeyVaultDataService
    {
        Task Update(AzureKeyVaultSecrets secrets);
        Task Create(AzureKeyVaultSecrets secrets);
        Task<AzureKeyVaultSecrets> Get();
    }
}
