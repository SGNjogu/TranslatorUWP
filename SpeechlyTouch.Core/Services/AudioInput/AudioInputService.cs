using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Helpers;
using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace SpeechlyTouch.Core.Services.AudioInput
{
    public class AudioInputService : IAudioInputService
    {
        private ConcurrentQueue<short[]> _frameQueue;
        private AudioEncodingProperties _encodingProperties;
        private AudioDeviceInputNode _inputNode;
        private AudioGraph _audioGraph;
        private AudioDeviceController _audioDeviceController;
        private InputDevice _inputDevice;
        private MediaCapture _capture;
        private InMemoryRandomAccessStream _buffer;
        private MediaFrameReader _mediaFrameReader;
        private bool _isRecording = false;
        private bool _isMute = false;
        public event AudioInputDataAvailable DataAvailable;
        public event RecordingStopped RecordingStopped;

        public void SetInputDeviceState(InputDeviceState newState)
        {
            if (_inputDevice != null)
                _inputDevice.SetState(newState);
        }

        public async void StartRecording(InputDevice inputDevice)
        {
            try
            {
                _inputDevice = inputDevice;
                _encodingProperties = AudioEncodingProperties.CreatePcm(16000, 1, 16);
                var isInitialized = await Initialize(_inputDevice);

                if (isInitialized)
                {
                    var profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium);
                    profile.Audio = _encodingProperties;
                    _isRecording = true;
                    await _capture.StartRecordToStreamAsync(profile, _buffer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task<bool> Initialize(InputDevice inputDevice)
        {
            if (_buffer != null)
            {
                _buffer.Dispose();
            }
            _buffer = new InMemoryRandomAccessStream();
            if (_capture != null)
            {
                _capture.Dispose();
            }
            try
            {
                _capture = new MediaCapture();

                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio,
                    AudioDeviceId = inputDevice.DeviceId,
                    MediaCategory = MediaCategory.Speech,
                    AudioProcessing = AudioProcessing.Default
                };

                await _capture.InitializeAsync(settings);

                var audioFrameSources = _capture.FrameSources.Where(x => x.Value.Info.MediaStreamType == MediaStreamType.Audio);

                if (audioFrameSources.Count() == 0)
                {
                    Debug.WriteLine("No audio frame source was found.");
                    return false;
                }

                MediaFrameSource frameSource = audioFrameSources.FirstOrDefault().Value;
                _audioDeviceController = frameSource.Controller.AudioDeviceController;

                _mediaFrameReader = await _capture.CreateFrameReaderAsync(frameSource);

                // Optionally set acquisition mode. Buffered is the default mode for audio.
                _mediaFrameReader.AcquisitionMode = MediaFrameReaderAcquisitionMode.Realtime;

                _mediaFrameReader.FrameArrived += MediaFrameReader_FrameArrived;

                var status = await _mediaFrameReader.StartAsync();

                if (status != MediaFrameReaderStartStatus.Success)
                {
                    Debug.WriteLine("The MediaFrameReader couldn't start.");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(UnauthorizedAccessException))
                {
                    throw ex.InnerException;
                }
                throw;
            }
            return true;
        }

        private void MediaFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            try
            {
                if (_audioDeviceController.Muted == true) return;

                using (MediaFrameReference reference = sender.TryAcquireLatestFrame())
                {
                    if (reference != null)
                    {
                        ProcessAudioFrame(reference.AudioMediaFrame);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private unsafe void ProcessAudioFrame(AudioMediaFrame audioMediaFrame)
        {
            using (AudioFrame audioFrame = audioMediaFrame.GetAudioFrame())
            using (AudioBuffer buffer = audioFrame.LockBuffer(AudioBufferAccessMode.Read))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                ((IMemoryBufferByteAccess)reference).GetBuffer(out var dataInBytes, out _);

                // The requested format was float
                var dataInFloat = (float*)dataInBytes;

                // Get the number of samples by multiplying the duration by sampling rate: 
                // duration [s] x sampling rate [samples/s] = # samples 

                // Duration can be gotten off the frame reference OR the audioFrame
                TimeSpan duration = audioMediaFrame.FrameReference.Duration;

                // frameDurMs is in milliseconds, while SampleRate is given per second.
                uint frameDurMs = (uint)duration.TotalMilliseconds;
                uint sampleRate = audioMediaFrame.AudioEncodingProperties.SampleRate;
                uint channelCount = audioMediaFrame.AudioEncodingProperties.ChannelCount;
                uint sampleCount = (frameDurMs * sampleRate) / 1000;

                // extract data and convert to mono
                float[] floats = new float[sampleCount];

                if (channelCount > 1)
                {
                    for (var i = 0; i < sampleCount * channelCount; i += 2)
                    {
                        floats[i / 2] = dataInFloat[i] * 0.5f + dataInFloat[i + 1] * 0.5f;
                    }
                }
                else
                {
                    for (var i = 0; i < sampleCount * channelCount; i += 1)
                    {
                        floats[i] = dataInFloat[i];
                    }
                }

                // downsample
                if (sampleRate > 16000)
                {
                    floats = AudioSampler.DownsampleBuffer(floats, sampleRate, 16000);
                }

                var newBuffer = FloatToBytesConverter.ToByteArray(floats);

                var inputDataAvailableArgs = new AudioInputDataAvailableArgs
                {
                    Buffer = newBuffer,
                    Count = newBuffer.Length
                };

                DataAvailable?.Invoke(this, inputDataAvailableArgs);
            }
        }


        public async void StopRecording()
        {
            try
            {
                _mediaFrameReader.FrameArrived -= MediaFrameReader_FrameArrived;

                if (_buffer != null)
                    _buffer.Dispose();

                if (_isRecording)
                {
                    _isRecording = false;
                    await _capture.StopRecordAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void Mute()
        {
            if (_audioDeviceController != null)
                _audioDeviceController.Muted = true;
        }

        public void Unmute()
        {
            if (_audioDeviceController != null)
                _audioDeviceController.Muted = false;
        }
    }
}

[ComImport]
[Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
unsafe interface IMemoryBufferByteAccess
{
    void GetBuffer(out byte* buffer, out uint capacity);
}