using System.Threading.Tasks;

namespace SpeechlyTouch.Services.CognitiveService
{
    public interface ICognitiveServicesHelper
    {
        Task DeleteCognitiveServicesEndpointId();
        Task<CognitiveEndpoint> GetAccessKeyAndRegionAsync();
    }
}
