using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Services.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using AppVersion = SpeechlyTouch.Models.AppVersion;

namespace SpeechlyTouch.Services.Versioning
{
    public class AppVersionService : IAppVersionService
    {
        // AppTypes as is on the Backend
        //public enum AppType
        //{​
        //    Android,
        //    IOS,
        //    UWP,
        //    WPF
        //}​
        public AppVersion CurrentVersion { get; private set; }

        private readonly Infrastructure.Services.Interfaces.IAppVersionService _backendAppVersionService;
        private readonly IAuthService _authService;
        private readonly IDataService _dataService;

        public AppVersionService(Infrastructure.Services.Interfaces.IAppVersionService backendAppVersionService, IAuthService authService, IDataService dataService)
        {
            _backendAppVersionService = backendAppVersionService;
            _authService = authService;
            _dataService = dataService;
        }

        public async Task<AppVersion> FetchAppVersion()
        {
            try
            {
                AppVersion newVersion = new AppVersion();
                AppVersion currentVersion = new AppVersion();
                CurrentVersion = new AppVersion();

                // Get versions from backend
                var currentVersionsEnumerable = await _backendAppVersionService.GetAppVersions(2, _authService.IdToken);
                if (currentVersionsEnumerable.Count() != 0)
                {
                    currentVersion = await GetCurrentAppVersion(currentVersionsEnumerable.ToList());
                    var fetchedNewVersion = currentVersionsEnumerable.FirstOrDefault();
                    newVersion.Version = fetchedNewVersion.Version;
                    newVersion.IsForcedUpdate = fetchedNewVersion.IsForcedUpdate;
                    newVersion.ReleaseNotesList = fetchedNewVersion.ReleaseNotesList;
                    newVersion.ReleaseDate = fetchedNewVersion.ReleaseDate;
                }

                // compare versions
                var latestVersion = IsLatestAppVersion(currentVersion, newVersion);
                latestVersion.IsForcedUpdate = newVersion.IsForcedUpdate;
                latestVersion.ReleaseNotesList = newVersion.ReleaseNotesList;
                latestVersion.ReleaseDate = newVersion.ReleaseDate;
                CurrentVersion = latestVersion;
                return latestVersion;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<AppVersion> GetCurrentAppVersion(List<Core.Domain.AppVersion> appVersions)
        {
            //Get current version
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            string current = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

            // check if on the latest version
            AppVersion currentVersion = new AppVersion { Version = current };

            var releaseNotes = await _dataService.GetReleaseNotes();

            var releaseNotesAvailable = releaseNotes != null && releaseNotes.Any();
            if (!releaseNotesAvailable)
            {
                var currentReleaseNotes = appVersions.FirstOrDefault(s => s.Version == currentVersion.Version);

                if (currentReleaseNotes != null && currentReleaseNotes.ReleaseNotesList.Any())
                {
                    List<ReleaseNote> notes = new List<ReleaseNote>();
                    foreach (var item in currentReleaseNotes.ReleaseNotesList)
                    {
                        notes.Add(new ReleaseNote { DateReleased = currentReleaseNotes.ReleaseDate, Note = item });
                    }
                    await _dataService.CreateReleaseNotes(notes);
                }
            }
            else
            {
                currentVersion.ReleaseNotesList = releaseNotes.Select(s => s.Note).ToList();
            }
            return currentVersion;
        }

        private AppVersion IsLatestAppVersion(AppVersion currentVersion, AppVersion newVersion)
        {
            Version currentVersionNumber;
            Version newVersionNumber;
            Version.TryParse(currentVersion.Version, out currentVersionNumber);
            Version.TryParse(newVersion.Version, out newVersionNumber);

            if (currentVersionNumber != null && newVersionNumber != null)
            {
                var result = VersionComparer(newVersionNumber, currentVersionNumber);
                if (result >= 3) // newVersion is greater
                    currentVersion.IsUnsupportedVersion = true;
                else if (result > 0) // newVersion is greater
                    currentVersion.IsLatestVersion = false;
                else if (result < 0) // currentVersionNumber is greater
                    currentVersion.IsLatestVersion = true;
                else // currentVersionNumber newVersion are equal
                    currentVersion.IsLatestVersion = true;
            }
            return currentVersion;
        }

        private int VersionComparer(Version newVersion, Version currentVersion)
        {
            if (newVersion.Major != currentVersion.Major)
                return newVersion.Major - currentVersion.Major;
            if (newVersion.Minor != currentVersion.Minor)
                return newVersion.Minor - currentVersion.Minor;
            if (newVersion.Build != currentVersion.Build)
                return newVersion.Build - currentVersion.Build;
            if (newVersion.Revision != currentVersion.Revision)
                return newVersion.Revision - currentVersion.Revision;
            return 0; // equal
        }
    }
}
