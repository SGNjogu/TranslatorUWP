using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.AuditTracking
{
    public class AppAnalytics : IAppAnalytics
    {
        public AppAnalytics()
        {
        }

        /// <summary>
        /// Method to Initialize Analytics
        /// </summary>
        private void InitializeAnalytics(string appCenterKey)
        {
            // Initialize AppCenter
            AppCenter.Start(appCenterKey, typeof(Analytics), typeof(Crashes));
        }

        /// <summary>
        /// Method to capture custom event
        /// </summary>
        /// <param name="customEventName">Takes in eventName</param>
        /// <param name="eventProperties">Takes in properties of the event (optional)
        /// e.g new Dictionary<string, string> {{ "Category", "Music" },{ "FileName", "favorite.avi"}} </param>
        public void CaptureCustomEvent(string customEventName, Dictionary<string, string> eventProperties = null)
        {
            Analytics.TrackEvent(customEventName, eventProperties);
        }

        /// <summary>
        /// Method to enable or disable analytics at Runtime
        /// This method does not have to be called
        /// unless analytics has been explicitly disabled at runtime
        /// and needs to be re-enabled at runtime
        /// </summary>
        /// <param name="isEnabled"></param>
        public async Task EnableAnalytics(bool isEnabled = true)
        {
            await Analytics.SetEnabledAsync(isEnabled);
        }

        /// <summary>
        /// Method to check if Analytics has been enabled
        /// </summary>
        /// <returns>True if Analytics has been enabled</returns>
        public async Task<bool> IsAnalyticsEnabled()
        {
            return await Analytics.IsEnabledAsync();
        }
    }
}
