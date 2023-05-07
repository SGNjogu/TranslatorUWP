using SpeechlyTouch.Core.Domain;

namespace SpeechlyTouch.Core.Services.AudioOutput
{
    public interface IAudioOutputService
    {
        /// <summary>
        /// Initialize Service to prepare for playing audio
        /// Shoud be called before calling play
        /// </summary>
        void Initialize(OutputDevice outputDevice);

        /// <summary>
        /// Start playback
        /// if audio is already playing, this audio data will be queued and played
        /// after the current audio track
        /// </summary>
        /// <param name="audio">Audio data to play</param>
        void Play(byte[] audio);

        /// <summary>
        /// Stop playback
        /// </summary>
        void Stop();

        void Mute();

        void UnMute();
    }
}
