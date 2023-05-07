namespace SpeechlyTouch.Core.Domain
{
    public class SessionTag
    {
        public int OrganizationTagId { get; set; }
        public int SessionId { get; set; }
        public int OrganizationId { get; set; }
        public string TagValue { get; set; } = "";
    }
}
