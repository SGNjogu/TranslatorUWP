using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Interfaces
{
    public interface IMicrosoftTextToTextTranslator
    {
        Task<string> TranslateTextToText(string apiKey, string apiRegion, string sourceLanguageCode, string textToTranslate, string targetLanguageCode);
    }
}