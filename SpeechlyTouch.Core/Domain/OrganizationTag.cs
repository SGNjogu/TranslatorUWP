namespace SpeechlyTouch.Core.Domain
{
    public class OrganizationTag
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string TagName { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsShownOnApp { get; set; }
    }
}
