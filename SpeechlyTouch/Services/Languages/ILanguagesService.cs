
using SpeechlyTouch.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.Languages
{
    public interface ILanguagesService
    {
        Task<List<Language>> GetSupportedLanguagesAsync();
    }
}
