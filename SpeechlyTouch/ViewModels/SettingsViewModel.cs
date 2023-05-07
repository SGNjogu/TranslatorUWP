using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DTOs;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Styles;
using SpeechlyTouch.Views.Pages;
using SpeechlyTouch.Views.Popups;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace SpeechlyTouch.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private ObservableCollection<MenuItem> _menuItems;
        public ObservableCollection<MenuItem> MenuItems
        {
            get { return _menuItems; }
            set { SetProperty(ref _menuItems, value); }
        }

        private MenuItem _selectedMenuItem;
        public MenuItem SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set
            {
                SetProperty(ref _selectedMenuItem, value);
                Navgigate(SelectedMenuItem?.Name);
            }
        }

        private bool _navigationEnabled = true;
        public bool NavigationEnabled
        {
            get { return _navigationEnabled; }
            set { SetProperty(ref _navigationEnabled, value); }
        }

        private ResourceLoader _resourceLoader;
        public Frame ContentFrame;

        private ErrorDialog errorDialog;
        private readonly ICrashlytics _crashlytics;
        private readonly IDialogService _dialogService;

        public SettingsViewModel(ICrashlytics crashlytics, IDialogService dialogService)
        {
            _crashlytics = crashlytics;
            _dialogService = dialogService;
            _resourceLoader = ResourceLoader.GetForCurrentView();
            errorDialog = new ErrorDialog();
            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<InternationalizationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<ErrorMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            SelectedMenuItem = new MenuItem();
            LoadMenuItems();
            _dialogService = dialogService;
 
        }

        private void HandleMessage(ErrorMessage m)
        {
            if (m.EnableNavigation == false)
            {
                NavigationEnabled = false;
            }
            else
            {
                NavigationEnabled = true;
            }
            if (!m.DisplayDialog)
            {
                if(errorDialog != null)
                errorDialog.Hide();
            }

        }

        private async void HandleMessage(InternationalizationMessage message)
        {
            if (!string.IsNullOrEmpty(message.LanguageCode))
            {
                await Task.Delay(300);
                ContentFrame.CacheSize = 0;
                LoadMenuItems();
                NavigateToProfileView();
            }
        }

        void LoadMenuItems()
        {
            MenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem { Name = _resourceLoader.GetString("SettingsPage_Profile"), Glyph = "\uE77B", Foreground = ThemeHelper._dynamicColors.UnSelectedMenuItemColor, Background = ThemeHelper._dynamicColors.UnSelectedMenuItemBackgroundColor, ShadowColor = ThemeHelper._dynamicColors.UnSelectedMenuItemShadowColor },
                new MenuItem { Name = _resourceLoader.GetString("SettingsPage_Language"), Glyph = "\uE774", Foreground = ThemeHelper._dynamicColors.UnSelectedMenuItemColor, Background = ThemeHelper._dynamicColors.UnSelectedMenuItemBackgroundColor, ShadowColor = ThemeHelper._dynamicColors.UnSelectedMenuItemShadowColor },
                new MenuItem { Name = _resourceLoader.GetString("SettingsPage_Devices"), Glyph = "\uE772", Foreground = ThemeHelper._dynamicColors.UnSelectedMenuItemColor, Background = ThemeHelper._dynamicColors.UnSelectedMenuItemBackgroundColor, ShadowColor = ThemeHelper._dynamicColors.UnSelectedMenuItemShadowColor },
                new MenuItem { Name = _resourceLoader.GetString("SettingsPage_Questions"), Glyph = "\uF142", Foreground = ThemeHelper._dynamicColors.UnSelectedMenuItemColor, Background = ThemeHelper._dynamicColors.UnSelectedMenuItemBackgroundColor, ShadowColor = ThemeHelper._dynamicColors.UnSelectedMenuItemShadowColor }
            };
        }

        private void UpdateNavigatedItemUI(MenuItem menuItem)
        {
            if (MenuItems != null && MenuItems.Any())
            {
                foreach (var item in MenuItems)
                {
                    item.Foreground = ThemeHelper._dynamicColors.UnSelectedMenuItemColor;
                    item.Background = ThemeHelper._dynamicColors.UnSelectedMenuItemBackgroundColor;
                    item.ShadowColor = ThemeHelper._dynamicColors.UnSelectedMenuItemShadowColor;
                }
                menuItem.Foreground = ThemeHelper._dynamicColors.SelectedMenuItemColor;
                menuItem.Background = ThemeHelper._dynamicColors.SelectedMenuItemBackgroundColor;
                menuItem.ShadowColor = ThemeHelper._dynamicColors.SelectedMenuItemShadowColor;
            }
        }

        private void HandleMessage(NavigationMessage message)
        {
            LoadMenuItems();
            if (message.LoadProfileView)
                NavigateToProfileView();
        }

        async void Navgigate(string page)
        {
            if (page == _resourceLoader.GetString("SettingsPage_Profile"))
            {
                if (!NavigationEnabled)
                   await _dialogService.ShowDialog(errorDialog);
                NavigateToProfileView();
                return;
            }
            if (page == _resourceLoader.GetString("SettingsPage_Language"))
            {
                if (!NavigationEnabled)
                    await _dialogService.ShowDialog(errorDialog);
                StrongReferenceMessenger.Default.Send(new LanguageMessage { UpdateLanguages = true });
                NavigateToLanguagesView();
                return;
            }
            if (page == _resourceLoader.GetString("SettingsPage_Devices"))
            {
                if (!NavigationEnabled)
                    await _dialogService.ShowDialog(errorDialog);
                NavigateToDevicesView();
                return;
            }
            if (page == _resourceLoader.GetString("SettingsPage_Questions"))
            {
                if (!NavigationEnabled)

                    await _dialogService.ShowDialog(errorDialog);
                StrongReferenceMessenger.Default.Send(new OrgQuestionsMessage { ReloadQuestions = true });
                NavigateToQuestionsView();
                return;
            }
        }

        private async void NavigateToProfileView()
        {
            try
            {
                if (ContentFrame != null)
                {
                    ContentFrame.Navigate(typeof(ProfilePage));
                    UpdateNavigatedItemUI(MenuItems[0]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void NavigateToLanguagesView()
        {
            try
            {
                if (ContentFrame != null)
                {
                   
                    ContentFrame.Navigate(typeof(LanguagesPage));
                    
                    UpdateNavigatedItemUI(MenuItems[1]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void NavigateToDevicesView()
        {
            try
            {
                if (ContentFrame != null)
                {
                    ContentFrame.Navigate(typeof(DevicesPage));
                    UpdateNavigatedItemUI(MenuItems[2]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void NavigateToQuestionsView()
        {
            try
            {
                if (ContentFrame != null && NavigationEnabled == true)
                {
                    ContentFrame.Navigate(typeof(QuestionsPage));
                    UpdateNavigatedItemUI(MenuItems[3]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }
    }
}
