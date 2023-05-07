using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface ITranscriptionService
    {
        Task<Transcription> Create(Transcription transcription, string token);
        Task<IEnumerable<Transcription>> GetTranscriptionsForSession(int sessionId, string token);
        Task<List<Transcription>> CreateBulk(BulkTranscriptions transcriptions, string token);
    }
}