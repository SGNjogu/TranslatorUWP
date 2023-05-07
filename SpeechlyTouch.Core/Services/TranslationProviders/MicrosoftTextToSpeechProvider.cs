using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using SpeechlyTouch.Core.Services.TranslationProviders.Utils;
using System.Threading.Tasks;
using System.Threading;
using SpeechlyTouch.Core.Domain;
using System.Linq;
using Microsoft.CognitiveServices.Speech;
using SpeechlyTouch.Core.DTO;

namespace SpeechlyTouch.Core.Services.TranslationProviders
{
    public class MicrosoftTextToSpeechProvider : IMicrosoftTextToSpeechProvider
    {
        private string _apiKey;
        private string _region;

        private Guid Guid { get; set; }
        private Participant _participant { get; set; }

        public event Action<TranslationResult> TranscriptionResultReady;
        public event Action<TranslationResult> TranslationSpeechReady;

        private readonly IMicrosoftTextToTextTranslator _microsoftTextToTextTranslator;

        public MicrosoftTextToSpeechProvider(IMicrosoftTextToTextTranslator microsoftTextToTextTranslator)
        {
            _microsoftTextToTextTranslator = microsoftTextToTextTranslator;
        }
        public async Task Translate(
            string apiKey,
            string apiRegion,
            string sourceLanguageCode,
            string textToTranslate,
            string targetLanguageCode,
            string participantOneLanguageCode,
            Participant participant
            )
        {
            _participant = participant ?? throw new ArgumentNullException(nameof(participant));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _region = apiRegion ?? throw new ArgumentNullException(nameof(apiRegion));

            Guid = _participant.Guid;

            //Text to Text
            var translatedText = await _microsoftTextToTextTranslator.TranslateTextToText(apiKey, apiRegion, sourceLanguageCode, textToTranslate, targetLanguageCode);
            var result = new TranslationResult
            {
                Guid = participant.Guid,
                OriginalText = textToTranslate,
                SourceLanguageCode = sourceLanguageCode,
                TargetLanguageCode = targetLanguageCode,
                TranslatedText = translatedText,
                OffsetInTicks = RandomLong()
            };

            if(sourceLanguageCode == participantOneLanguageCode)
            {
                result.IsPersonOne = true;
            }
            else
            {
                result.IsPersonOne = false;
            }

            //Synthesize Text to Speech
            var neuralLanguages = NeuralVoiceLanguages.GetLanguages();
            var targetLanguage = neuralLanguages.FirstOrDefault(l => l.Code.ToLower().Contains(targetLanguageCode.ToLower()));

            if (targetLanguage != null)
            {
                try
                {
                    var synthesizer = SetupNeuralVoiceSynthesizer(targetLanguage.Code, targetLanguage.Voice.First());
                    synthesizer.OnAudioAvailable += Synthesizer_OnAudioAvailable;
                    synthesizer.OnError += Synthesizer_OnError;
                    await synthesizer.Synthesize(CancellationToken.None, result);
                }
                catch (Exception ex)
                {
                    //TODO log exception
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                TranscriptionResultReady?.Invoke(result);

                try
                {
                    var autoSynthesizer = SetupStandardVoiceSynthesizer(result.TargetLanguageCode);
                    var synthesisResult = await autoSynthesizer.SpeakTextAsync(result.TranslatedText);

                    if (synthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        var audioResult = new TranslationResult
                        {
                            Guid = Guid,
                            SourceLanguageCode = sourceLanguageCode,
                            TargetLanguageCode = result.TargetLanguageCode,
                            AudioResult = synthesisResult.AudioData
                        };

                        TranslationSpeechReady?.Invoke(audioResult);
                    }
                    else if (synthesisResult.Reason == ResultReason.Canceled)
                    {
                        Debug.WriteLine($"Standard Synthesizer Error: {synthesisResult.Reason}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Standard Synthesizer Error: {ex.Message}");
                }
            }
        }

        private void Synthesizer_OnAudioAvailable(object sender, SynthesizerEventArgs<TranslationResult> e)
        {
            var result = e.EventData;
            TranscriptionResultReady?.Invoke(result);
            TranslationSpeechReady?.Invoke(result);
        }

        private MicrosoftNeuralVoiceSynthesizer SetupNeuralVoiceSynthesizer(string targetLanguageCode, string voiceName)
        {
            // TODO: Remove this when allocation of region specific endpoints is implemented
            // Make sure the region supports neural voice
            string tokenUrl = $"https://{_region}.api.cognitive.microsoft.com/sts/v1.0/issueToken";
            string endpointUri = $"https://{_region}.tts.speech.microsoft.com/cognitiveservices/v1";

            var auth = new CognitiveServiceAuthentication(tokenUrl, _apiKey);

            var accessToken = auth.GetAccessToken();

            SynthesizerInputOptions inputOptions = new SynthesizerInputOptions()
            {
                RequestUri = new Uri(endpointUri),
                Guid = Guid,
                VoiceType = Gender.Female,
                Locale = targetLanguageCode,
                VoiceName = voiceName,
                OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken
            };

            return new MicrosoftNeuralVoiceSynthesizer(inputOptions);
        }

        private SpeechSynthesizer SetupStandardVoiceSynthesizer(string targetLanguageCode)
        {
            var config = SpeechConfig.FromSubscription(_apiKey, _region);
            config.SpeechSynthesisLanguage = targetLanguageCode;
            config.SetProfanity(ProfanityOption.Raw);

            return new SpeechSynthesizer(config, null);
        }

        private void Synthesizer_OnError(object sender, SynthesizerEventArgs<Exception> e)
        {
            Console.WriteLine($"Neural Synthesizer Error: {e.EventData}");
        }

        private long RandomLong()
        {
            Random random = new Random();
            byte[] bytes = new byte[8];
            random.NextBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }
    }

}
