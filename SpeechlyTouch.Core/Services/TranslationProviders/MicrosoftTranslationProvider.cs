using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Services.AudioInput;
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
        private readonly IAudioInputService _audioInputService;
        private readonly InputDevice _inputDevice;
        private readonly string _apiKey;
        private readonly string _region;
        private readonly object _lockObject = new object();

        public Language SourceLanguage { get; }
        public Language TargetLanguage { get; }
        public string VoiceName { get; }
        private ProfanityOption profanityOption { get; set; }

        public event Action<TranslationResult> PartialResultReady;
        public event Action<TranslationResult> TranscriptionResultReady;
        public event Action<TranslationResult> TranslationSpeechReady;
        public event Action<TranslationResult> FinalResultReady;
        public event Action<TranslationCancelled> TranslationCancelled;
        public event AudioInputDataAvailable InputDataAvailable;

        // Translation config for ForwardRecognizer
        public Guid Guid { get; set; }
        public SpeechTranslationConfig TranslationConfig { get; private set; }
        public TranslationRecognizer Recognizer { get; private set; }

        public MicrosoftTranslationProvider
        (
            Language sourceLanguage,
            Language targetLanguage,
            string voiceName,
            IAudioInputService audioInputService,
            InputDevice inputDevice,
            string apiKey,
            string region
        )
        {
            SourceLanguage = sourceLanguage ?? throw new ArgumentNullException(nameof(sourceLanguage));
            TargetLanguage = targetLanguage ?? throw new ArgumentNullException(nameof(targetLanguage));
            VoiceName = voiceName;
            _audioInputService = audioInputService ?? throw new ArgumentNullException(nameof(audioInputService));
            _inputDevice = inputDevice ?? throw new ArgumentNullException(nameof(inputDevice));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _region = region ?? throw new ArgumentNullException(nameof(region));
        }

        /// <summary>
        /// Initialize provider
        /// </summary>
        /// <exception cref="NullReferenceException">Throw when SourceLanguage or TargetLanguage is null</exception>
        private void Initialize(bool allowExplicitContent, bool enableAudioEnhancement)
        {
            if (allowExplicitContent)
            {
                profanityOption = ProfanityOption.Raw;
            }
            else
            {
                profanityOption = ProfanityOption.Masked;
            }
            TranslationConfig = SpeechTranslationConfig.FromSubscription(_apiKey, _region);

            if (string.IsNullOrEmpty(SourceLanguage.Code))
                throw new NullReferenceException($"Target Language code is null: {SourceLanguage}");

            if (string.IsNullOrEmpty(TargetLanguage.Code))
                throw new NullReferenceException($"Target Language code is null: {TargetLanguage}");

            TranslationConfig.SpeechRecognitionLanguage = SourceLanguage.Code;
            TranslationConfig.AddTargetLanguage(TargetLanguage.Code);
            TranslationConfig.SetProfanity(profanityOption);

            var isNeuralVoiceLanguge = NeuralVoiceLanguages.IsNeuralVoiceLanguage(TargetLanguage.Code);

            if (!isNeuralVoiceLanguge)
            {
                TranslationConfig.VoiceName = VoiceName;
                TranslationConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);
            }

            TranslationConfig.RequestWordLevelTimestamps();
            _audioInputService.DataAvailable += OnDataAvailable;

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
          
            Recognizer = new TranslationRecognizer(TranslationConfig, audioConfig);

            Recognizer.Recognizing += OnRecognizing;
            Recognizer.Recognized += OnRecognized;
            //Recognizer.Synthesizing += OnSynthesing;
            Recognizer.Canceled += OnCanceled;
        }

        private void OnDataAvailable(object sender, AudioInputDataAvailableArgs dataAvailableEventArgs)
        {
            lock (_lockObject)
            {
                InputDataAvailable?.Invoke(_inputDevice, dataAvailableEventArgs);
            }
        }

        private void OnCanceled(object sender, TranslationRecognitionCanceledEventArgs e)
        {
            var translationCancelled = new TranslationCancelled
            {
                Reason = e.Reason.ToString(),
                ErrorCode = e.ErrorCode.ToString(),
                ErrorDetails = e.ErrorDetails
            };

            TranslationCancelled?.Invoke(translationCancelled);
        }

        Queue<long> offsetQueue = new Queue<long>();
        long offset;

        private void OnRecognizing(object sender, TranslationRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.TranslatingSpeech)
            {
                _audioInputService.SetInputDeviceState(InputDeviceState.Recognizing);

                var partialResult = new TranslationResult
                {
                    Guid = Guid,
                    OriginalText = e.Result.Text,
                    SourceLanguageCode = SourceLanguage.Code,
                    TargetLanguageCode = TargetLanguage.Code,
                    OffsetInTicks = e.Result.OffsetInTicks,
                    Duration = e.Result.Duration
                };

                if (offset != e.Result.OffsetInTicks)
                {
                    offset = e.Result.OffsetInTicks;
                    offsetQueue.Enqueue(offset);
                }

                foreach (var t in e.Result.Translations)
                {
                    partialResult.TranslatedText = t.Value;
                }

                PartialResultReady?.Invoke(partialResult);
            }
        }

        private async void OnRecognized(object sender, TranslationRecognitionEventArgs e)
        {
            _audioInputService.SetInputDeviceState(InputDeviceState.Recognized);

            if (e.Result.Text.Length > 0)
            {
                TranslationResult result = new TranslationResult();
                if (offsetQueue.Any())
                {
                    var firstItem = offsetQueue.Dequeue();
                    result = new TranslationResult
                    {
                        Guid = Guid,
                        OriginalText = e.Result.Text,
                        SourceLanguageCode = SourceLanguage.Code,
                        TargetLanguageCode = TargetLanguage.Code,
                        Duration = e.Result.Duration,
                        OffsetInTicks = firstItem
                    };
                }
                else
                {
                    result = new TranslationResult
                    {
                        Guid = Guid,
                        OriginalText = e.Result.Text,
                        SourceLanguageCode = SourceLanguage.Code,
                        TargetLanguageCode = TargetLanguage.Code,
                        Duration = e.Result.Duration,
                        OffsetInTicks = e.Result.OffsetInTicks
                    };
                }

                foreach (var t in e.Result.Translations)
                {
                    result.TranslatedText = t.Value;
                }

                var neuralLanguages = NeuralVoiceLanguages.GetLanguages();
                var targetLanguage = neuralLanguages.FirstOrDefault(l => l.Code.ToLower().Contains(TargetLanguage.Code.ToLower()));

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
                                SourceLanguageCode = result.SourceLanguageCode,
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
                }
            }
        }

        private void Synthesizer_OnAudioAvailable(object sender, SynthesizerEventArgs<TranslationResult> e)
        {
            var result = e.EventData;
            TranscriptionResultReady?.Invoke(result);
            TranslationSpeechReady?.Invoke(result);
            FinalResultReady?.Invoke(result);
        }

        private void Synthesizer_OnError(object sender, SynthesizerEventArgs<Exception> e)
        {
            Console.WriteLine($"Neural Synthesizer Error: {e.EventData}");
        }

        //private void OnSynthesing(object sender, TranslationSynthesisEventArgs e)
        //{
        //    _audioInputService.SetInputDeviceState(InputDeviceState.Synthesizing);

        //    var audio = e.Result.GetAudio();

        //    if (audio.Length == 0)
        //        return;

        //    var result = new TranslationResult
        //    {
        //        Guid = Guid,
        //        SourceLanguageCode = SourceLanguage.Code,
        //        TargetLanguageCode = TargetLanguage.Code,
        //        AudioResult = e.Result.GetAudio()
        //    };

        //    TranslationSpeechReady?.Invoke(result);
        //    FinalResultReady?.Invoke(result);
        //}

        public async Task<bool> StartTranslationAsync(bool allowExlicitContent, bool enableAudioEnhancement, List<string> candidateLanguages = null)
        {
            try
            {
                Initialize(allowExlicitContent, enableAudioEnhancement);

                if (_audioInputService != null)
                    _audioInputService.StartRecording(_inputDevice);

                await Recognizer.StartContinuousRecognitionAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> StopTranslationAsync()
        {
            _audioInputService.SetInputDeviceState(InputDeviceState.Idle);

            try
            {
                _audioInputService.DataAvailable -= OnDataAvailable;
                _audioInputService.StopRecording();

                if (Recognizer != null)
                {
                    Recognizer.Recognizing -= OnRecognizing;
                    Recognizer.Recognized -= OnRecognized;
                    //Recognizer.Synthesizing -= OnSynthesing;
                    Recognizer.Canceled -= OnCanceled;

                    await Recognizer.StopContinuousRecognitionAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {
            try
            {
                lock (_lockObject)
                {
                    if (Recognizer != null)
                        Recognizer.Dispose();
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                //TODO log exception
                Debug.WriteLine(ex.Message);
            }
        }

        public void ForceStop()
        {
            try
            {
                _audioInputService.SetInputDeviceState(InputDeviceState.Idle);

                if (_audioInputService != null)
                {
                    _audioInputService.DataAvailable -= OnDataAvailable;
                    _audioInputService.StopRecording();
                }

                if (Recognizer != null)
                {
                    Recognizer.Recognizing -= OnRecognizing;
                    Recognizer.Recognized -= OnRecognized;
                    //Recognizer.Synthesizing -= OnSynthesing;
                    Recognizer.Canceled -= OnCanceled;
                    Recognizer.Dispose();
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                //TODO log exception
                Debug.WriteLine(ex.Message);
            }
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

        public void PauseSpeechToTextClient()
        {
            // throw new NotImplementedException();
        }
    }
}
