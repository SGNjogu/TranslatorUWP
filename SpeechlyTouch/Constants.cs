////STAGING
//using System;
//using System.IO;
//using System.Threading.Tasks;
//using Windows.ApplicationModel;
//using Windows.Storage;

//namespace SpeechlyTouch
//{
//    public static class Constants
//    {
//        public static string KeyVaultUri = "https://speechlykeyvaultstaging.vault.azure.net";
//        public static string VaultAppId = "c2c3ac61-f1a6-463e-90a0-cb965d7dc1c1";
//        public static string TenantId = "f9205e77-06c5-4ff9-a054-97bf1a3d3474";
//        public static string KeygenClientSecret = "C5AIsEO7C~wB.M9__-5lrW6Rzn66Tw4Vc4";
//        public static string AppCenterClientId = "3f538ac5-e806-4f06-aceb-b81b9b8f54c2";
//        public static string MailGunApiKey = "";
//        public static string AzureKey = "";
//        public static string AzureRegion = "";
//        public static string GoogleTranslateCredentials = "";
//        public static string ResolvedDatabasePath = "";

//        public static async Task<string> DatabasePath()
//        {
//            var locationFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("SpeechlyTouch", CreationCollisionOption.OpenIfExists);
//            await locationFolder.CreateFileAsync("Speechly.db", CreationCollisionOption.OpenIfExists);
//            var path = Path.Combine(locationFolder.Path, "Speechly.db");
//            ResolvedDatabasePath = path;
//            return path;
//        }

//        public static double ReEnterPasscodeTimeoutSeconds = 16;

//        public static string[] Scopes = { "openid", "profile" };
//        public static string Tenant = "speechlyauth.onmicrosoft.com";
//        public static string AB2CClientId = "";
//        public static string AB2CPolicySignUpSignIn = "B2C_1A_SIGNUP_SIGNIN";

//        private static readonly string BaseAuthority = "https://speechlyauth.b2clogin.com/tfp/{tenant}/{policy}/oauth2/v2.0/authorize";
//        public static string Authority = BaseAuthority.Replace("{tenant}", Tenant).Replace("{policy}", AB2CPolicySignUpSignIn);
//        public static string BackendAPiEndpoint = "https://speechly-api-staging.azurewebsites.net/";
//        public static string AzureADB2CUsersEndpoint = "api/v2/Users/GetByEmail";
//        public static string AzureADB2CUserDataEndpoint = "api/v2/Users/GetUserData";
//        public static string OrganizationBrandingEndpoint = "api/v2/organizationBranding";
//        public static string LicenseCheckValidityEndpoint = "api/v2/UserLicenses/CheckLicenseValidity";

//        public static string ConferenceAnnualLicenseSku = "";  //Speechly Conference (Annual) 
//        public static string ConferenceMonthlyLicenseSku = "";  //Speechly Conference (Monthly) 
//        public static string BusinessAnnualLicenseSku = "";  //Speechly Business (Annual) 
//        public static string BusinessMonthlyLicenseSku = "";  //Speechly Business (Monthly)

//        public static string GetApplicationFilesPath()
//        {
//            var path = Path.Combine(ApplicationData.Current.LocalFolder.Path + @"\SpeechlyTouch");
//            Directory.CreateDirectory(path);
//            return path;
//        }

//        public static string AzureStorageConnectionString;
//        public static string RecordingsContainer = "recordings";
//        public static string ConvertedRecordingsContainer = "recordings-converted";
//        public static string AzureStorageUrl = "https://speechlystoragestaging.blob.core.windows.net";
//        public static string RecordingsURL = $"{AzureStorageUrl}/{RecordingsContainer}/";

//        public static string ImmersiveReaderEndpoint = "api/v2/ImmersiveReader";

//        public static string GetSoftwareVersion()
//        {
//            Package package = Package.Current;
//            PackageId packageId = package.Id;
//            PackageVersion version = packageId.Version;

//            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
//        }

//        public static string GetRecordingsPath()
//        {
//            return GetApplicationFilesPath() + @"\Sessions";
//        }
//    }
//}

// RELEASE

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace SpeechlyTouch
{
    public static class Constants
    {
        public static string KeyVaultUri = "https://speechlyvaultproduction.vault.azure.net";
        public static string VaultAppId = "c2c3ac61-f1a6-463e-90a0-cb965d7dc1c1";
        public static string TenantId = "f9205e77-06c5-4ff9-a054-97bf1a3d3474";
        public static string KeygenClientSecret = "C5AIsEO7C~wB.M9__-5lrW6Rzn66Tw4Vc4";
        public static string AppCenterClientId = "8a3140f3-0f48-4c54-ab07-572bda09f478";
        public static string MailGunApiKey = "";
        public static string AzureKey = "";
        public static string AzureRegion = "";
        public static string GoogleTranslateCredentials = "";
        public static string ResolvedDatabasePath = "";

        public static async Task<string> DatabasePath()
        {
            var locationFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("SpeechlyTouch", CreationCollisionOption.OpenIfExists);
            await locationFolder.CreateFileAsync("Speechly.db", CreationCollisionOption.OpenIfExists);
            var path = Path.Combine(locationFolder.Path, "Speechly.db");
            ResolvedDatabasePath = path;
            return path;
        }

        public static double ReEnterPasscodeTimeoutSeconds = 16;

        public static string[] Scopes = { "openid", "profile" };
        public static string Tenant = "speechlyauth.onmicrosoft.com";
        public static string AB2CClientId = "";
        public static string AB2CPolicySignUpSignIn = "B2C_1A_SIGNUP_SIGNIN";

        private static readonly string BaseAuthority = "https://speechlyauth.b2clogin.com/tfp/{tenant}/{policy}/oauth2/v2.0/authorize";
        public static string Authority = BaseAuthority.Replace("{tenant}", Tenant).Replace("{policy}", AB2CPolicySignUpSignIn);
        public static string BackendAPiEndpoint = "https://speechly-api.azurewebsites.net/";
        public static string AzureADB2CUsersEndpoint = "api/v2/Users/GetByEmail";
        public static string AzureADB2CUserDataEndpoint = "api/v2/Users/GetUserData";
        public static string OrganizationBrandingEndpoint = "api/v2/organizationBranding";
        public static string LicenseCheckValidityEndpoint = "api/v2/UserLicenses/CheckLicenseValidity";

        public static string ConferenceAnnualLicenseSku = "";  //Speechly Conference (Annual) 
        public static string ConferenceMonthlyLicenseSku = "";  //Speechly Conference (Monthly) 
        public static string BusinessAnnualLicenseSku = "";  //Speechly Business (Annual) 
        public static string BusinessMonthlyLicenseSku = "";  //Speechly Business (Monthly)

        public static string GetApplicationFilesPath()
        {
            var path = Path.Combine(ApplicationData.Current.LocalFolder.Path + @"\SpeechlyTouch");
            Directory.CreateDirectory(path);
            return path;
        }

        public static string AzureStorageConnectionString;
        public static string RecordingsContainer = "recordings";
        public static string ConvertedRecordingsContainer = "recordings-converted";
        public static string AzureStorageUrl = "https://speechlyproduction.blob.core.windows.net";
        public static string RecordingsURL = $"{AzureStorageUrl}/{RecordingsContainer}/";

        public static string ImmersiveReaderEndpoint = "api/v2/ImmersiveReader";

        public static string GetSoftwareVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        public static string GetRecordingsPath()
        {
            return GetApplicationFilesPath() + @"\Sessions";
        }
    }
}

