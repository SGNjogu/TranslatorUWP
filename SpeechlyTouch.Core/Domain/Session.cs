namespace SpeechlyTouch.Core.Domain
{
    public class Session : BaseEntity
    {
        public long StartTime { get; set; }

        public long EndTime { get; set; }

        public string Duration { get; set; }

        public int UserId { get; set; }

        public int OrganizationId { get; set; }

        public string SourceLangISO { get; set; }

        public string TargetLangIso { get; set; }

        public string SoftwareVersion { get; set; }

        public string ClientType { get; set; }

        public string SessionUrl { get; set; }

        public string SessionNumber { get; set; }

        public double BillableSeconds { get; set; }

        public string SessionName { get; set; }

        public string CustomTags { get; set; }
    }
}
