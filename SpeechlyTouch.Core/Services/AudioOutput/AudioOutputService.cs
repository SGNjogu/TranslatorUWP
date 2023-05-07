using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace SpeechlyTouch.Core.Services.AudioOutput
{
    public class AudioOutputService : IAudioOutputService
    {
        private bool _isInitialized { get; set; }
        private MemoryStream _audioMemoryStream { get; set; }

        private OutputDevice _outputDevice;
        private MediaPlayer _mediaPlayer;
        private ConcurrentQueue<byte[]> _audioQueue { get; set; }
        private readonly object _lockObject = new object();
        public event Action PlaybackStopped;

        private bool _mute { get; set; } = false;

        public AudioOutputService()
        {
            _audioQueue = new ConcurrentQueue<byte[]>();
        }

        public void Initialize(OutputDevice outputDevice)
        {
            try
            {
                _mute = false;

                if (outputDevice is null)
                    throw new ArgumentNullException(nameof(outputDevice));

                if (_isInitialized)
                    return;

                _outputDevice = outputDevice;
                _audioMemoryStream = new MemoryStream();
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.AudioDevice = _outputDevice.AudioDevice;
                _mediaPlayer.MediaEnded += MediaEnded;
                _isInitialized = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async void Play(byte[] audio)
        {
            if (_mute)
                return;

            if (audio == null)
                throw new ArgumentException("Audio Response is null");

            _audioQueue.Enqueue(audio);

            try
            {
                while (!_audioQueue.IsEmpty)
                {
                    _audioQueue.TryPeek(out var queuedBytes);
                    _audioMemoryStream = new MemoryStream(queuedBytes);

                    if(_audioQueue != null && _mediaPlayer != null && _mediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                    {
                        _audioQueue.TryDequeue(out _);
                        IRandomAccessStream randomAccessStream = _audioMemoryStream.AsRandomAccessStream();
                        _mediaPlayer.Source = MediaSource.CreateFromStream(randomAccessStream, "wav");
                        _mediaPlayer.Play();
                        _outputDevice.SetState(OutputDeviceState.Playing);
                        await Task.Delay(2000); // Delay to avoid sounding like one sentence
                    }
                }                
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void MediaEnded(MediaPlayer sender, object args)
        {
            _outputDevice.SetState(OutputDeviceState.Idle);
            PlaybackStopped?.Invoke();
        }

        public void Stop()
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    ClearAudioResponseQueue();
                    _mediaPlayer.Pause();
                    _mediaPlayer.Source = null;
                    CleanupPlayback();
                    _outputDevice.SetState(OutputDeviceState.Idle);
                    _isInitialized = false;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Empty AudioQueue
        /// </summary>
        private void ClearAudioResponseQueue()
        {
            while (_audioQueue.TryDequeue(out _)) { }
        }

        // <summary>
        /// Clean up playback related resources
        /// </summary>
        private void CleanupPlayback()
        {
            try
            {
                if (_audioMemoryStream != null)
                {
                    lock (_lockObject)
                    {
                        _audioMemoryStream.Dispose();
                    }
                }

                if (_mediaPlayer != null)
                {
                    lock (_lockObject)
                    {
                        _mediaPlayer.Dispose();
                        _mediaPlayer = null;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO log exception
                Debug.WriteLine(ex.Message);
            }
        }

        public void Mute()
        {
            _mute = true;
        }

        public void UnMute()
        {
            _mute = false;
        }
    }
}
