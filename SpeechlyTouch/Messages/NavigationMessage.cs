namespace SpeechlyTouch.Messages
{
    public class NavigationMessage
    {
        public bool LoadSettingsView { get; set; }
        public bool ReLoadSettingsView { get; set; }
        public bool CloseSettingsView { get; set; }
        public bool LoadProfileView { get; set; }
        public bool StopAppIdleTimerCountDown { get; set; }
        public bool StartAppIdleTimerCountDown { get; set; }
        public bool LoadQuickViewLanguages { get; set; }
        public bool CloseConsentDialog { get; set; }
        public bool AcceptConsent { get; set; }
        public bool CloseLoginView { get; set; }
        public bool CloseLoginFailedView { get; set; }
        public bool ShowLoginFailedView { get; set; }
        public bool ShowLoginDialogView { get; set; }
        public bool CloseInvalidLicenseView { get; set; }
        public bool ShowInvalidLicenseView { get; set; }
        public bool InitializeSession { get; set; }
        public bool CloseSessionDetails { get; set; }
        public bool ResetSessionDetails { get; set; }
        public bool LoadSessionHistory { get; set; }
        public bool ShowAudioNotDetectedDialog { get; set; }
        public bool StopTranslation { get; set; }
        public bool ContinueTranslation { get; set; }
        public bool CloseImmersiveReader { get; set; }
        public bool LoadAboutView { get; set; }
        public bool LoadNetworkInitialSetup { get; set; }
        public bool LoadDevicesInitialSetup { get; set; }
        public bool LoadLanguagesInitialSetup { get; set; }
        public bool LoadPasscodeInitialSetup { get; set; }
        public bool FinishAppSetup { get; set; }
        public bool HideShellTitleView { get; set; }
        public bool CheckSetupStatus { get; set; }
        public bool CloseConfirmAppCloseDialog { get; set; }
        public bool RegisterAppCloseEvent { get; set; }
        public bool UnRegisterAppCloseEvent { get; set; }
        public bool CloseQuestionsDialog { get; set; }
        public bool CloseSessionMetadataDialog { get; set; }
    }
}
