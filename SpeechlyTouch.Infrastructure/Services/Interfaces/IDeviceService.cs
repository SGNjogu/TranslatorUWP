using SpeechlyTouch.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IDeviceService
    {
        Task<Device> Create(Device device, string token);
        Task<IEnumerable<Device>> GetDevicesForSession(int sessionId, string token);
    }
}