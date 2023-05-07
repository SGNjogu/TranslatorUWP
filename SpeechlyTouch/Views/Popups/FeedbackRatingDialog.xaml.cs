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
    public sealed partial class FeedbackRatingDialog : ContentDialog
    {
        public static List<string> CommentsList = new List<string>();

        private static string FirstReason = "It didn't pick up my voice correctly";
        private static string SecondReason = "It mistranslated my sentences";
        private static string ThirdReason = "The translation time was too slow";
        private static string FourthReason = "It picked up background voices";
        private static string FifthReason = "I heard echo in the call";
        private static string SixthReason = "The volume was too low";
        private static string SeventhReason = "The session ended unexpectedly";
        private static string EighthReason = "We kept interrupting each other";
        public FeedbackRatingDialog()
        {
            this.InitializeComponent();
            CommentsList.Clear();
        }
        private void FirstCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CommentsList.Add(FirstReason);
        }

        private void FirstCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CommentsList.Contains(FirstReason))
                CommentsList.Remove(FirstReason);
        }

        private void SecondCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CommentsList.Add(SecondReason);
        }

        private void SecondCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CommentsList.Contains(SecondReason))
                CommentsList.Remove(SecondReason);
        }

        private void ThirdCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CommentsList.Add(ThirdReason);
        }

        private void ThirdCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CommentsList.Contains(ThirdReason))
                CommentsList.Remove(ThirdReason);
        }

        private void FourthCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CommentsList.Add(FourthReason);
        }

        private void FourthCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CommentsList.Contains(FourthReason))
                CommentsList.Remove(FourthReason);
        }

        private void FifthCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CommentsList.Add(FifthReason);
        }

        private void FifthCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CommentsList.Contains(FifthReason))
                CommentsList.Remove(FifthReason);
        }

        private void SixthCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CommentsList.Add(SixthReason);
        }

        private void SixthCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CommentsList.Contains(SixthReason))
                CommentsList.Remove(SixthReason);
        }

        private void SeventhCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CommentsList.Add(SeventhReason);
        }

        private void SeventhCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CommentsList.Contains(SeventhReason))
                CommentsList.Remove(SeventhReason);
        }

        private void EighthCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CommentsList.Add(EighthReason);
        }

        private void EighthCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CommentsList.Contains(EighthReason))
                CommentsList.Remove(EighthReason);
        }
    }
}
