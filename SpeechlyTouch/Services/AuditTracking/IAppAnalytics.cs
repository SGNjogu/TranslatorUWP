using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.AuditTracking
{
    public interface IAppAnalytics
    {
        void CaptureCustomEvent(string customEventName, Dictionary<string, string> eventProperties = null);
        Task EnableAnalytics(bool isEnabled = true);
        Task<bool> IsAnalyticsEnabled();
    }
}