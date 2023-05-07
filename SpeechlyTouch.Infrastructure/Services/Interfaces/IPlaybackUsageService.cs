using SpeechlyTouch.Core.Domain;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IPlaybackUsageService
    {
        Task<PlaybackUsageTracking> Create(PlaybackUsageTracking playbackUsage, string token);
    }
}