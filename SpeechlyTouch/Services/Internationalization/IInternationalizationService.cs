using System.Threading.Tasks;

namespace SpeechlyTouch.Services.Internationalization
{
    public interface IInternationalizationService
    {
        Task GetInternationalizationLanguages();
        void LoadApplicationLanguage();
        void SetAppLanguage(string languageCode);
    }
}
