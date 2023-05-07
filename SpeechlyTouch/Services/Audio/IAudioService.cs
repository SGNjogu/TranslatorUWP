using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.Audio
{
    public interface IAudioService
    {
        Task<List<InputDevice>> InputDevices();
        Task LoadDeviceLists();
        Task LoadInputDevices();
        Task LoadOutputDevices();
        Task<List<OutputDevice>> OutputDevices();
        Task<InputDevice> ParticipantOneInputDevice();
        Task<OutputDevice> ParticipantOneOutputDevice();
        Task<InputDevice> ParticipantTwoInputDevice();
        Task<OutputDevice> ParticipantTwoOutputDevice();
    }
}
