using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IAzureStorageService
    {
        Task<bool> CheckIfBlobExists(string blobName, string azureStorageConnectionString, string containerName);
        string GetUrl(string blobName, string azureStorageConnectionString, string containerName);
        Task<bool> UploadFile(string filePath, string fileName, string azureStorageConnectionString, string containerName);
    }
}