using SpeechlyTouch.Models;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.DataSync.Interfaces
{
    public interface IPullDataService
    {
        void BeginDataSync();
        void CancelDataSync();
        Task SyncDatabase(User userSettings);
    }
}
