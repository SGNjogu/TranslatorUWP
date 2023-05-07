using SpeechlyTouch.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.FlagLanguage
{
    public interface IFlagLanguageService
    {
        Task<List<LanguageFlag>> GetFlagLanguages();
    }
}