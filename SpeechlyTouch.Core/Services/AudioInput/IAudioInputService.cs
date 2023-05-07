using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Services.TranslationProviders.Events;

namespace SpeechlyTouch.Core.Services.AudioInput
{
    public interface IAudioInputService
    {
        event AudioInputDataAvailable DataAvailable;
        event RecordingStopped RecordingStopped;
        void StartRecording(InputDevice inputDevice);
        void StopRecording();
        void Mute();
        void Unmute();
        void SetInputDeviceState(InputDeviceState newState);
    }
}
