using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.Enums;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Views.Popups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class FeedbackRatingViewModel : ObservableObject
    {
        private List<string> ReasonsForRating = new List<string>();

        private string otherComment;
        public string OtherComment
        {
            get { return otherComment; }
            set
            {
                SetProperty(ref otherComment, value);
            }
        }

        private int _rating;
        public int Rating
        {
            get { return _rating; }
            set
            {
                SetProperty(ref _rating, value);
            }
        }

        private string _sessionNumber = null;
        public string SessionNumber
        {
            get { return _sessionNumber; }
            set
            {
                SetProperty(ref _sessionNumber, value);
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

        private FeedbackType _feedbackType;
        public FeedbackType FeedbackType
        {
            get { return _feedbackType; }
            set
            {
                SetProperty(ref _feedbackType, value);
            }
        }

        private Visibility _sessionFeedbackVisibility = Visibility.Collapsed;
        public Visibility SessionFeedbackVisibility
        {
            get { return _sessionFeedbackVisibility; }
            set
            {
                SetProperty(ref _sessionFeedbackVisibility, value);
            }
        }

        private readonly IDataService _dataService;
        private readonly IAppAnalytics _appAnalytics;
        private readonly ISettingsService _settingsService;
        public FeedbackRatingViewModel(IDataService dataService, IAppAnalytics appAnalytics, ISettingsService settingsService)
        {
            _dataService = dataService;
            _appAnalytics = appAnalytics;
            _settingsService = settingsService;
            StrongReferenceMessenger.Default.Register<FeedbackDialogMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
        }

        private void HandleMessage(FeedbackDialogMessage message)
        {
            if (message.Rating > 0)
            {
                Rating = message.Rating;
            }
            if (message.SessionNumber != null)
            {
                SessionNumber = message.SessionNumber;
            }
            if (message.Feedbacktitle != null)
            {
                FeedbackTitle = message.Feedbacktitle;
            }
            if (message.FeedbackType == FeedbackType.Translation)
            {
                FeedbackType = message.FeedbackType;
                SessionFeedbackVisibility = Visibility.Visible;
        }
        }

        public async Task SendFeedback()
        {

            await InsertFeedback();
            ShowFeedbackSubmittedDialog();
            OtherComment = "";
            Rating = 0;
            ReasonsForRating.Clear();

        }

        private async Task InsertFeedback()
        {
            if (FeedbackType == FeedbackType.Translation && SessionNumber != null)
            {
            ReasonsForRating = FeedbackRatingDialog.CommentsList;
            if (ReasonsForRating.Count > 0)
            {
                foreach (string reason in ReasonsForRating)
                {
                    DataService.Models.UserFeedback userFeedback = new DataService.Models.UserFeedback
                    {
                        SessionNumber = SessionNumber,
                        ReasonForRating = reason,
                        Comment = OtherComment,
                        Rating = Rating,
                        FeedbackType = (int)FeedbackType.Translation
                    };

                    await _dataService.AddItemAsync<DataService.Models.UserFeedback>(userFeedback);
                }
            }
            else
            {
                DataService.Models.UserFeedback userFeedback = new DataService.Models.UserFeedback
                {
                    SessionNumber = SessionNumber,
                    Comment = OtherComment,
                    Rating = Rating,
                    FeedbackType = (int)FeedbackType.Translation
                };
                await _dataService.AddItemAsync<DataService.Models.UserFeedback>(userFeedback);
            }

            }

            var user = await _settingsService.GetUser();
            _appAnalytics.CaptureCustomEvent("Feedback Events",
              new Dictionary<string, string>
               {
                         {"User", user?.UserEmail},
                         {"Rating", Rating.ToString() },
                         {"Action", "Less than 5 star submitted" }
              });
        }

        private void ShowFeedbackSubmittedDialog()
        {
            StrongReferenceMessenger.Default.Send(new FeedbackDialogMessage { ShowFeedbackSubmitted = true }); ;
        }

        private void DismissDialog()
        {
            StrongReferenceMessenger.Default.Send(new FeedbackDialogMessage { CloseRatingFeedback = true }); ;
        }


        private RelayCommand _closeFeedbackCommand = null;

        public RelayCommand CloseFeedbackCommand
        {
            get
            {
                return _closeFeedbackCommand ?? (_closeFeedbackCommand = new RelayCommand(() => { DismissDialog(); }));
            }
        }

        private RelayCommand _sendFeedbackCommand = null;

        public RelayCommand SendFeedbackCommand
        {
            get
            {
                return _sendFeedbackCommand ?? (_sendFeedbackCommand = new RelayCommand(async () => { await SendFeedback(); }));
            }
        }
    }
}
