
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SQLite;
using System;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService.Services
{
    public partial class DataService : IDataService
    {
        #region properties

        SQLiteAsyncConnection Dataservice;
        bool initialized = false;

        #endregion

        #region Methods

        private Lazy<SQLiteAsyncConnection> Initialize(string dataBasePath)
        {
            var db = new SQLiteAsyncConnection(dataBasePath, Constants.Flags);
            return new Lazy<SQLiteAsyncConnection>(() => db);
        }

        /// <summary>
        /// Method to Initialize Database
        /// </summary>
        public async Task InitializeAsync(string dataBasePath)
        {
            // Initialization
            if (!initialized)
            {
                Dataservice = Initialize(dataBasePath).Value;
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(Session)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(Transcription)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(Device)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(PlaybackUsage)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(InputDevice)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(OutputDevice)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(AzureKeyVaultSecrets)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(UserFeedback)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(SyncStatus)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(OrganizationSettings)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(SessionTag)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(OrganizationTag)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(Reseller)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(ReleaseNote)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(InternationalizationLanguage)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(CustomTag)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(UsageLimit)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(OrgQuestions)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(BackendLanguage)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(CustomProfile)).ConfigureAwait(false);
                initialized = true;
            }
        }

        #endregion
    }
}
