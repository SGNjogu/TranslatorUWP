using Newtonsoft.Json;
using SpeechlyTouch.DataService.Helpers;
using SQLite;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;

namespace SpeechlyTouch.DataService.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    [Table("Session")]
    public class Session : BaseModel, ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        private long _startTime;
        [JsonProperty("startTime")]
        public long StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }

        private long _endTime;
        [JsonProperty("endTime")]
        public long EndTime
        {
            get { return _endTime; }
            set
            {
                SetProperty(ref _endTime, value);
                try
                {
                    SessionDuration = string.Format("{0:0.00}", DateTimeUtility.ReturnDurationInMinutesFromLongSeconds(startTime: StartTime, endTime: EndTime));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("customerId")]
        public int CustomerId { get; set; }

        [JsonProperty("ingramMicroCustomerId")]
        public string IngramMicroCustomerId { get; set; }

        [JsonIgnore]
        public int OrganizationId { get; set; }

        private string _sourceLangISO;
        [JsonProperty("sourceLangISO")]
        public string SourceLangISO
        {
            get { return _sourceLangISO; }
            set { SetProperty(ref _sourceLangISO, value); }
        }

        private string _targetLangIso;
        [JsonProperty("targetLangIso")]
        public string TargetLangIso
        {
            get { return _targetLangIso; }
            set { SetProperty(ref _targetLangIso, value); }
        }

        [JsonProperty("softwareVersion")]
        public string SoftwareVersion { get; set; }

        [JsonProperty("clientType")]
        public string ClientType { get; set; }

        [JsonProperty("licenseKeyUsed")]
        public string LicenseKeyUsed { get; set; }

        [JsonProperty("duration")]
        private string _duration;
        public string Duration
        {
            get { return _duration; }
            set
            {
                SetProperty(ref _duration, value);
                if (string.IsNullOrEmpty(Duration) && StartTime > 0 && EndTime > 0)
                {
                    var rawStart = DateTimeOffset.FromUnixTimeSeconds(StartTime).ToLocalTime();
                    var rawEnd = DateTimeOffset.FromUnixTimeSeconds(EndTime).ToLocalTime();
                    var timeDuration = rawEnd - rawStart;
                    Duration = timeDuration.ToString(@"m\:ss");
                }
                else
                {
                    var thisDuration = Duration;
                    string[] split = thisDuration.Split(':');
                    string firstPart = string.Join(":", split.Take(split.Length - 1)); //My. name. is Bond
                    string seconds = split.Last();

                    string[] minSplit = firstPart.Split(':');
                    string secondPart = string.Join(":", minSplit.Take(minSplit.Length - 1));
                    string minutes = minSplit.Last();

                    DisplayDuration = $"{minutes} min {seconds} sec";
                }
            }
        }

        private bool _syncedToServer;
        [JsonIgnore]
        public bool SyncedToServer
        {
            get { return _syncedToServer; }
            set { SetProperty(ref _syncedToServer, value); }
        }

        // History specific attributes
        private string _recordDate;
        [JsonIgnore]
        public string RecordDate
        {
            get { return _recordDate; }
            set { SetProperty(ref _recordDate, value); }
        }

        private string _rawStartTime;
        [JsonIgnore]
        public string RawStartTime
        {
            get { return _rawStartTime; }
            set { SetProperty(ref _rawStartTime, value); }
        }

        private string _rawEndTime;
        [JsonIgnore]
        public string RawEndTime
        {
            get { return _rawEndTime; }
            set { SetProperty(ref _rawEndTime, value); }
        }

        private string _sourceLanguage;
        [JsonIgnore]
        public string SourceLanguage
        {
            get { return _sourceLanguage; }
            set
            {
                SetProperty(ref _sourceLanguage, value);
                if (!string.IsNullOrEmpty(SourceLanguage))
                    DisplaySourceLanguage = SourceLanguage.Substring(0, SourceLanguage.IndexOf(" "));
            }
        }

        private string _targeLanguage;
        [JsonIgnore]
        public string TargeLanguage
        {
            get { return _targeLanguage; }
            set
            {
                SetProperty(ref _targeLanguage, value);
                if (!string.IsNullOrEmpty(TargeLanguage))
                    DisplayTargetLanguage = TargeLanguage.Substring(0, TargeLanguage.IndexOf(" "));
            }
        }

        private string _sessionDuration;
        [JsonIgnore]
        public string SessionDuration
        {
            get { return _sessionDuration; }
            set { SetProperty(ref _sessionDuration, value); }
        }

        private string _displayDuration;
        [JsonIgnore]
        public string DisplayDuration
        {
            get { return _displayDuration; }
            set { SetProperty(ref _displayDuration, value); }
        }

        private string _displayDate;
        [JsonIgnore]
        public string DisplayDate
        {
            get { return _displayDate; }
            set { SetProperty(ref _displayDate, value); }
        }

        private string _displaySourceLanguage;
        [JsonIgnore]
        public string DisplaySourceLanguage
        {
            get { return _displaySourceLanguage; }
            set { SetProperty(ref _displaySourceLanguage, value); }
        }

        private string _displayTargetLanguage;
        [JsonIgnore]
        public string DisplayTargetLanguage
        {
            get { return _displayTargetLanguage; }
            set { SetProperty(ref _displayTargetLanguage, value); }
        }

        [JsonProperty("billableSeconds")]
        public double BillableSeconds { get; set; }

        [JsonProperty("sessionNumber")]
        public string SessionNumber { get; set; } = "";

        private string _syncText = "Syncing";
        [JsonIgnore]
        public string SyncText
        {
            get { return _syncText; }
            set { SetProperty(ref _syncText, value); }
        }

        private bool _allModelsSyncedToServer = false;
        [JsonIgnore]
        public bool AllModelsSyncedToServer
        {
            get { return _allModelsSyncedToServer; }
            set { SetProperty(ref _allModelsSyncedToServer, value); }
        }

        private string _sessionUrl;
        [JsonProperty("sessionUrl")]
        public string SessionUrl
        {
            get { return _sessionUrl; }
            set { SetProperty(ref _sessionUrl, value); }
        }

        private string _sessionName = string.Empty;
        [JsonProperty("sessionName")]
        public string SessionName
        {
            get { return _sessionName; }
            set { SetProperty(ref _sessionName, value); }
        }

        [JsonProperty("customTags")]
        public string CustomTags { get; set; } = string.Empty;

        [JsonIgnore]
        public bool IsSingleDeviceSession { get; set; } = true;

        private string _syncedIconFillColor;
        [JsonIgnore]
        public string SyncedIconFillColor
        {
            get { return _syncedIconFillColor; }
            set { SetProperty(ref _syncedIconFillColor, value); }
        }

        private string _syncedIconGlyph;
        [JsonIgnore]
        public string SyncedIconGlyph
        {
            get { return _syncedIconGlyph; }
            set { SetProperty(ref _syncedIconGlyph, value); }
        }

        private bool _isConferenceCall = false;
        [JsonIgnore]
        public bool IsConferenceCall
        {
            get { return _isConferenceCall; }
            set
            {
                SetProperty(ref _isConferenceCall, value);
                if (IsConferenceCall)
                {
                    IsVisibleConferenceCallIcon = Visibility.Visible;
                }
                else
                {
                    IsVisibleConferenceCallIcon = Visibility.Collapsed;
                }
            }
        }

        private Visibility _isVisibleConferenceCallIcon = Visibility.Collapsed;
        [JsonIgnore]
        [Ignore]
        public Visibility IsVisibleConferenceCallIcon
        {
            get { return _isVisibleConferenceCallIcon; }
            set { SetProperty(ref _isVisibleConferenceCallIcon, value); }
        }

        public bool IsEmailRequired { get; set; }
        public bool IsEmailSent { get; set; }
        public string EmailingAddress { get; set; }

        public object Clone()
        {
            return (Session)MemberwiseClone();
        }
    }
}
