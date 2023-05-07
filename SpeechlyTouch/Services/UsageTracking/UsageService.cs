using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.Settings;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.UsageTracking
{
    public class UsageService : IUsageService
    {
        private readonly IUsageTrackingService _usageTrackingService;
        private readonly ISettingsService _settingsService;
        private readonly IDataService _dataService;
        private readonly IAuthService _authService;

        public UsageService
            (
            IUsageTrackingService usageTrackingService,
            ISettingsService settingsService,
            IDataService dataService,
            IAuthService authService
            )
        {
            _usageTrackingService = usageTrackingService;
            _settingsService = settingsService;
            _dataService = dataService;
            _authService = authService;
        }

        public async Task GetUsageLimits()
        {
            try
            {
                var user = await _settingsService.GetUser();
                var organizationUsageLimit = await _usageTrackingService.GetOrganizationUsageLimit(user.OrganizationId, _authService.IdToken);
                var userUsageLimit = await _usageTrackingService.GetUserUsageLimit((int)user.UserIntID, _authService.IdToken);

                var usageLimit = new UsageLimit();

                if (organizationUsageLimit != null)
                {
                    usageLimit.OrganizationBillingType = organizationUsageLimit.BillingType;
                    usageLimit.OrganizationLicensingType = organizationUsageLimit.LicensingType;
                    usageLimit.OrganizationStorageLimitExceeded = organizationUsageLimit.StorageLimitExceeded;
                    usageLimit.OrganizationTranslationLimitExceeded = organizationUsageLimit.TranslationLimitExceeded;
                }

                if (userUsageLimit != null)
                {
                    usageLimit.UserMaxSessionTime = userUsageLimit.MaxSessionTime;
                    usageLimit.UserStorageBytes = userUsageLimit.StorageBytes;
                    usageLimit.UserStorageTimeframe = userUsageLimit.StorageTimeframe;
                    usageLimit.UserTranslationMinutes = userUsageLimit.TranslationMinutes;
                    usageLimit.UserTranslationTimeframe = userUsageLimit.TranslationTimeframe;
                }

                await _dataService.CreateUsageLimit(usageLimit);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
