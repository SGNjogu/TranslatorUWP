using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using AudioDetectTimer = System.Timers.Timer;

namespace SpeechlyTouch.ViewModels
{
    public class AudioNotDetectedViewModel : ObservableObject
    {
        private string _countDownDisplay;
        public string CountDownDisplay
        {
            get { return _countDownDisplay; }
            set { SetProperty(ref _countDownDisplay, value); }
        }

        private AudioDetectTimer _audioNotDetectedTimer;
        private int _timeCounter;

        private readonly ICrashlytics _crashlytics;

        public AudioNotDetectedViewModel(ICrashlytics crashlytics)
        {
            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            _crashlytics = crashlytics;
        }

        private async void HandleMessage(NavigationMessage message)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (message.ShowAudioNotDetectedDialog)
                    {
                        _audioNotDetectedTimer = new AudioDetectTimer();
                        _audioNotDetectedTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
                        _audioNotDetectedTimer.Elapsed += TimeElapsed;
                        _timeCounter = 90;
                        _audioNotDetectedTimer.Start();
                        CountDownDisplay = "0:00";
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void TimeElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _timeCounter--;
                    CountDownDisplay = _timeCounter / 60 + ":" + ((_timeCounter % 60) >= 10 ? (_timeCounter % 60).ToString() : "0" + _timeCounter % 60);

                    if (_timeCounter == 0)
                        StrongReferenceMessenger.Default.Send(new NavigationMessage { StopTranslation = true });
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void ContinueTranslation()
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    StrongReferenceMessenger.Default.Send(new NavigationMessage { ContinueTranslation = true });
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private RelayCommand _continueTranslationCommand = null;

        public RelayCommand ContinueTranslationCommand
        {
            get
            {
                return _continueTranslationCommand ?? (_continueTranslationCommand = new RelayCommand(() => { ContinueTranslation(); }));
            }
        }
    }
}
