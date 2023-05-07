using Microsoft.AppCenter.Crashes;
using SpeechlyTouch.Core.Services.CongnitiveService;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.Settings;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.CognitiveService
{
    public class CognitiveServicesHelper : ICognitiveServicesHelper
    {
        private readonly ISettingsService _settingsService;
        private readonly ICognitiveEndpointsService _cognitiveServicesMethods;
        private readonly IAppAnalytics _appAnalytics;
        private readonly IAuthService _authService;
        private readonly ICrashlytics _crashlytics;

        private static string AccessKey { get; set; }
        private static string Region { get; set; }

        public CognitiveServicesHelper
            (
            ISettingsService settingsService,
            ICognitiveEndpointsService cognitiveServicesMethods,
            IAppAnalytics appAnalytics,
            IAuthService authService,
            ICrashlytics crashlytics
            )
        {
            _settingsService = settingsService;
            _cognitiveServicesMethods = cognitiveServicesMethods;
            _appAnalytics = appAnalytics;
            _authService = authService;
            _crashlytics = crashlytics;
        }

        //Allocate AccessKey and Region for each session
        public async Task<CognitiveEndpoint> GetAccessKeyAndRegionAsync()
        {
            try
            {
                //Allocate endpoint
                var cognitiveServicesEndpoint = await _cognitiveServicesMethods.AllocateCognitiveEndpointId(_authService.IdToken);
                if (cognitiveServicesEndpoint != null)
                {
                    AccessKey = cognitiveServicesEndpoint.accessKey;
                    Region = cognitiveServicesEndpoint.region;
                    PersistEndpointIdToDatabase(cognitiveServicesEndpoint.id.ToString());
                }
                else
                {
                    _appAnalytics.CaptureCustomEvent("Failed to Allocate endpoint. Default endpoint used.");
                    AccessKey = Constants.AzureKey;
                    Region = Constants.AzureRegion;
                }
            }
            catch (Exception ex)
            {
                AccessKey = Constants.AzureKey;
                Region = Constants.AzureRegion;

                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return new CognitiveEndpoint
            {
                AccessKey = AccessKey,
                Region = Region
            };
        }

        //Save the Current EndpointId in the database
        private void PersistEndpointIdToDatabase(string endpointId)
        {
            _settingsService.CurrentEndpointId = endpointId;
        }

        //Deallocate the Endpoints assigned for the session
        public async Task DeleteCognitiveServicesEndpointId()
        {
            try
            {
                if (_settingsService.CurrentEndpointId != null)
                {
                    bool isEndpointDeallocated = await _cognitiveServicesMethods.DeallocateCognitiveEndpointId(_settingsService.CurrentEndpointId, _authService.IdToken);
                    if (isEndpointDeallocated)
                    {
                        //Delete CurrentEndpointId from the database
                        _settingsService.CurrentEndpointId = null;
                    }
                    else
                    {
                        //Second attempt to deallocate endpoint
                        isEndpointDeallocated = await _cognitiveServicesMethods.DeallocateCognitiveEndpointId(_settingsService.CurrentEndpointId, _authService.IdToken);
                        if (isEndpointDeallocated)
                        {
                            //Delete CurrentEndpointId from the database
                            _settingsService.CurrentEndpointId = null;
                        }
                        else
                        {
                            //Sends endpoint Id to App Center
                            _appAnalytics.CaptureCustomEvent($"EndpointId {_settingsService.CurrentEndpointId} unsuccessfully deallocated");

                            //Delete CurrentEndpointId from the database anyway
                            //Allows the user to make subsequent sessions
                            _settingsService.CurrentEndpointId = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
            finally
            {
                //Reset values for Accesskey and Region
                AccessKey = null;
                Region = null;
            }
        }
    }
}

public class CognitiveEndpoint
{
    public string AccessKey { get; set; }
    public string Region { get; set; }
}
