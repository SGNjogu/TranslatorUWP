using Microsoft.Toolkit.Mvvm.ComponentModel;
using SpeechlyTouch.DataService.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace SpeechlyTouch.ViewModels
{
    public class WhatsNewViewModel : ObservableObject
    {
        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion; }
            set { SetProperty(ref _appVersion, value); }
        }

        private string _releaseDate;
        public string ReleaseDate
        {
            get { return _releaseDate; }
            set
            {
                _releaseDate = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _releaseNotes;

        public ObservableCollection<string> ReleaseNotes
        {
            get { return _releaseNotes; }
            set { SetProperty(ref _releaseNotes, value); }
        }

        private readonly IDataService _dataservice;

        public WhatsNewViewModel(IDataService dataservice)
        {
            _dataservice = dataservice;
            GetAppInfo();
        }

        private async void GetAppInfo()
        {
            try
            {
                AppVersion = Constants.GetSoftwareVersion();

                var releaseNotes = await _dataservice.GetReleaseNotes();

                if (releaseNotes.Any())
                {
                    ReleaseNotes = new ObservableCollection<string>(releaseNotes.Select(s => s.Note));
                    ReleaseDate = releaseNotes.FirstOrDefault().DateReleased.ToString("MMM yyyy");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


    }
}
