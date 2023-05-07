using System.Collections.Generic;

namespace SpeechlyTouch.Models
{
    public class SpeechlyUser
    {
        public int Id { get; set; }
        public string ObjectId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public int ResellerId { get; set; }
        public Reseller Reseller { get; set; }
        public bool AccountEnabled { get; set; }
        public bool Deleted { get; set; }
        public bool IsActive { get; set; }
        public List<UserRole> UserRoles { get; set; }
        public List<UserLicense> userLicenses { get; set; }
    }
}
