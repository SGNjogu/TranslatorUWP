using System;

namespace SpeechlyTouch.Models
{
    public class UserLicense
    {
        public int organizationId { get; set; }
        public int userId { get; set; }
        public int licenseId { get; set; }
        public int licensePolicyId { get; set; }
        public LicensePolicy licensePolicy { get; set; }
        public DateTime startDate { get; set; }
        public DateTime expiryDate { get; set; }
        public DateTime dateAssigned { get; set; }
        public bool active { get; set; }
        public int id { get; set; }
    }
}
