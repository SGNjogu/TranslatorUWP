using Microsoft.AppCenter.Crashes;
using Microsoft.Identity.Client;
using SpeechlyTouch.Helpers;
using SpeechlyTouch.Services;
using SpeechlyTouch.Styles;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace SpeechlyTouch
{
    public sealed partial class App : Application
    {
        // public static new App Current => (App)Application.Current;

        public static IPublicClientApplication PublicClientApp { get; set; }

        private Lazy<ActivationService> _activationService;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            InitializeComponent();

            EnteredBackground += App_EnteredBackground;
            Resuming += App_Resuming;

            UnhandledException += OnAppUnhandledException;

            // Deferred execution until used. Check https://docs.microsoft.com/dotnet/api/system.lazy-1 for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            ThemeHelper.ListenForRequestedTheme();

            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);

                // Disable TabNavigation
                // DisableTabNavigation();
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private void OnAppUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs ex)
        {
            // TODO WTS: Please log and handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/uwp/api/windows.ui.xaml.application.unhandledexception
            Debug.WriteLine($"Unhandled Exception: {ex.Message}");

            var environment = "production";

#if DEBUG
            environment = "development";
#elif STAGING
            environment = "staging";
#endif

            var attachments = ErrorAttachmentLog.AttachmentWithText(
                        "Unhandled Exception \n" +
                        $"AppVersion: {Constants.GetSoftwareVersion()} \n" +
                        $"Environment: {environment}", "Details.txt");

            Crashes.TrackError(ex.Exception, attachments: new ErrorAttachmentLog[] { attachments });
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(Views.Pages.StartupPage));
        }

        private async void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            await Singleton<SuspendAndResumeService>.Instance.SaveStateAsync();
            deferral.Complete();
        }

        private void App_Resuming(object sender, object e)
        {
            Singleton<SuspendAndResumeService>.Instance.ResumeApp();
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        //Disable TabNavigation
        //private void DisableTabNavigation()
        //{
        //    Window.Current.Content.PreviewKeyDown += Content_PreviewKeyDown;
        //}

        //private void Content_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        //{
        //    e.Handled = e.Key == VirtualKey.Tab ? true : false;
        //}
    }
}
