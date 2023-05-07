using System.Threading.Tasks;
using SpeechlyTouch.Models;

namespace SpeechlyTouch.Services.Versioning
{
    public interface IAppVersionService
    {
        Task<AppVersion> FetchAppVersion();
        AppVersion CurrentVersion { get; }
    }
}
