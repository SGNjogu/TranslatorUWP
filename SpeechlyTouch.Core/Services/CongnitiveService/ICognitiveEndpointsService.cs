using SpeechlyTouch.Core.Domain;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.CongnitiveService
{
    public interface ICognitiveEndpointsService
    {
        Task<CognitiveServiceEndpoint> AllocateCognitiveEndpointId(string token);
        Task<bool> DeallocateCognitiveEndpointId(string endpointId, string token);
    }
}