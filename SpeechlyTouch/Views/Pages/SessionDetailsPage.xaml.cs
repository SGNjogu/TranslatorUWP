using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Helpers;
using SpeechlyTouch.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SpeechlyTouch.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SessionDetailsPage : Page
    {
        private SessionDetailsViewModel _dataContext;

        public SessionDetailsPage()
        {
            this.InitializeComponent();
            _dataContext = DataContext as SessionDetailsViewModel;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            TitleBar.LoadTitleBar();

            var parameters = e.Parameter as Session;
            await _dataContext.GetSessionDetails(parameters);
        }
    }
}
