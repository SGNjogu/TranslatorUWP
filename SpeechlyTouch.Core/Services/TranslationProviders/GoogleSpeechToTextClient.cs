using Google.Cloud.Speech.V1;
using Google.Protobuf;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.AudioInput;
using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.TranslationProviders
{
    public class GoogleSpeechToTextClient : IGoogleSpeechToTextClient
    {
        private SpeechClient _client;

        public event Action<GoogleSpeechToTextResponse> GoogleSpeechToTextOnTextAvailable;

        /// <summary>
        /// Sample code for infinite streaming. The strategy for infinite streaming is to restart each stream
        /// shortly before it would time out (currently at 5 minutes). We keep track of the end result time of
        /// of when we last saw a "final" transcription, and resend the audio data we'd recorded past that point.
        /// </summary>

        private const int SampleRate = 16000;
        private const int ChannelCount = 1;
        private const int BytesPerSample = 2;
        private const int BytesPerSecond = SampleRate * ChannelCount * BytesPerSample;
        private static readonly TimeSpan s_streamTimeLimit = TimeSpan.FromSeconds(290);

        /// <summary>
        /// Microphone chunks that haven't yet been processed at all.
        /// </summary>
        private readonly BlockingCollection<ByteString> _microphoneBuffer = new BlockingCollection<ByteString>();

        /// <summary>
        /// Chunks that have been sent to Cloud Speech, but not yet finalized.
        /// </summary>
        private readonly LinkedList<ByteString> _processingBuffer = new LinkedList<ByteString>();

        /// <summary>
        /// The start time of the processing buffer, in relation to the start of the stream.
        /// </summary>
        private TimeSpan _processingBufferStart;

        /// <summary>
        /// The current RPC stream, if any.
        /// </summary>
        private SpeechClient.StreamingRecognizeStream _rpcStream;

        /// <summary>
        /// The deadline for when we should stop the current stream.
        /// </summary>
        private DateTime _rpcStreamDeadline;

        /// <summary>
        /// The task indicating when the next response is ready, or when we've
        /// reached the end of the stream. (The task will complete in either case, with a result
        /// of True if it's moved to another response, or False at the end of the stream.)
        /// </summary>
        private ValueTask<bool> _serverResponseAvailableTask;

        private bool _listenToMicInput { get; set; }

        public event AudioInputDataAvailable InputDataAvailable;

        private readonly object _lockObject = new object();
        private readonly string _jsonCredentials;
        private readonly string _languageCode;
        private readonly IAudioInputService _audioInputService;
        private readonly InputDevice _inputDevice;

        public GoogleSpeechToTextClient
            (
            string languageCode,
            IAudioInputService audioInputService,
            InputDevice inputDevice,
            string jsonCredentials
            )
        {
            _languageCode = languageCode;
            _audioInputService = audioInputService;
            _inputDevice = inputDevice;
            _jsonCredentials = jsonCredentials;
        }

        private async Task Initialize()
        {
            try
            {
                if (_client == null)
                {
                    SpeechClientBuilder speechClientBuilder = new SpeechClientBuilder
                    {
                        JsonCredentials = _jsonCredentials
                    };

                    _client = await speechClientBuilder.BuildAsync();
                }

                _audioInputService.DataAvailable += OnDataAvailableAsync;

                _listenToMicInput = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Runs the main loop until "exit" or "quit" is heard.
        /// </summary>
        public async Task StartSpeechToTextAsync()
        {
            await Initialize();
            _audioInputService.StartRecording(_inputDevice);

            while (_listenToMicInput)
            {
                await MaybeStartStreamAsync();
                // ProcessResponses will return false if it hears "exit" or "quit".
                if (!await ProcessResponsesAsync())
                {
                    return;
                }
                await TransferMicrophoneChunkAsync();
            }
        }

        private void OnDataAvailableAsync(object sender, AudioInputDataAvailableArgs dataAvailableEventArgs)
        {
            try
            {
                lock (_lockObject)
                {
                    InputDataAvailable?.Invoke(_inputDevice, dataAvailableEventArgs);

                    _microphoneBuffer.Add(ByteString.CopyFrom(dataAvailableEventArgs.Buffer, 0, dataAvailableEventArgs.Count));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Starts a new RPC streaming call if necessary. This will be if either it's the first call
        /// (so we don't have a current request) or if the current request will time out soon.
        /// In the latter case, after starting the new request, we copy any chunks we'd already sent
        /// in the previous request which hadn't been included in a "final result".
        /// </summary>
        private async Task MaybeStartStreamAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    var now = DateTime.UtcNow;
                    if (_rpcStream != null && now >= _rpcStreamDeadline)
                    {
                        Console.WriteLine($"Closing stream before it times out");
                        await _rpcStream.WriteCompleteAsync();
                        _rpcStream.GrpcCall.Dispose();
                        _rpcStream = null;
                    }

                    // If we have a valid stream at this point, we're fine.
                    if (_rpcStream != null)
                    {
                        return;
                    }
                    // We need to create a new stream, either because we're just starting or because we've just closed the previous one.
                    _rpcStream = _client.StreamingRecognize();
                    _rpcStreamDeadline = now + s_streamTimeLimit;
                    _processingBufferStart = TimeSpan.Zero;
                    _serverResponseAvailableTask = _rpcStream.GetResponseStream().MoveNextAsync();
                    await _rpcStream.WriteAsync(new StreamingRecognizeRequest
                    {
                        StreamingConfig = new StreamingRecognitionConfig
                        {
                            Config = new RecognitionConfig
                            {
                                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                                SampleRateHertz = SampleRate,
                                LanguageCode = _languageCode,
                                MaxAlternatives = 1
                            },
                            InterimResults = true,
                        }
                    });

                    Console.WriteLine($"Writing {_processingBuffer.Count} chunks into the new stream.");
                    foreach (var chunk in _processingBuffer)
                    {
                        await WriteAudioChunk(chunk);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Processes responses received so far from the server,
        /// returning whether "exit" or "quit" have been heard.
        /// </summary>
        private async Task<bool> ProcessResponsesAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    while (_serverResponseAvailableTask.IsCompleted && _serverResponseAvailableTask.Result)
                    {
                        var response = _rpcStream.GetResponseStream().Current;

                        foreach(var item in response.Results)
                        {
                            Debug.WriteLine($"Partial Results: {item}");
                        }

                        _serverResponseAvailableTask = _rpcStream.GetResponseStream().MoveNextAsync();
                        // Uncomment this to see the details of interim results.
                        // Console.WriteLine($"Response: {response}");

                        // See if one of the results is a "final result". If so, we trim our
                        // processing buffer.
                        var finalResult = response.Results.FirstOrDefault(r => r.IsFinal);

                        if (finalResult != null)
                        {
                            string transcript = finalResult.Alternatives[0].Transcript;

                            Console.WriteLine($"Final Response: {transcript}");
                            if (transcript.ToLowerInvariant().Contains("exit") ||
                                transcript.ToLowerInvariant().Contains("quit"))
                            {
                                return false;
                            }

                            TimeSpan resultEndTime = finalResult.ResultEndTime.ToTimeSpan();

                            // Rather than explicitly iterate over the list, we just always deal with the first
                            // element, either removing it or stopping.
                            double duration = 0;
                            int removed = 0;
                            while (_processingBuffer.First != null)
                            {
                                var sampleDuration = TimeSpan.FromSeconds(_processingBuffer.First.Value.Length / (double)BytesPerSecond);
                                duration += sampleDuration.TotalSeconds;
                                var sampleEnd = _processingBufferStart + sampleDuration;

                                // If the first sample in the buffer ends after the result ended, stop.
                                // Note that part of the sample might have been included in the result, but the samples
                                // are short enough that this shouldn't cause problems.
                                if (sampleEnd > resultEndTime)
                                {
                                    break;
                                }
                                _processingBufferStart = sampleEnd;
                                _processingBuffer.RemoveFirst();
                                removed++;
                            }

                            GoogleSpeechToTextOnTextAvailable?.Invoke(new GoogleSpeechToTextResponse { SpeechText = transcript, Duration = TimeSpan.FromSeconds(duration), OffsetInTicks = resultEndTime.Ticks });
                        }
                    }
                    return true;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Takes a single sample chunk from the microphone buffer, keeps a local copy
        /// (in case we need to send it again in a new request) and sends it to the server.
        /// </summary>
        /// <returns></returns>
        private async Task TransferMicrophoneChunkAsync()
        {
            try
            {
                // This will block - but only for ~100ms, unless something's really broken.
                var chunk = _microphoneBuffer.Take();
                _processingBuffer.AddLast(chunk);
                await WriteAudioChunk(chunk);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Writes a single chunk to the RPC stream.
        /// </summary>
        private async Task WriteAudioChunk(ByteString chunk)
        {
            await _rpcStream.WriteAsync(new StreamingRecognizeRequest { AudioContent = chunk });
        }

        public async Task<bool> StopTranslationAsync()
        {
            try
            {
                PauseSpeechToTextClient();
                await _rpcStream.WriteCompleteAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return await Task.Run(() => true);
        }

        public void PauseSpeechToTextClient()
        {
            _audioInputService.DataAvailable -= OnDataAvailableAsync;
            _listenToMicInput = false;
        }
    }
}
