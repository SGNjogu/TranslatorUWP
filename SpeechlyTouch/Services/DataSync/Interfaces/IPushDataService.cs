using System.Threading.Tasks;

namespace SpeechlyTouch.Services.DataSync.Interfaces
{
    public interface IPushDataService
    {
        void BeginDataSync();
        void BeginPlaybackUsageSync();
        void CancelDataSync();
        Task<Core.Domain.Session> UploadSession(DataService.Models.Session session);
    }
}
