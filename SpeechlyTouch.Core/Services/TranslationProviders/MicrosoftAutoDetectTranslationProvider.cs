using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using SpeechlyTouch.Core.Services.TranslationProviders.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders
{
    public partial class MicrosoftTranslationProvider : ITranslationProvider
    {
        public SpeechTranslationConfig AutoDetectTranslationConfig { get; private set; }
        public TranslationRecognizer AutoDetectRecognizer { get; private set; }
        private List<string> CandidateLanguages;

        private async void InitializeAutoDetect(bool allowExplicitContent, bool enableAudioEnhancement, List<string> candidateLanguages = null)
        {
            try
            {
                if (allowExplicitContent)
                {
                    profanityOption = ProfanityOption.Raw;
                }
                else
                {
                    profanityOption = ProfanityOption.Masked;
                }

                // Currently the v2 endpoint is required. In a future SDK release you won't need to set it.
                var endpointString = $"wss://{_region}.stt.speech.microsoft.com/speech/universal/v2";
                var endpointUrl = new Uri(endpointString);
                AutoDetectTranslationConfig = SpeechTranslationConfig.FromEndpoint(endpointUrl, _apiKey);

                // Source language is required, but currently ignored. 
                if (string.IsNullOrEmpty(SourceLanguage.Code))
                    throw new NullReferenceException($"Target Language code is null: {SourceLanguage}");

                if (candidateLanguages == null)
                    throw new NullReferenceException($"No Candidate Languages provided");

                CandidateLanguages = new List<string>(candidateLanguages);

                AutoDetectTranslationConfig.SpeechRecognitionLanguage = SourceLanguage.Code;

                if (candidateLanguages != null && candidateLanguages.Any())
                {
                    foreach (var code in candidateLanguages)
                        AutoDetectTranslationConfig.AddTargetLanguage(code);
                }
                else
                {
                    AutoDetectTranslationConfig.AddTargetLanguage(TargetLanguage.Code);
                }

                AutoDetectTranslationConfig.AddTargetLanguage(SourceLanguage.Code);
                AutoDetectTranslationConfig.SetProfanity(profanityOption);

                var isNeuralVoiceLanguge = NeuralVoiceLanguages.IsNeuralVoiceLanguage(TargetLanguage.Code);

                if (!isNeuralVoiceLanguge)
                {
                    AutoDetectTranslationConfig.VoiceName = VoiceName;
                    AutoDetectTranslationConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);
                }

                AutoDetectTranslationConfig.RequestWordLevelTimestamps();

                AutoDetectTranslationConfig.SetProperty(PropertyId.SpeechServiceConnection_ContinuousLanguageIdPriority, "Accuracy");

                string[] candidateLanguagesArray = candidateLanguages.ToArray<string>();

                AutoDetectSourceLanguageConfig autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(candidateLanguagesArray);

                var stopTranslation = new TaskCompletionSource<int>();

                AudioConfig audioConfig = default;
                if (enableAudioEnhancement)
                {
                    var audioProcessingOptions = AudioProcessingOptions.Create(AudioProcessingConstants.AUDIO_INPUT_PROCESSING_DISABLE_GAIN_CONTROL);
                    audioConfig = AudioConfig.FromMicrophoneInput(_inputDevice.DeviceId, audioProcessingOptions);
                }
                else
                {
                    audioConfig = AudioConfig.FromMicrophoneInput(_inputDevice.DeviceId);
                }

                AutoDetectRecognizer = new TranslationRecognizer(AutoDetectTranslationConfig, autoDetectSourceLanguageConfig, audioConfig);

                AutoDetectRecognizer.Recognizing += OnAutoDetectRecognizing;
                AutoDetectRecognizer.Recognized += OnAutoDetectRecognized;
                AutoDetectRecognizer.Canceled += OnAutoDetectCanceled;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        private async void OnAutoDetectRecognized(object sender, TranslationRecognitionEventArgs e)
        {
            try
            {
                string sourceLanguageCode = string.Empty;
                string targetLanguageCode = string.Empty;

                if (e.Result.Reason == ResultReason.TranslatedSpeech)
                {
                    var lidResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);

                    sourceLanguageCode = lidResult;
                }

                _audioInputService.SetInputDeviceState(InputDeviceState.Recognized);

                if (e.Result.Text.Length > 0)
                {
                    var result = new TranslationResult
                    {
                        Guid = Guid,
                        OriginalText = e.Result.Text,
                        SourceLanguageCode = sourceLanguageCode,
                        Duration = e.Result.Duration,
                        OffsetInTicks = e.Result.OffsetInTicks
                    };

                    if (e.Result.Translations == null || e.Result.Translations.Count == 0)
                    {
                        result.TranslatedText = e.Result.Text;
                        result.TargetLanguageCode = sourceLanguageCode;
                    }
                    else
                    {
                        foreach (var t in e.Result.Translations)
                        {
                            result.TranslatedText = t.Value;

                            if (CandidateLanguages != null && CandidateLanguages.Count() == 2 && string.IsNullOrWhiteSpace(t.Key))
                            {
                                result.TargetLanguageCode = CandidateLanguages.FirstOrDefault(_ => _.ToLower() != sourceLanguageCode.ToLower());
                            }
                            else
                            {
                                result.TargetLanguageCode = t.Key;
                            }
                        }
                    }

                    if (SourceLanguage.Code == result.TargetLanguageCode)
                    {
                        result.IsPersonOne = false;
                    }
                    else
                    {
                        result.IsPersonOne = true;
                    }

                    var neuralLanguages = NeuralVoiceLanguages.GetLanguages();
                    var targetLanguage = neuralLanguages.FirstOrDefault(l => l.Code.ToLower().Contains(result.TargetLanguageCode.ToLower()));

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
                            _audioInputService.SetInputDeviceState(InputDeviceState.Synthesizing);

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
                                FinalResultReady?.Invoke(audioResult);
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

                        _audioInputService.SetInputDeviceState(InputDeviceState.Idle);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        private void OnAutoDetectRecognizing(object sender, TranslationRecognitionEventArgs e)
        {
            try
            {
                var lidResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);

                var sourceLanguageCode = lidResult;
                if (e.Result.Reason == ResultReason.TranslatingSpeech)
                {
                    _audioInputService.SetInputDeviceState(InputDeviceState.Recognizing);

                    var partialResult = new TranslationResult
                    {
                        Guid = Guid,
                        OriginalText = e.Result.Text,
                        SourceLanguageCode = sourceLanguageCode,
                        Duration = e.Result.Duration,
                        OffsetInTicks = e.Result.OffsetInTicks
                    };

                    if (e.Result.Translations == null || e.Result.Translations.Count == 0)
                    {
                        partialResult.TranslatedText = e.Result.Text;
                        partialResult.TargetLanguageCode = sourceLanguageCode;
                    }
                    else
                    {
                        foreach (var t in e.Result.Translations)
                        {
                            partialResult.TranslatedText = t.Value;
                            partialResult.TargetLanguageCode = t.Key;
                        }
                    }

                    if (SourceLanguage.Code == sourceLanguageCode)
                    {
                        partialResult.IsPersonOne = true;
                    }
                    else
                    {
                        partialResult.IsPersonOne = false;
                    }

                    PartialResultReady?.Invoke(partialResult);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        private void OnAutoDetectCanceled(object sender, TranslationRecognitionCanceledEventArgs e)
        {
            var translationCancelled = new TranslationCancelled
            {
                Reason = e.Reason.ToString(),
                ErrorCode = e.ErrorCode.ToString(),
                ErrorDetails = e.ErrorDetails
            };

            TranslationCancelled?.Invoke(translationCancelled);
        }

        public async Task<bool> StartAutoDetectTranslationAsync(bool allowExlicitContent, bool enableAudioEnhancement, List<string> candidateLanguages = null)
        {
            try
            {
                InitializeAutoDetect(allowExlicitContent, enableAudioEnhancement, candidateLanguages);

                _audioInputService.StartRecording(_inputDevice);
                await AutoDetectRecognizer.StartContinuousRecognitionAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> StopAutoDetectTranslationAsync()
        {
            _audioInputService.SetInputDeviceState(InputDeviceState.Idle);

            try
            {
                _audioInputService.DataAvailable -= OnDataAvailable;
                _audioInputService.StopRecording();

                if (AutoDetectRecognizer != null)
                {
                    var phraseList = PhraseListGrammar.FromRecognizer(AutoDetectRecognizer);
                    if (phraseList != null)
                    {
                        phraseList.Clear();
                    }
                    AutoDetectRecognizer.Recognizing -= OnAutoDetectRecognizing;
                    AutoDetectRecognizer.Recognized -= OnAutoDetectRecognized;
                    AutoDetectRecognizer.Canceled -= OnAutoDetectCanceled;

                    await AutoDetectRecognizer.StopContinuousRecognitionAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AutoDetectForceStop()
        {
            try
            {
                _audioInputService.SetInputDeviceState(InputDeviceState.Idle);

                if (_audioInputService != null)
                {
                    _audioInputService.DataAvailable -= OnDataAvailable;
                    _audioInputService.StopRecording();
                }

                if (AutoDetectRecognizer != null)
                {
                    AutoDetectRecognizer.Recognizing -= OnAutoDetectRecognizing;
                    AutoDetectRecognizer.Recognized -= OnAutoDetectRecognized;
                    AutoDetectRecognizer.Canceled -= OnAutoDetectCanceled;
                    AutoDetectRecognizer.Dispose();
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                //TODO log exception
                Debug.WriteLine(ex.Message);
            }
        }

        private SpeechSynthesizer SetupStandardVoiceSynthesizer(string targetLanguageCode)
        {
            var config = SpeechConfig.FromSubscription(_apiKey, _region);
            config.SpeechSynthesisLanguage = targetLanguageCode;
            config.SetProfanity(ProfanityOption.Raw);

            return new SpeechSynthesizer(config, null);
        }
    }
}
