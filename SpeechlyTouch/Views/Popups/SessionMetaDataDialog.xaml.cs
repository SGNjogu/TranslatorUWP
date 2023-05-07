using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SpeechlyTouch.Views.Popups
{
    public sealed partial class SessionMetaDataDialog : ContentDialog
    {
        private SessionMetaDataViewModel _dataContext;
        public SessionMetaDataDialog()
        {
            this.InitializeComponent();
            _dataContext = DataContext as SessionMetaDataViewModel;
        }

        private void Button_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void Button_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        private void customTag_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var height = CustomTagsScrollViewer.ActualHeight;

            CustomTagsScrollViewer.ChangeView(null, height, null);

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!string.IsNullOrEmpty(customTag.Text))
                {
                    StrongReferenceMessenger.Default.Send(new CustomTagMessage { AddTag = true, TagValue = customTag.Text });
                }

                customTag.Text = string.Empty;
            }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            StrongReferenceMessenger.Default.Send(new CustomTagMessage { Initialize = true });
        }

        private void sessionName_Loaded(object sender, RoutedEventArgs e)
        {
            sessionName.Focus(FocusState.Programmatic);
        }

        private void customTag_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if(args.ChosenSuggestion != null)
            {
                customTag.Text = args.ChosenSuggestion.ToString();
                StrongReferenceMessenger.Default.Send(new CustomTagMessage { AddTag = true, TagValue = customTag.Text });
                customTag.Text = string.Empty;
                customTag.ItemsSource = GetSuggestions(string.Empty);
            }
            else
            {
                StrongReferenceMessenger.Default.Send(new CustomTagMessage { AddTag = true, TagValue = customTag.Text });
                customTag.Text = string.Empty;
                customTag.ItemsSource = GetSuggestions(string.Empty);
            }
        }

        private ObservableCollection<string> GetSuggestions(string text)
        {
            ObservableCollection<string> suggestions = new ObservableCollection<string>();

            var organizationCustomTagsList = _dataContext.OrganizationCustomTags.ToList();
            if (organizationCustomTagsList.Count() > 0 && !string.IsNullOrEmpty(text))
            {
                var filteredList = organizationCustomTagsList.FindAll(t => t.ToLower().Contains(text.ToLower()));
                if (filteredList.Count() > 0)
                    suggestions = new ObservableCollection<string>(filteredList);
            }

            return suggestions;
        }

        private void customTag_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            string suggestion = sender.Text;
            customTag.ItemsSource = GetSuggestions(suggestion);

            if (customTag.Text.Length >= 40)
            {
                customTag.Text = customTag.Text.Substring(0, 40);
            }
        }
    }
}
