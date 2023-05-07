using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.Enums;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Views.Popups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace SpeechlyTouch.ViewModels
{
    public class FeedbackStarRatingViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;
        private readonly IAppAnalytics _appAnalytics;
        private readonly IDialogService _dialogService;

        private FeedbackSubmittedDialog _feedbackSubmittedDialog;
        private FeedbackRatingDialog _feedbackRatingDialog;

        private string _sessionNumber;
        public string SessionNumber
        {
            get { return _sessionNumber; }
            set
            {
                SetProperty(ref _sessionNumber, value);
            }
        }

        private FeedbackType _feedbackType;
        public FeedbackType FeedbackType
        {
            get { return _feedbackType; }
            set
            {
                SetProperty(ref _feedbackType, value);
            }
        }


        private string _feedbackDescription = null;
        public string FeedbackDescription
        {
            get { return _feedbackDescription; }
            set
            {
                SetProperty(ref _feedbackDescription, value);
            }
        }

        private string _feedbackTitle = null;
        public string FeedbackTitle
        {
            get { return _feedbackTitle; }
            set
            {
                SetProperty(ref _feedbackTitle, value);
            }
        }
        public FeedbackStarRatingViewModel(IDataService dataService, ISettingsService settingsService, IAppAnalytics appAnalytics, IDialogService dialogService)
        {

            _dataService = dataService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _appAnalytics = appAnalytics;
          
            StrongReferenceMessenger.Default.Register<FeedbackDialogMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            LoadDialogs();
        }

        private async void LoadDialogs()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _feedbackSubmittedDialog = new FeedbackSubmittedDialog();
                _feedbackRatingDialog = new FeedbackRatingDialog();
            });
        }
        private async void HandleMessage(FeedbackDialogMessage message)
        {
          
            if (message.Feedbacktitle != null && message.Feedbackdescription != null)
            {
                FeedbackTitle = message.Feedbacktitle;
                FeedbackDescription = message.Feedbackdescription;
            }
            if (message.ShowFeedbackSubmitted == false)
            {
                _dialogService.HideDialog();
            }
            if (message.ShowFeedbackSubmitted == true)
            {
                _dialogService.HideDialog();

              await _dialogService.ShowDialog(_feedbackSubmittedDialog);
               
            }
            if (message.CloseRatingFeedback == true)
            {
                _dialogService.HideDialog();

            }
            if (message.ShowRatingFeedback == true)
            {
                _feedbackRatingDialog = new FeedbackRatingDialog();
              await _dialogService.ShowDialog(_feedbackRatingDialog);
            }
            if (message.SessionNumber != null)
            {
                SessionNumber = message.SessionNumber;
                FeedbackType = message.FeedbackType;
            }
        }

        public async Task InsertFiveRatingFeedback()
        {
            if (FeedbackType == FeedbackType.Translation && SessionNumber != null)
            {
            _feedbackSubmittedDialog = new FeedbackSubmittedDialog();

            DataService.Models.UserFeedback userFeedback = new DataService.Models.UserFeedback
            {
                SessionNumber = SessionNumber,
                    FeedbackType = (int)FeedbackType.Translation,
                Rating = 5
            };

            await _dataService.AddItemAsync<DataService.Models.UserFeedback>(userFeedback);

                SessionNumber = null;
            }
             

            var user = await _settingsService.GetUser();
            _appAnalytics.CaptureCustomEvent("Feedback Events",
              new Dictionary<string, string>
               {
                         {"User", user?.UserEmail},
                         {"Rating", "5" },
                         {"Action", " 5 star submitted" }
              });

            await _dialogService.ShowDialog(_feedbackSubmittedDialog);

        }


        public void ShowRatingFeedbackDialog()
        {
            StrongReferenceMessenger.Default.Send(new FeedbackDialogMessage { ShowRatingFeedback = true, Rating = FeedbackStarRatingDialog.Rating });

        }

        private RelayCommand _dismissFeedbackCommand = null;

        public RelayCommand DismissFeedbackCommand
        {
            get
            {
                return _dismissFeedbackCommand ?? (_dismissFeedbackCommand = new RelayCommand(() => { Dismiss(); }));
            }
        }

        private void Dismiss()
        {

            StrongReferenceMessenger.Default.Send(new FeedbackDialogMessage { CloseStarRatingFeedback = true });
        }

    }
}
