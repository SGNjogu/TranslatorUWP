using SpeechlyTouch.DataService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Interfaces
{
    public interface IDataService
    {
        Task InitializeAsync(string databasePath);
        Task<T> AddItemAsync<T>(object obj);
        Task<T> UpdateItemAsync<T>(object obj);
        Task<T> DeleteItemAsync<T>(object obj);
        Task DeleteAllItemsAsync<T>();
        Task<List<Session>> GetSessionsAsync();
        Task<Session> GetOneSessionAsync(int sessionId);
        Task<List<Transcription>> GetTranscriptionsAsync();
        Task<Transcription> GetOneTranscriptionAsync(int transcriptionId);
        Task<List<Transcription>> GetSessionTranscriptions(int sessionId);
        Task<int> GetSessionCountAsync();
        Task<List<PlaybackUsage>> GetPlaybackUsageAsync();
        Task<List<InputDevice>> GetInputDevicesAsync();
        Task<InputDevice> AddInputDeviceAsync(InputDevice inputDevice);
        Task<List<OutputDevice>> GetOuputDevicesAsync();
        Task<OutputDevice> AddOutputDeviceAsync(OutputDevice outputDevice);
        Task<List<AzureKeyVaultSecrets>> GetKeyVaultKeysAsync();
        Task<AzureKeyVaultSecrets> GetOneKeyVaultSecretAsync(int secretId);
        Task<List<Device>> GetDevicesAsync();
        Task<List<UserFeedback>> GetFeedbackRatingsAsync();
        Task<List<SyncStatus>> GetSyncStatusAsync();
        Task<SyncStatus> GetOneSyncStatusAsync(int syncStatusId);
        Task<int> GetSyncStatusCountAsync();
        Task<bool> IsSessionValid(int sessionId);
        Task DeleteSessionAsync(Session session);
        Task<List<OrganizationSettings>> GetOrganizationSettingsAsync();
        Task<OrganizationSettings> GetOneOrganizationSettingsAsync();
        Task<List<SessionTag>> CreateSessionTags(List<SessionTag> sessionTags, int sessionId);
        Task<List<SessionTag>> GetSessionTags(int sessionId);
        Task DeleteSessionTagAsync(SessionTag sessionTag);
        Task<List<OrganizationTag>> GetOrganizationTagsAsync();
        Task<Reseller> GetFirstResellerInfoAsync();
        Task<List<Reseller>> GetResellersAsync();
        Task<List<ReleaseNote>> CreateReleaseNotes(IEnumerable<ReleaseNote> releaseNotes);
        Task<List<ReleaseNote>> GetReleaseNotes();
        Task CreateInternationalizationLanguages(IEnumerable<InternationalizationLanguage> internationalizationLanguages);
        Task<List<InternationalizationLanguage>> GetInternationalizationLanguages();
        Task<List<CustomTag>> GetCustomTagsAsync();
        Task<UsageLimit> CreateUsageLimit(UsageLimit usageLimit);
        Task<UsageLimit> GetUsageLimit();
        Task<List<OrgQuestions>> GetOrgQuestionsAsync();
        Task<OrgQuestions> GetQuestionAsync(int id);
        Task<List<string>> GetBackendLanguagesAsync();
        Task<List<CustomProfile>> GetCustomProfilesAsync();
        Task<CustomProfile> GetCustomProfile(string name);
    }
}
