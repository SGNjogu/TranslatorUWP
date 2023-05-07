using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.AudioProfileService
{
    public interface IAudioProfileService
    {
        Task<List<InputDevice>> GetInputDevices();
        Task<List<OutputDevice>> GetOutputDevices();
    }
}