using SpeechlyTouch.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SpeechlyTouch.Views.Popups
{
    public sealed partial class FeedbackStarRatingDialog : ContentDialog
    {
        public static int Rating;
        public static string CurrentSessionNumber;
        public FeedbackStarRatingDialog()
        {

            this.InitializeComponent();
        }

        private async void StarRating_ValueChanged(Microsoft.UI.Xaml.Controls.RatingControl sender, object args)
        {
            var feedbackSubmittedContext = DataContext as FeedbackStarRatingViewModel;

            Rating = (int)sender.Value;

            if (Rating == 0)
                return;

            if (Rating == 5)
            {
                StarRating.Value = -1;
                this.Hide();
                await feedbackSubmittedContext.InsertFiveRatingFeedback();
            }
            else
            {
                StarRating.Value = -1;
                this.Hide();
                feedbackSubmittedContext.ShowRatingFeedbackDialog();
            }

        }
    }
}
