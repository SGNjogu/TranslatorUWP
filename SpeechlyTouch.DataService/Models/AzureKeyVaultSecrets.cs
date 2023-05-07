using Newtonsoft.Json;
using SQLite;
using System;

namespace SpeechlyTouch.DataService.Models
{
    public class AzureKeyVaultSecrets : BaseModel
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string AzureKey { get; set; }
        public string AzureRegion { get; set; }

        [JsonProperty("UWPAppCenterKeyStaging")]
        public string AppCenterKeyStaging { get; set; }

        [JsonProperty("AppCenterKey-UWP")]
        public string AppCenterKey { get; set; }

        [JsonProperty("MailGunApiKey")]
        public string MailGunApiKey { get; set; }

        [JsonProperty("AB2CClientId-UWP")]
        public string AB2CClientId { get; set; }

        public string GoogleTranslateCredentials { get; set; }

        public string ConferenceAnnualLicenseSku { get; set; }  //Speechly Conference (Annual) 
        public string ConferenceMonthlyLicenseSku { get; set; }  //Speechly Conference (Monthly) 
        public string BusinessAnnualLicenseSku { get; set; }  //Speechly Business (Annual) 
        public string BusinessMonthlyLicenseSku { get; set; }  //Speechly Business (Monthly) 
        public string AzureStorageConnectionString { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
