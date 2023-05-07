using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IBackendLanguageService
    {
        Task<List<BackendLanguage>> GetUserLanguages(int? userId, string token);
    }
}