using Microsoft.AppCenter.Crashes;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Enums;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.Auth
{
    public class AuthService : IAuthService
    {
        public UserLicense UserLicense { get; private set; }

        private readonly ISettingsService _settings;
        private readonly IDataService _dataService;
        private readonly IOrganizationSettingsService _organizationSettingsService;
        private readonly ICustomTagService _customTagService;
        private readonly IQuestionsService _questionsService;
        private readonly IBackendLanguageService _backendLanguageService;
        private readonly ICrashlytics _crashlytics;
        private readonly IAppAnalytics _appAnalytics;

        public string IdToken { get; private set; }
        public string ErrorMessage { get; private set; }

        public AuthService(
            ISettingsService settings,
            IDataService dataService,
            IOrganizationSettingsService organizationSettingsService,
            ICustomTagService customTagService,
            IQuestionsService questionsService,
            IBackendLanguageService backendLanguageService,
            ICrashlytics crashlytics,
            IAppAnalytics appAnalytics
            )
        {
            _settings = settings;
            _dataService = dataService;
            _organizationSettingsService = organizationSettingsService;
            _customTagService = customTagService;
            _questionsService = questionsService;
            _backendLanguageService = backendLanguageService;
            _crashlytics = crashlytics;
            _appAnalytics = appAnalytics;
        }

        private void Initialize()
        {
            if (App.PublicClientApp == null)
            {
                App.PublicClientApp = PublicClientApplicationBuilder.Create(Constants.AB2CClientId)
                    .WithB2CAuthority(Constants.Authority)
                    .WithRedirectUri($"msal{Constants.AB2CClientId}://auth")
                    .Build();
            }
        }

        public async Task<AuthenticationObject> Login()
        {
            Initialize();
            AuthenticationResult authResult = null;
            var app = App.PublicClientApp;

            var accounts = await app.GetAccountsAsync();

            IEnumerable<IAccount> accountsArray = accounts as IAccount[] ?? accounts.ToArray();
            foreach (var account in accountsArray)
            {
                Console.WriteLine(account);
            }

            var firstAccount = accountsArray.FirstOrDefault();

            try
            {
                authResult = await app.AcquireTokenSilent(Constants.Scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                try
                {
                    authResult = await App.PublicClientApp.AcquireTokenInteractive(Constants.Scopes)
                        .WithAccount(GetAccountByPolicy(accountsArray, Constants.AB2CPolicySignUpSignIn))
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();
                }
                catch (MsalException ex)
                {
                    Debug.WriteLine($"Error Acquiring Token:{ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error Acquiring Token:{ex.Message}");
                    Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Acquiring Token Silently:{Environment.NewLine}{ex}");
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            if (authResult != null)
            {
                SpeechlyUserData speechlyUserData = new SpeechlyUserData();
                var azureAdB2cUser = ValidateToken(authResult.IdToken);
                string email_objectId;
                if (string.IsNullOrEmpty(azureAdB2cUser.EmailAddress) && !string.IsNullOrEmpty(azureAdB2cUser.UserId))
                {
                    email_objectId = azureAdB2cUser.UserId;
                }
                else
                {
                    email_objectId = azureAdB2cUser.EmailAddress;
                }

                if (email_objectId == "97a31129-7393-4b72-bc7f-e1306a6ddeee")
                {
                    email_objectId = "speechlytestuser@outlook.com";
                }

                email_objectId = "jgreene@jabra.com";

                speechlyUserData = await GetBackendUserData(email_objectId, authResult.IdToken);
                ErrorMessage = speechlyUserData?.ErrorMessage;

                if (speechlyUserData?.User == null)
                {
                    foreach (var account in accountsArray)
                        await App.PublicClientApp.RemoveAsync(account);
                }
                else
                {
                    if (speechlyUserData.CanLogIn)
                    {
                        await SaveUser(speechlyUserData.User, authResult.IdToken, speechlyUserData.HasValidSpeechlyCoreLicense);
                    }
                    else
                    {
                        foreach (var account in accountsArray)
                            await App.PublicClientApp.RemoveAsync(account);
                    }
                }

                return new AuthenticationObject { AuthResult = authResult, SpeechlyUserData = speechlyUserData };
            }
            else
            {
                return new AuthenticationObject { AuthResult = authResult, SpeechlyUserData = null };
            }
        }

        private IAccount GetAccountByPolicy(IEnumerable<IAccount> accounts, string policy)
        {
            foreach (var account in accounts)
            {
                string userIdentifier = account.HomeAccountId.ObjectId.Split('.')[0];
                if (userIdentifier.EndsWith(policy.ToLower())) return account;
            }

            return null;
        }

        private AzureAdB2cUser ValidateToken(string jwtToken)
        {
            IdToken = jwtToken;
            string stream = jwtToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            Microsoft.IdentityModel.Tokens.SecurityToken jsonToken = handler.ReadToken(stream);
            JwtSecurityToken tokenS = jsonToken as JwtSecurityToken;

            AzureAdB2cUser azureAdB2CUser = new AzureAdB2cUser()
            {
                TenantId = tokenS.Claims.FirstOrDefault(claim => claim.Type == "tid")?.Value,
                UserId = tokenS.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value,
                FirstName = tokenS.Claims.FirstOrDefault(claim => claim.Type == "given_name")?.Value,
                LastName = tokenS.Claims.FirstOrDefault(claim => claim.Type == "family_name")?.Value,
                UserName = tokenS.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value,
                EmailAddress = tokenS.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value
            };
            return azureAdB2CUser;
        }

        private async Task<SpeechlyUserData> GetBackendUserData(string email_objectId, string idToken)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", idToken);

                HttpResponseMessage get_response = await client.GetAsync($"{Constants.BackendAPiEndpoint}{Constants.AzureADB2CUserDataEndpoint}/{email_objectId}");

                string get_content = await get_response.Content.ReadAsStringAsync();

                if ((int)get_response.StatusCode == 200)
                {
                    Debug.WriteLine($"SUCCESS GET USER: {get_response.StatusCode}");
                    return JsonConvert.DeserializeObject<SpeechlyUserData>(get_content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
            return null;
        }

        private async Task SaveUser(SpeechlyUser speechlyUser, string token, bool hasValidLicense)
        {
            try
            {
                var user = new User
                {
                    IsLoggedIn = true,
                    UserIntID = speechlyUser.Id,
                    UserEmail = speechlyUser.Email,
                    UserName = $"{speechlyUser.FirstName} {speechlyUser.LastName}",
                    UserType = "Member",
                    UserID = speechlyUser.ObjectId,
                    OrganizationId = speechlyUser.OrganizationId,
                    DomainName = DomainName(speechlyUser.Email),
                    DataConsentStatus = speechlyUser.Organization != null ? speechlyUser.Organization.DataConsentStatus : true,
                    IsJabraLocked = speechlyUser.Organization != null ? speechlyUser.Organization.LockToJabraDevices : false,
                    HasValidLicense = hasValidLicense,
                    HasProfileImage = false,
                    Organization = speechlyUser.Organization != null ? speechlyUser.Organization.Name : string.Empty
                };

                if (speechlyUser.userLicenses != null && speechlyUser.userLicenses.Any())
                {
                    var policyType = speechlyUser.userLicenses.Find(l => l.licensePolicy.sku == Constants.BusinessAnnualLicenseSku || l.licensePolicy.sku == Constants.BusinessMonthlyLicenseSku);

                    if (policyType != null)
                    {
                        user.PolicyType = policyType.licensePolicy.name;
                        user.PolicyExpiryDate = policyType.expiryDate;
                    }
                }

                await _settings.SaveUser(user);
                await UpdateResellerInfo(speechlyUser.Reseller);

                _appAnalytics.CaptureCustomEvent("Sign In",
           new Dictionary<string, string> {
                            { "Organization", user.Organization },
                            { "User", user.UserEmail },
                            { "Licence", user.PolicyType },
                            { "App Version", Constants.GetSoftwareVersion() } });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task UpdateResellerInfo(Models.Reseller reseller)
        {
            if (reseller != null)
            {
                await _dataService.DeleteAllItemsAsync<DataService.Models.Reseller>().ConfigureAwait(true);

                var newReseller = new DataService.Models.Reseller
                {
                    Address = reseller.Address,
                    Email = reseller.Email,
                    Name = reseller.Name,
                    PhoneNumber = reseller.PhoneNumber,
                    Website = reseller.Website
                };

                await _dataService.AddItemAsync<DataService.Models.Reseller>(newReseller);
            }
        }
        public async Task Logout()
        {
            try
            {
                Initialize();
                IEnumerable<IAccount> accounts = await App.PublicClientApp.GetAccountsAsync();
                User user = await _settings.GetUser();

                var accountsArray = accounts as IAccount[] ?? accounts.ToArray();
                if (accountsArray.Any())
                {
                    foreach (var account in accountsArray)
                        await App.PublicClientApp.RemoveAsync(account);

                    if (user != null)
                    {
                        _appAnalytics.CaptureCustomEvent("Sign Out",
           new Dictionary<string, string> {
                            { "User", user.UserEmail },
                            { "Organisation", user.Organization },
                            { "App Version", Constants.GetSoftwareVersion() } });

                        user.UserName = null;
                        user.UserID = null;
                        user.UserEmail = null;
                        user.UserType = null;
                        user.TenantID = null;
                        user.DomainName = null;
                        user.Organization = null;
                        user.IsLoggedIn = false;
                        user.CanShowHelpTips = true;
                        user.DataConsentStatus = true;
                        user.UserIntID = null;
                        user.OrganizationId = 0;
                        user.DataConsentStatus = false;
                        user.IsJabraLocked = false;
                        user.HasValidLicense = false;

                        await _settings.SaveUser(user);
                    }

                    await _dataService.DeleteAllItemsAsync<Session>();
                    await _dataService.DeleteAllItemsAsync<DataService.Models.SessionTag>();
                    await _dataService.DeleteAllItemsAsync<Transcription>();
                    await _dataService.DeleteAllItemsAsync<InputDevice>();
                    await _dataService.DeleteAllItemsAsync<OutputDevice>();
                    await _dataService.DeleteAllItemsAsync<Device>();
                    await _dataService.DeleteAllItemsAsync<UserFeedback>();
                    await _dataService.DeleteAllItemsAsync<PlaybackUsage>();
                    await _dataService.DeleteAllItemsAsync<OrganizationSettings>();
                    await _dataService.DeleteAllItemsAsync<OrganizationTag>();
                    await _dataService.DeleteAllItemsAsync<DataService.Models.Reseller>();
                    await _dataService.DeleteAllItemsAsync<SyncStatus>();
                    await _dataService.DeleteAllItemsAsync<CustomTag>();
                    await _dataService.DeleteAllItemsAsync<CustomProfile>();
                    await _dataService.DeleteAllItemsAsync<OrgQuestions>();
                    await _dataService.DeleteAllItemsAsync<UsageLimit>();


                    _settings.IsUserLoggedIn = false;
                    _settings.Passcode = null;
                    _settings.AdminModeTimeout = 1;
                    _settings.DefaultTranslationLanguageCode = "en-GB";
                    _settings.TargetTranslationLanguageCode = "fr-FR";
                    _settings.ApplicationLanguageCode = "English";
                    _settings.QuickViewLanguages = null;
                    _settings.CurrentEndpointId = null;
                    _settings.IsResetPasscodeEmailSent = false;
                    _settings.DefaultPlaybackLanguage = "Language 1";
                    _settings.IsUserDoneSettingUp = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task<AuthenticationObject> AttemptSilentLogin()
        {
            Initialize();
            AuthenticationResult authResult = null;
            SpeechlyUserData speechlyUserData = new SpeechlyUserData();
            var app = App.PublicClientApp;
            var accounts = await app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await app.AcquireTokenSilent(Constants.Scopes, firstAccount)
                    .ExecuteAsync();

                if (authResult != null)
                {
                    var azureAdB2cUser = ValidateToken(authResult.IdToken);
                    string email_objectId;
                    if (string.IsNullOrEmpty(azureAdB2cUser.EmailAddress) && !string.IsNullOrEmpty(azureAdB2cUser.UserId))
                    {
                        email_objectId = azureAdB2cUser.UserId;
                    }
                    else
                    {
                        email_objectId = azureAdB2cUser.EmailAddress;
                    }

                    email_objectId = "jgreene@jabra.com";
                    speechlyUserData = await GetBackendUserData(email_objectId, authResult.IdToken);
                    ErrorMessage = speechlyUserData?.ErrorMessage;

                    if (speechlyUserData?.User == null)
                    {
                        authResult = null;

                        foreach (var account in accounts)
                            await App.PublicClientApp.RemoveAsync(account);
                    }
                    else
                    {
                        if (speechlyUserData.CanLogIn)
                        {
                            await SaveUser(speechlyUserData.User, authResult.IdToken, speechlyUserData.HasValidSpeechlyCoreLicense);
                        }
                        else
                        {
                            await Logout();
                        }
                    }
                }
                else
                {
                    await Logout();
                }

                return new AuthenticationObject { AuthResult = authResult, SpeechlyUserData = speechlyUserData };
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent. 
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return null;
        }

        private string DomainName(string email)
        {
            int atStringPosition = email.IndexOf("@");
            string domain = email.Substring(atStringPosition + 1);
            return domain;
        }

        public async Task<OrganizationBranding> GetOrganizationBranding()
        {
            try
            {
                var user = await _settings.GetUser();
                var currentOrganizationId = user.OrganizationId;

                //Fetch organization branding details
                var client = new HttpClient();

                HttpResponseMessage get_response = await client.GetAsync($"{Constants.BackendAPiEndpoint}{Constants.OrganizationBrandingEndpoint}/{currentOrganizationId}");

                string get_content = await get_response.Content.ReadAsStringAsync();

                if ((int)get_response.StatusCode == 200)
                {
                    var responseBranding = JsonConvert.DeserializeObject<OrganizationBranding>(get_content);
                    OrganizationBranding branding = new OrganizationBranding
                    {
                        OrganizationId = responseBranding.OrganizationId,
                        LogoImageURL = responseBranding.LogoImageURL,
                        WebsiteURL = responseBranding.WebsiteURL,
                        OrganizationName = responseBranding.OrganizationName
                    };
                    user.Organization = branding.OrganizationName;
                    await _settings.SaveUser(user);
                    return branding;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                return null;
            }
        }

        public async Task GetOrganizationSettings()
        {
            try
            {
                var userSettings = await _settings.GetUser();

                if (userSettings.IsLoggedIn)
                {
                    var organizationSettings = await _dataService.GetOrganizationSettingsAsync();
                    var updatedOrganizationSettings = await _organizationSettingsService.GetUserOrganizationSettings(userSettings.OrganizationId, IdToken).ConfigureAwait(true);
                    if (updatedOrganizationSettings != null)
                    {
                        await _dataService.DeleteAllItemsAsync<OrganizationSettings>().ConfigureAwait(true);
                        await _dataService.AddItemAsync<OrganizationSettings>(new OrganizationSettings
                        {
                            OrganizationId = updatedOrganizationSettings.OrganizationId,
                            PlaybackMinutesLimit = updatedOrganizationSettings.PlaybackMinutesLimit,
                            TranslationMinutesLimit = updatedOrganizationSettings.TranslationMinutesLimit,
                            AllowExplicitContent = updatedOrganizationSettings.AllowExplicitContent,
                            CopyPasteEnabled = updatedOrganizationSettings.CopyPasteEnabled,
                            ExportEnabled = updatedOrganizationSettings.ExportEnabled,
                            HistoryPlaybackEnabled = updatedOrganizationSettings.HistoryPlaybackEnabled,
                            HistoryAudioPlaybackEnabled = updatedOrganizationSettings.HistoryAudioPlaybackEnabled,
                            AutoUpdateDesktopApp = updatedOrganizationSettings.AutoUpdateDesktopApp,
                            AutoUpdateIOTApp = updatedOrganizationSettings.AutoUpdateIOTApp,
                            LanguageId = updatedOrganizationSettings.LanguageId,
                            EnableSessionTags = updatedOrganizationSettings.EnableSessionTags,
                            LanguageCode = updatedOrganizationSettings.LanguageCode,
                            EnableAudioEnhancement = updatedOrganizationSettings.AudioEnhancement
                        });

                        if (!string.IsNullOrEmpty(updatedOrganizationSettings.LanguageCode) && updatedOrganizationSettings.LanguageCode != "string")
                        {
                            if (string.IsNullOrEmpty(_settings.DefaultTranslationLanguageCode))
                            {
                                _settings.DefaultTranslationLanguageCode = updatedOrganizationSettings.LanguageCode;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        /// <summary>
        /// Gets and/or updates OrganizationTags for the currently logged in user
        /// </summary>
        /// <returns></returns>
        public async Task GetOrganizationTags()
        {
            try
            {
                var userSettings = await _settings.GetUser();

                if (userSettings.IsLoggedIn)
                {
                    await _dataService.DeleteAllItemsAsync<OrganizationTag>().ConfigureAwait(true);
                    var updatedOrganizationTags = await _organizationSettingsService.GetUserOrganizationTags(userSettings.OrganizationId, IdToken).ConfigureAwait(true);
                    if (updatedOrganizationTags != null && updatedOrganizationTags.Any())
                    {
                        foreach (var tag in updatedOrganizationTags)
                        {
                            await _dataService.AddItemAsync<OrganizationTag>(new OrganizationTag
                            {
                                OrganizationId = tag.OrganizationId,
                                IsMandatory = tag.IsMandatory,
                                IsShownOnApp = tag.IsShownOnApp,
                                TagName = tag.TagName,
                                OrganizationTagId = tag.Id
                            });
                        }
                    }

                    StrongReferenceMessenger.Default.Send(new CustomTagMessage { Initialize = true }); ;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task GetCustomTags()
        {
            try
            {
                var userSettings = await _settings.GetUser();
                if (userSettings.IsLoggedIn)
                {
                    await _dataService.DeleteAllItemsAsync<CustomTag>().ConfigureAwait(true);
                    var updatedCustomTags = await _customTagService.GetOrganizationCustomTags(userSettings.OrganizationId, IdToken);
                    if (updatedCustomTags.Any() && updatedCustomTags != null)
                    {
                        foreach (var tag in updatedCustomTags)
                        {
                            await _dataService.AddItemAsync<CustomTag>(new CustomTag
                            {
                                TagName = tag.TagName
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task UpdateOrgQuestions()
        {
            try
            {
                var userSettings = await _settings.GetUser();
                if (userSettings != null && userSettings.IsLoggedIn)
                {
                    var localQuestions = await _dataService.GetOrgQuestionsAsync();
                    var currentQuestions = await _questionsService.GetUserQuestions((int)userSettings.UserIntID, IdToken);
                    List<bool> existenceList = new List<bool>();

                    if (currentQuestions.Count() == localQuestions.Count())
                    {
                        //Check whether all old questions exist in the new ones
                        foreach (var item in localQuestions)
                        {
                            var exists = currentQuestions.Exists(q => q.Question == item.Question);
                            existenceList.Add(exists);
                        }
                    }

                    if ((existenceList.Count() == 0 || existenceList.Any(o => o == false)) && currentQuestions.Count() > 0)
                    {
                        await _dataService.DeleteAllItemsAsync<OrgQuestions>().ConfigureAwait(true);
                        int counter = 0;
                        foreach (var question in currentQuestions)
                        {
                            counter++;
                            await _dataService.AddItemAsync<OrgQuestions>(new OrgQuestions
                            {
                                Question = question.Question,
                                LanguageCode = question.LanguageCode,
                                Shortcut = counter < 10 ? counter.ToString() : string.Empty,
                                SyncedToServer = true,
                                QuestionStatus = (int)UserQuestionStatus.Available,
                                QuestionID = question.Id,
                                QuestionType = (int)question.QuestionType
                            });
                        }
                    }

                    StrongReferenceMessenger.Default.Send(new OrgQuestionsMessage { ReloadQuestions = true });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task UpdateBackendLanguages()
        {
            try
            {
                var userSettings = await _settings.GetUser();
                if (userSettings != null && userSettings.IsLoggedIn)
                {
                    var backendLanguages = await _dataService.GetBackendLanguagesAsync();
                    var currentBackendLanguages = await _backendLanguageService.GetUserLanguages(userSettings.UserIntID, IdToken);
                    if (currentBackendLanguages != null && currentBackendLanguages.Any())
                    {
                        await _dataService.DeleteAllItemsAsync<BackendLanguage>().ConfigureAwait(true);

                        foreach (var language in currentBackendLanguages)
                        {
                            await _dataService.AddItemAsync<BackendLanguage>(new BackendLanguage { Code = language.Code, Name = language.Name });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }
    }
}
