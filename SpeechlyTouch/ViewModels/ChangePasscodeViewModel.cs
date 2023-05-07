using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class ChangePasscodeViewModel : ObservableObject
    {
        private string _currentPasscode;
        public string CurrentPasscode
        {
            get { return _currentPasscode; }
            set { SetProperty(ref _currentPasscode, value); }
        }

        private string _enteredPasscodeConfirmation;
        public string EnteredPasscodeConfirmation
        {
            get { return _enteredPasscodeConfirmation; }
            set { SetProperty(ref _enteredPasscodeConfirmation, value); }
        }

        private string _enteredPasscode;
        public string EnteredPasscode
        {
            get { return _enteredPasscode; }
            set { SetProperty(ref _enteredPasscode, value); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }

        private Visibility _errorMessageVisibility;
        public Visibility ErrorMessageVisibility
        {
            get { return _errorMessageVisibility; }
            set { SetProperty(ref _errorMessageVisibility, value); }
        }

        private string _infoMessage;
        public string InfoMessage
        {
            get { return _infoMessage; }
            set { SetProperty(ref _infoMessage, value); }
        }

        private Visibility _infoMessageVisibility;
        public Visibility InfoMessageVisibility
        {
            get { return _infoMessageVisibility; }
            set { SetProperty(ref _infoMessageVisibility, value); }
        }

        private readonly ISettingsService _settings;
        private readonly IEmailService _emailService;
        private readonly ICrashlytics _crashlytics;
        private readonly IAppAnalytics _appAnalytics;
        private ResourceLoader _resourceLoader;

        public ChangePasscodeViewModel(ISettingsService settings, IEmailService emailService, ICrashlytics crashlytics, IAppAnalytics appAnalytics)
        {
            StrongReferenceMessenger.Default.Register<PasscodeMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            _settings = settings;
            _emailService = emailService;
            _crashlytics = crashlytics;
            _appAnalytics = appAnalytics;
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();

            ErrorMessageVisibility = Visibility.Collapsed;
            ErrorMessage = "";
            InfoMessage = "";
            InfoMessageVisibility = Visibility.Collapsed;
        }

        public async void ValidatePasscode()
        {
            try
            {
                if (EnteredPasscode?.Length < 4 || EnteredPasscodeConfirmation?.Length < 4)
                {
                    ErrorMessageVisibility = Visibility.Visible;
                    ErrorMessage = "Minimum length of passcode should be 4.";
                }
                else if (CurrentPasscode != _settings.Passcode)
                {
                    ErrorMessageVisibility = Visibility.Visible;
                    ErrorMessage = "The current passcode entered is wrong. Kindly re-enter your current passcode then try again.";
                }
                else if (!EnteredPasscode.Equals(EnteredPasscodeConfirmation))
                {
                    ErrorMessageVisibility = Visibility.Visible;
                    ErrorMessage = "The passcodes entered do not match. Kindly re-enter the passcodes then try again.";
                }
                else if (EnteredPasscode.Equals(EnteredPasscodeConfirmation))
                {
                    if (EnteredPasscode?.Length < 4 || EnteredPasscodeConfirmation?.Length < 4)
                    {
                        ErrorMessageVisibility = Visibility.Visible;
                        ErrorMessage = "Minimum length of passcode should be 4.";
                    }
                    else if (CurrentPasscode != _settings.Passcode)
                    {
                        ErrorMessageVisibility = Visibility.Visible;
                        ErrorMessage = "The current passcode entered is wrong. Kindly re-enter your current passcode then try again.";
                    }
                    else
                    {
                        _settings.Passcode = EnteredPasscode;
                        StrongReferenceMessenger.Default.Send(new PasscodeMessage { ClosePasscodeDialogs = true });
                        CurrentPasscode = "";
                        EnteredPasscode = "";
                        EnteredPasscodeConfirmation = "";
                        InfoMessage = "";
                        InfoMessageVisibility = Visibility.Collapsed;
                        _settings.IsResetPasscodeEmailSent = false;
                    }

                    StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("PasswordUpdated") });
                    await Task.Delay(5000);
                    ErrorMessageVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private RelayCommand _closeDialogCommand = null;
        public RelayCommand CloseDialogCommand
        {
            get
            {
                return _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(() =>
                {
                    CurrentPasscode = "";
                    EnteredPasscode = "";
                    EnteredPasscodeConfirmation = "";
                    InfoMessage = "";
                    InfoMessageVisibility = Visibility.Collapsed;
                    StrongReferenceMessenger.Default.Send(new PasscodeMessage { ClosePasscodeDialogs = true });
                }));
            }
        }

        private async void HandleMessage(PasscodeMessage message)
        {
            if (message.ResetPasscode)
            {
                var user = await _settings.GetUser();
                await ResetPasscodeSetup();
                _appAnalytics.CaptureCustomEvent("App Password Changed",
                    new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization }
                    });
            }
        }

        bool resendEmail = false;
        private async Task ResendEmail()
        {
            try
            {
                var user = await _settings.GetUser();
                resendEmail = true;
                await ResetPasscodeSetup();
                InfoMessage = "Email Sent! Please check your inbox.";
                await Task.Delay(5000);
                InfoMessage = "Please check your email for instructions on how to reset your passcode.";

                _appAnalytics.CaptureCustomEvent("App Password Resend",
                    new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization }
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task ResetPasscodeSetup()
        {
            try
            {
                InfoMessage = "Please check your email for instructions on how to reset your passcode.";
                InfoMessageVisibility = Visibility.Visible;

                User user = await _settings.GetUser();

                if (user != null)
                {
                    string tempPasscode = "";

                    if (_settings.IsResetPasscodeEmailSent)
                    {
                        if (!resendEmail)
                            return;

                        resendEmail = false;
                        tempPasscode = _settings.Passcode;
                    }
                    else
                    {
                        Random random = new Random();
                        tempPasscode = random.Next(1000, 9999).ToString();
                        _settings.IsResetPasscodeEmailSent = true;
                    }

                    InfoMessage = "Sending Email...";
                    _settings.Passcode = tempPasscode.ToString();
                    string userName = user.UserName;
                    if (userName.Contains(" "))
                        userName = userName.Substring(0, userName.IndexOf(" "));
                    var email = $"Hi {userName},</p>Please enter <b>{tempPasscode}</b> as the <b>Current Passcode</b> to reset your passcode.</p></p>Kind Regards,</br>The Tala Team.";
                    //var emailBody = EmailHelper.CreateNoLinkEmailBody(user.UserName, email);
                    await _emailService.SendEmailAsync(user.UserEmail, "Tala Passcode Reset Instructions", email, Constants.MailGunApiKey);

                    InfoMessage = "Please check your email for instructions on how to reset your passcode.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private RelayCommand _validatePasscodeCommand = null;
        public RelayCommand ValidatePasscodeCommand
        {
            get
            {
                return _validatePasscodeCommand ?? (_validatePasscodeCommand = new RelayCommand(() => { ValidatePasscode(); }));
            }
        }

        private RelayCommand _resendEmailCommand = null;
        public RelayCommand ResendEmailCommand
        {
            get
            {
                return _resendEmailCommand ?? (_resendEmailCommand = new RelayCommand(async () => { await ResendEmail(); }));
            }
        }
    }
}
