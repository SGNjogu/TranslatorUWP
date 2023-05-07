using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SpeechlyTouch.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        private HistoryViewModel _dataContext;
        public HistoryPage()
        {
            this.InitializeComponent();
            _dataContext = DataContext as HistoryViewModel;
            _dataContext.ContentFrame = contentFrame;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { LoadSessionHistory = true, ResetSessionDetails = true });
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await _dataContext.LoadOrganizationSettings();
        }
    }
}
