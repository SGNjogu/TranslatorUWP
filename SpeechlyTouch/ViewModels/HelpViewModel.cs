using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DTOs;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Styles;
using SpeechlyTouch.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace SpeechlyTouch.ViewModels
{
    public class HelpViewModel : ObservableObject
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

        private User user = new User();

        private ResourceLoader _resourceLoader;
        public Frame ContentFrame;

        private readonly IAuthService _authService;
        private readonly ISettingsService _settingsService;
        private readonly IAppAnalytics _appAnalytics;

        public HelpViewModel(IAuthService authService, ISettingsService settingsService, IAppAnalytics appAnalytics)
        {
            _authService = authService;
            _settingsService = settingsService;
            _appAnalytics = appAnalytics;
            _resourceLoader = ResourceLoader.GetForCurrentView();
            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<InternationalizationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            SelectedMenuItem = new MenuItem();
            LoadMenuItems();
        }

        private async void HandleMessage(InternationalizationMessage message)
        {
            if (!string.IsNullOrEmpty(message.LanguageCode))
            {
                await Task.Delay(300);
                ContentFrame.CacheSize = 0;
                LoadMenuItems();
                UpdateNavigatedItemUI(MenuItems[0]);
                StrongReferenceMessenger.Default.Send(new NavigationMessage { LoadProfileView = true });
            }
        }

        private async void LoadMenuItems()
        {
            user = await _settingsService.GetUser();
            MenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem { Name = _resourceLoader.GetString("About"), Glyph = "\uE946", Foreground = ThemeHelper._dynamicColors.UnSelectedMenuItemColor, Background = ThemeHelper._dynamicColors.UnSelectedMenuItemBackgroundColor, ShadowColor = ThemeHelper._dynamicColors.UnSelectedMenuItemShadowColor },
                new MenuItem { Name = _resourceLoader.GetString("WhatsNew"), Glyph = "\uEA8F", Foreground = ThemeHelper._dynamicColors.UnSelectedMenuItemColor, Background = ThemeHelper._dynamicColors.UnSelectedMenuItemBackgroundColor, ShadowColor = ThemeHelper._dynamicColors.UnSelectedMenuItemShadowColor },
                new MenuItem { Name = _resourceLoader.GetString("Licence"), Glyph = "\uE8A1", Foreground = ThemeHelper._dynamicColors.UnSelectedMenuItemColor, Background = ThemeHelper._dynamicColors.UnSelectedMenuItemBackgroundColor, ShadowColor = ThemeHelper._dynamicColors.UnSelectedMenuItemShadowColor },
                new MenuItem { Name = _resourceLoader.GetString("Feedback"), Glyph = "\uED15", Foreground = ThemeHelper._dynamicColors.UnSelectedMenuItemColor, Background = ThemeHelper._dynamicColors.UnSelectedMenuItemBackgroundColor, ShadowColor = ThemeHelper._dynamicColors.UnSelectedMenuItemShadowColor }
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
            if (message.LoadAboutView)
                NavigateToAboutView();
        }

        private void Navgigate(string page)
        {
            if (string.IsNullOrEmpty(page))
                return;

            if (page == _resourceLoader.GetString("About"))
            {
                NavigateToAboutView();
                return;
            }
            if (page == _resourceLoader.GetString("WhatsNew"))
            {
                NavigateToWhatsNewView();
                return;
            }
            if (page == _resourceLoader.GetString("Licence"))
            {
                NavigateToLicenseView();
                return;
            }
            if (page == _resourceLoader.GetString("Feedback"))
            {
                NavigateToFeedbackView();
                return;
            }
        }

        private void NavigateToAboutView()
        {
            if (ContentFrame != null)
                ContentFrame.Navigate(typeof(AboutPage));
            UpdateNavigatedItemUI(MenuItems[0]);
            _appAnalytics.CaptureCustomEvent("Help Changes",
                    new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "About Navigation" }
                    });
        }

        private void NavigateToWhatsNewView()
        {
            if (ContentFrame != null)
                ContentFrame.Navigate(typeof(WhatsNewPage));
            UpdateNavigatedItemUI(MenuItems[1]);
            _appAnalytics.CaptureCustomEvent("Help Changes",
                    new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "What's New Navigation" }
                    });
        }

        private void NavigateToLicenseView()
        {
            if (ContentFrame != null)
                ContentFrame.Navigate(typeof(LicencePage));
            UpdateNavigatedItemUI(MenuItems[2]);
            _appAnalytics.CaptureCustomEvent("Help Changes",
                   new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Licences Navigation" }
                   });
        }

        private void NavigateToFeedbackView()
        {
            if (ContentFrame != null)
                ContentFrame.Navigate(typeof(FeedbackPage));
            UpdateNavigatedItemUI(MenuItems[3]);
            _appAnalytics.CaptureCustomEvent("Help Changes",
                   new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Feedback Navigation" }
                   });
        }

        private async Task Logout()
        {
            await _authService.Logout();
            NavigationService.GoBack();
            StrongReferenceMessenger.Default.Send(new NavigationMessage { ShowLoginDialogView = true });
        }

        private RelayCommand _logoutCommand = null;
        public RelayCommand LogoutCommand
        {
            get
            {
                return _logoutCommand ?? (_logoutCommand = new RelayCommand(async () => { await Logout(); }));
            }
        }
    }
}
