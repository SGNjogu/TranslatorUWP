using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.System;
using Windows.UI.Popups;

namespace SpeechlyTouch.Helpers
{
    public static class PermissionsHelper
    {
        public static async Task<bool> MicrophoneAccess()
        {
            try
            {
                MediaCapture mediaCapture = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings();
                settings.StreamingCaptureMode = StreamingCaptureMode.Audio;
                await mediaCapture.InitializeAsync(settings);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageDialog requestPermissionDialog =
                    new MessageDialog($"The app needs to access your Microphone. " +
                                      "If the app closes, reopen it afterwards. " +
                                      "If you Cancel, the app will have limited functionality.");

                var okCommand = new UICommand("Give Access");
                requestPermissionDialog.Commands.Add(okCommand);
                var cancelCommand = new UICommand("Cancel");
                requestPermissionDialog.Commands.Add(cancelCommand);
                requestPermissionDialog.DefaultCommandIndex = 0;
                requestPermissionDialog.CancelCommandIndex = 1;

                var requestPermissionResult = await requestPermissionDialog.ShowAsync();
                if (requestPermissionResult == cancelCommand)
                {
                    //user chose to Cancel, app will not have permission
                    StrongReferenceMessenger.Default.Send(new PermissionsMessage { MicPermissionDenied = true });
                }
                else
                {
                    //open app settings to allow users to give us permission
                    await Launcher.LaunchUriAsync(new Uri("ms-settings:appsfeatures-app"));

                    //confirmation dialog to retry
                    var confirmationDialog = new MessageDialog($"Please give this app Microphone permission " +
                                                               "in the Settings app which has now opened.");

                    confirmationDialog.Commands.Add(okCommand);
                    await confirmationDialog.ShowAsync();

                    StrongReferenceMessenger.Default.Send(new PermissionsMessage { MicPermissionDenied = true });
                }
            }

            return false;
        }
    }
}
