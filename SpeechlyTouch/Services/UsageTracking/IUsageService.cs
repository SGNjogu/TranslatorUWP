using System.Threading.Tasks;

namespace SpeechlyTouch.Services.UsageTracking
{
    public interface IUsageService
    {
        Task GetUsageLimits();
    }
}
