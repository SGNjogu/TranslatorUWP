using SpeechlyTouch.Core.Domain;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Interfaces
{
    public interface IMicrosoftStandardVoiceSynthesizer
    {
        Task<bool> SynthesizeText(string targetLanguageCode, string textToSynthesize, string apiKey, string apiRegion, OutputDevice outputDevice);
    }
}