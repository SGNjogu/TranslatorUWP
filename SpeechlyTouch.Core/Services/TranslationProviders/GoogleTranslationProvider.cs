using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.AudioInput;
using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using SpeechlyTouch.Core.Services.TranslationProviders.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders
{
    public class GoogleTranslationProvider : ITranslationProvider
    {
        private IGoogleTextToTextTranslator _textToTextClient;
        private IGoogleSpeechToTextClient _googleSpeechToTextClient;
        private readonly IAudioInputService _audioInputService;
        private readonly InputDevice _inputDevice;
        private readonly string _jsonCredentials;
        private ConcurrentQueue<GoogleSpeechToTextResponse> SpeechToTextResponseQueue { get; set; }

        private readonly string _apiKey;
        private readonly string _region;

        public Language SourceLanguage { get; }
        public Language TargetLanguage { get; }
        public string VoiceName { get; }

        public event Action<TranslationResult> PartialResultReady;
        public event Action<TranslationResult> TranscriptionResultReady;
        public event Action<TranslationResult> TranslationSpeechReady;
        public event Action<TranslationResult> FinalResultReady;
        public event Action<TranslationCancelled> TranslationCancelled;
        public event AudioInputDataAvailable InputDataAvailable;

        // Translation config for ForwardRecognizer
        public Guid Guid { get; set; }


        public GoogleTranslationProvider
        (
            Language sourceLanguage,
            Language targetLanguage,
            string voiceName,
            IAudioInputService audioInputService,
            InputDevice inputDevice,
            string jsonCredentials,
            string region,
            string apiKey
        )
        {
            SourceLanguage = sourceLanguage ?? throw new ArgumentNullException(nameof(sourceLanguage));
            TargetLanguage = targetLanguage ?? throw new ArgumentNullException(nameof(targetLanguage));
            VoiceName = voiceName;
            _audioInputService = audioInputService ?? throw new ArgumentNullException(nameof(audioInputService));
            _inputDevice = inputDevice ?? throw new ArgumentNullException(nameof(inputDevice));
            _jsonCredentials = jsonCredentials ?? throw new ArgumentNullException(nameof(jsonCredentials));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _region = region ?? throw new ArgumentNullException(nameof(region));
        }

        private async Task Initialize()
        {
            try
            {
                _googleSpeechToTextClient = new GoogleSpeechToTextClient(SourceLanguage.Code, _audioInputService, _inputDevice, _jsonCredentials);
                _googleSpeechToTextClient.InputDataAvailable += OnInputDataAvailable;
                _googleSpeechToTextClient.GoogleSpeechToTextOnTextAvailable += OnSpeechToTextTextAvailable;
                _textToTextClient = new GoogleTextToTextTranslator(_jsonCredentials);
                _textToTextClient.GoogleTextTranslationOnTextAvailable += OnTranslatedTextAvailable;
                await _textToTextClient.Initialize();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void OnInputDataAvailable(object sender, AudioInputDataAvailableArgs dataAvailableEventArgs)
        {
            InputDataAvailable?.Invoke(_inputDevice, dataAvailableEventArgs);
        }

        public void Dispose()
        {
            // throw new NotImplementedException();
        }

        public void ForceStop()
        {
            // throw new NotImplementedException();
        }

        public async Task<bool> StartTranslationAsync(bool allowExplicitContent,bool enableAudioEnhancement = false, List<string> candidateLanguages = null)
        {
            try
            {
                await Initialize();
                Thread thread = new Thread(async () => { await _googleSpeechToTextClient.StartSpeechToTextAsync(); });
                thread.Start();

                return await Task.Run(() => true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void OnSpeechToTextTextAvailable(GoogleSpeechToTextResponse speechToTextResponse)
        {
            if (SpeechToTextResponseQueue == null)
            {
                SpeechToTextResponseQueue = new ConcurrentQueue<GoogleSpeechToTextResponse>();
            }

            SpeechToTextResponseQueue.Enqueue(speechToTextResponse);

            await TranslateTextResponse(speechToTextResponse.SpeechText, speechToTextResponse.OffsetInTicks, speechToTextResponse.Duration);
        }

        //  We probably need an event for this
        private async Task TranslateTextResponse(string transcript, long offsetInTicks, TimeSpan duration)
        {
            try
            {
                while (SpeechToTextResponseQueue.Any())
                {
                    GoogleSpeechToTextResponse googleSpeechToTextResponse;
                    SpeechToTextResponseQueue.TryDequeue(out googleSpeechToTextResponse);

                    if (googleSpeechToTextResponse != null)
                    {
                        await _textToTextClient.TranslateAsync(transcript, SourceLanguage.Code, TargetLanguage.Code, offsetInTicks, duration);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void OnTranslatedTextAvailable(GoogleTextTranslationResponse googleTextTranslationResponse)
        {
            try
            {
                var result = new TranslationResult
                {
                    Guid = Guid,
                    OriginalText = googleTextTranslationResponse.OriginalText,
                    SourceLanguageCode = SourceLanguage.Code,
                    TargetLanguageCode = TargetLanguage.Code,
                    TranslatedText = googleTextTranslationResponse.TranslatedText,
                    OffsetInTicks = googleTextTranslationResponse.OffsetInTicks,
                    Duration = googleTextTranslationResponse.Duration
                };

                TranscriptionResultReady?.Invoke(result);

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task<bool> StopTranslationAsync()
        {
            if (_textToTextClient != null)
            {
                _textToTextClient.GoogleTextTranslationOnTextAvailable -= OnTranslatedTextAvailable;
            }
            if (_googleSpeechToTextClient != null)
            {
                _googleSpeechToTextClient.InputDataAvailable -= OnInputDataAvailable;
                _googleSpeechToTextClient.GoogleSpeechToTextOnTextAvailable -= OnSpeechToTextTextAvailable;
                await _googleSpeechToTextClient.StopTranslationAsync();
            }
            return await Task.Run(() => true);
        }

        public void PauseSpeechToTextClient()
        {
            if (_googleSpeechToTextClient != null)
                _googleSpeechToTextClient.PauseSpeechToTextClient();
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

        public async Task<bool> StartAutoDetectTranslationAsync(bool allowExlicitContent,bool enableAudioEnhancement, List<string> candidateLanguages = null)
        {
            //Not implemented with Google
            return await Task.Run(() => true);
        }

        public async Task<bool> StopAutoDetectTranslationAsync()
        {
            //Not implemented with Google
            return await Task.Run(() => true);

        }

        public void AutoDetectForceStop()
        {
            //Not implemented with Google
        }
    }
}
