namespace SpeechlyTouch.Core.Domain
{
    public class OrganizationSettings : BaseEntity
    {
        public int OrganizationId { get; set; }
        public int PlaybackMinutesLimit { get; set; }
        public int? TranslationMinutesLimit { get; set; }
        public bool AllowExplicitContent { get; set; }
        public bool CopyPasteEnabled { get; set; }
        public bool ExportEnabled { get; set; }
        public bool HistoryPlaybackEnabled { get; set; }
        public bool HistoryAudioPlaybackEnabled { get; set; }
        public bool AutoUpdateDesktopApp { get; set; }
        public bool AutoUpdateIOTApp { get; set; }
        public int LanguageId { get; set; }
        public bool EnableSessionTags { get; set; }
        public string LanguageCode { get; set; }
        public bool AudioEnhancement { get; set; }
    }
}
