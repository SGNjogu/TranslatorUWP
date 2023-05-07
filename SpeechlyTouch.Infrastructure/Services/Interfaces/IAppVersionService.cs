using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IAppVersionService
    {
        Task<IEnumerable<AppVersion>> GetAppVersions(int appType, string token);
    }
}