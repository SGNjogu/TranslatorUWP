using Microsoft.CognitiveServices.Speech;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.AudioOutput;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders
{
    public class MicrosoftStandardVoiceSynthesizer : IMicrosoftStandardVoiceSynthesizer
    {
        private string _apiKey;
        private string _region;
        private IAudioOutputService audioOutputService;

        private SpeechSynthesizer SetupStandardVoiceSynthesizer(string targetLanguageCode)
        {
            var config = SpeechConfig.FromSubscription(_apiKey, _region);
            config.SpeechSynthesisLanguage = targetLanguageCode;
            config.SetProfanity(ProfanityOption.Raw);

            return new SpeechSynthesizer(config, null);
        }

        public async Task<bool> SynthesizeText(string targetLanguageCode, string textToSynthesize, string apiKey, string apiRegion, OutputDevice outputDevice)
        {
            audioOutputService = new AudioOutputService();
            audioOutputService.Initialize(outputDevice);

            if (!string.IsNullOrEmpty(apiKey)) _apiKey = apiKey;
            if (!string.IsNullOrEmpty(apiRegion)) _region = apiRegion;

            try
            {
                var autoSynthesizer = SetupStandardVoiceSynthesizer(targetLanguageCode);
                var synthesisResult = await autoSynthesizer.SpeakTextAsync(textToSynthesize);

                if (synthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    var audioResult = synthesisResult.AudioData;
                    audioOutputService.Play(audioResult);
                    return true;
                }
                else if (synthesisResult.Reason == ResultReason.Canceled)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Standard Synthesizer Error: {ex.Message}");
            }
            return false;
        }
    }
}
