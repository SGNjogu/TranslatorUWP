using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.AudioFileWriter;
using SpeechlyTouch.Core.Services.AudioInput;
using SpeechlyTouch.Core.Services.AudioOutput;
using SpeechlyTouch.Core.Services.Languages;
using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using SpeechlyTouch.Core.Services.TranslationProviders.Exceptions;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using SpeechlyTouch.Core.Services.TranslationProviders.Utils;
using SpeechlyTouch.Core.Services.Voices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders
{
    public class GoogleTranslationHub : ITranslationHub
    {
        private IEnumerable<Participant> _participants { get; set; }
        private readonly ILanguagesService _languagesService;
        private readonly IVoicesService _voicesService;
        private IAudioFileWriterService _participantOneAudioFileWriter;
        private IAudioFileWriterService _participantTwoAudioFileWriter;
        private bool IsSingleDeviceTranslation { get; set; } = false;
        public bool AllowExplicitContent { get; set; }
        public bool EnableAudioEnhancement { get; set; }

        public List<string> CurrentPhrasesList { get; set; }

        private string _jsonCredentials;
        private string _apiKey;
        private string _region;

        public event Action<TranslationResult> PartialResultReady;
        public event Action<TranslationResult> TranscriptionResultReady;
        public event Action<TranslationResult> TranslationSpeechReady;
        public event Action<TranslationResult> FinalResultReady;
        public event Action<TranslationCancelled> TranslationCancelled;

        public GoogleTranslationHub(
            ILanguagesService languagesService,
            IVoicesService voicesService,
            string apiKey,
            string region
        )
        {
            _languagesService = languagesService ?? throw new ArgumentNullException(nameof(languagesService));
            _voicesService = voicesService ?? throw new ArgumentNullException(nameof(voicesService));
            _apiKey = apiKey;
            _region = region;
            AllowExplicitContent = false;
            EnableAudioEnhancement = false;
            CurrentPhrasesList = new List<string>();
        }


        public void ForceStop()
        {
            // throw new NotImplementedException();
        }

        public async Task StartTranslationAsync
            (
            IEnumerable<Participant> participants,
            string jsonCredentials,
            string region,
            bool isSingleDevice,
            string participantOnelWavFilePath = null,
            string participantTwoWavFilePath = null,
            List<string> candidateLanguages = null
            )
        {
            try
            {
                if (participants is null) throw new ArgumentNullException(nameof(participants));
                if (jsonCredentials is null) throw new ArgumentNullException(nameof(jsonCredentials));
                if (region is null) throw new ArgumentNullException(nameof(region));

                _participants = participants;
                _jsonCredentials = jsonCredentials;
                _region = region;
                IsSingleDeviceTranslation = isSingleDevice;

                // Languages selected by all participants
                var selectedLanguages = _participants
                    .Where(p => IsValid(p))
                    .Select(p => p.SelectedLanguage).ToArray();

                var index = 0;

                foreach (var participant in _participants)
                {
                    if (!IsValid(participant)) throw new InvalidOperationException("Invalid participant. Please make sure participant/SelectedLanguage/AudioProfile is not null");
                    if (participant.Guid == null) throw new NullReferenceException("Participant Guid is null");

                    if (IsSingleDeviceTranslation)
                    {
                        // Single Device session
                        if (index == 0)
                        {
                            var audioInputService = (IAudioInputService)new AudioInputService();
                            var outputService = (IAudioOutputService)new AudioOutputService();
                            outputService.Initialize(participant.AudioProfile.OutputDevice);
                            if (outputService != null) participant.AudioOutputService = outputService;
                            if (audioInputService != null) participant.AudioInputService = audioInputService;
                        }
                        else
                        {
                            var firstParticipant = _participants.First();
                            participant.AudioOutputService = firstParticipant.AudioOutputService;
                            participant.AudioInputService = firstParticipant.AudioInputService;
                        }
                    }
                    else
                    {
                        var audioInputService = (IAudioInputService)new AudioInputService();
                        var outputService = (IAudioOutputService)new AudioOutputService();
                        outputService.Initialize(participant.AudioProfile.OutputDevice);
                        if (outputService != null) participant.AudioOutputService = outputService;
                        if (audioInputService != null) participant.AudioInputService = audioInputService;
                    }

                    ITranslationProvider translationProvider = null;
                    Language targetLanguage = null;

                    if (index == 0) targetLanguage = selectedLanguages[1];
                    if (index == 1) targetLanguage = selectedLanguages[0];
                    index++;

                    translationProvider = CreateTranslationProvider(participant, targetLanguage, targetLanguage.Voice.First(), participant.AudioInputService);
                    participant.TranslationProvider = translationProvider;
                }

                AddTranslationEventListeners();

                if (participantOnelWavFilePath != null)
                {
                    _participantOneAudioFileWriter = new AudioFileWriterService();
                    _participantOneAudioFileWriter.Initialize(participantOnelWavFilePath);
                }

                // if participants are using the same input device, pause the second translator
                if (IsSingleDeviceTranslation)
                {
                    await StartParticipantOneTranslation();
                }
                else
                {
                    if (participantTwoWavFilePath != null)
                    {
                        _participantTwoAudioFileWriter = new AudioFileWriterService();
                        _participantTwoAudioFileWriter.Initialize(participantTwoWavFilePath);
                    }
                    await StartTranslationAll();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private ITranslationProvider CreateTranslationProvider
            (
            Participant participant,
            Language targetLanguage,
            string voiceName,
            IAudioInputService audioInputService
            )
        {
            try
            {
                var provider = new GoogleProvider(_languagesService, _voicesService, _jsonCredentials, _apiKey, _region);
                provider.Initialize();
                var translationProvider = provider.CreateTranslationProvider(
                    participant.SelectedLanguage,
                    targetLanguage,
                    voiceName,
                    participant.AudioProfile.InputDevice,
                    audioInputService);

                if (translationProvider == null)
                    throw new TranslationProviderCreationFailedException("Check Microsoft provider for errors related to calling CreateTranslationProvider");
                translationProvider.Guid = participant.Guid;
                return translationProvider;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<bool> StartParticipantOneTranslation()
        {
            try
            {
                var participant = _participants.First();
                await participant.TranslationProvider.StartTranslationAsync(AllowExplicitContent, EnableAudioEnhancement);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<bool> StartTranslationAll()
        {
            try
            {
                var _tasks = await Task
                .WhenAll(_participants
                .Select(p => p.TranslationProvider.StartTranslationAsync(AllowExplicitContent, EnableAudioEnhancement)));

                return _tasks.All(t => t == true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool IsValid(Participant participant)
        {
            if (participant is null)
                return false;

            if (participant.SelectedLanguage is null)
                return false;

            if (participant.AudioProfile is null)
                return false;

            return true;
        }

        private void AddTranslationEventListeners()
        {
            foreach (var participant in _participants)
            {
                participant.TranslationProvider.PartialResultReady += OnPartialResultReady;
                participant.TranslationProvider.TranscriptionResultReady += OnTranscriptionResultReady;
                participant.TranslationProvider.TranslationSpeechReady += OnTranslationSpeechReady;
                participant.TranslationProvider.TranslationCancelled += OnTranslationCancelled;
                participant.TranslationProvider.InputDataAvailable += OnInputDataAvailable;
            }
        }

        private void OnPartialResultReady(TranslationResult obj)
        {
            PartialResultReady?.Invoke(obj);
        }

        private void OnTranscriptionResultReady(TranslationResult translationResult)
        {
            TranscriptionResultReady?.Invoke(translationResult);
        }

        private void OnError(object sender, SynthesizerEventArgs<Exception> e)
        {
            Console.WriteLine(e.EventData);
        }

        private void OnTranslationCancelled(TranslationCancelled obj)
        {
            Debug.WriteLine($"OncancelledError: \n{obj.ErrorDetails}");
            TranslationCancelled?.Invoke(obj);
        }

        private void OnInputDataAvailable(object sender, AudioInputDataAvailableArgs audioData)
        {
            if (IsSingleDeviceTranslation)
            {
                if (_participantOneAudioFileWriter != null)
                    _participantOneAudioFileWriter.Write(audioData.Buffer, audioData.Count);
            }
            else
            {
                var inputDevice = sender as InputDevice;
                var fromParticipant = _participants
                    .Where(p => p.AudioProfile.InputDevice.DeviceId == inputDevice.DeviceId)
                    .FirstOrDefault();

                // Save audio from first participants microphone
                if (fromParticipant != null && fromParticipant.Guid == _participants.First().Guid)
                {
                    if (_participantOneAudioFileWriter != null)
                        _participantOneAudioFileWriter.Write(audioData.Buffer, audioData.Count);
                }

                // Save audio from second participant
                if (fromParticipant != null && fromParticipant.Guid == _participants.Skip(1).First().Guid)
                {
                    if (_participantTwoAudioFileWriter != null)
                        _participantTwoAudioFileWriter.Write(audioData.Buffer, audioData.Count);
                }
            }
        }

        private void OnTranslationSpeechReady(TranslationResult result)
        {
            try
            {
                var otherParticipants = _participants.Where(p => p.Guid != result.Guid);

                foreach (var participant in otherParticipants)
                {
                    if (participant == null || participant.AudioOutputService == null)
                        return;
                    participant.AudioOutputService.Play(result.AudioResult);
                }
                TranslationSpeechReady?.Invoke(result);
                FinalResultReady?.Invoke(result);

                if (IsSingleDeviceTranslation)
                {
                    if (_participantOneAudioFileWriter != null)
                        _participantOneAudioFileWriter.Write(result.AudioResult, result.AudioResult.Length);
                }
                else
                {
                    // Save synthesized speech from other participants other than the first participant
                    var fromParticipant = _participants.Where(p => p.Guid == result.Guid).FirstOrDefault();

                    // Save audio from first participants microphone
                    if (fromParticipant != null && fromParticipant.Guid == _participants.First().Guid)
                    {
                        if (_participantTwoAudioFileWriter != null)
                            _participantTwoAudioFileWriter.Write(result.AudioResult, result.AudioResult.Length);
                    }

                    // Save audio from second participant
                    if (fromParticipant != null && fromParticipant.Guid == _participants.Skip(1).First().Guid)
                    {
                        if (_participantOneAudioFileWriter != null)
                            _participantOneAudioFileWriter.Write(result.AudioResult, result.AudioResult.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO log exception
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task Switch(Participant to)
        {
            var others = _participants.Except(new List<Participant> { to }, new ParticipantComparer());
            foreach (var other in others)
            {
                other.TranslationProvider.PauseSpeechToTextClient();
            }

            await to.TranslationProvider.StartTranslationAsync(AllowExplicitContent, EnableAudioEnhancement);
        }

        public async Task<bool> StopTranslationAsync()
        {
            if (!_participants.Any())
                return false;

            foreach (var participant in _participants)
            {
                participant.TranslationProvider.PauseSpeechToTextClient();
            }

            var _tasks = await Task
                .WhenAll(_participants
                .Select(p => p.TranslationProvider.StopTranslationAsync()));

            return _tasks.All(t => t == true);
        }
    }
}
