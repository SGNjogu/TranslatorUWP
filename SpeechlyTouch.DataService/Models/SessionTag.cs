namespace SpeechlyTouch.DataService.Models
{
    public class SessionTag : BaseModel
    {
        public int OrganizationTagId { get; set; }
        public int SessionId { get; set; }
        public int OrganizationId { get; set; }
        public string TagValue { get; set; }
        public bool SyncedToServer { get; set; } = false;
    }
}
