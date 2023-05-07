using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DTOs;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services;
using SpeechlyTouch.Views.ContentControls.InitialSetup;
using SpeechlyTouch.Views.Pages;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace SpeechlyTouch.ViewModels
{
    public class InitialSetupViewModel : ObservableObject
    {
        private ObservableCollection<InitialSetupMenutItem> _menuItems;
        public ObservableCollection<InitialSetupMenutItem> MenutItems
        {
            get { return _menuItems; }
            set { SetProperty(ref _menuItems, value); }
        }

        private InitialSetupMenutItem _selectedMenuItem;
        public InitialSetupMenutItem SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set
            {
                SetProperty(ref _selectedMenuItem, value);
                OnMenuItemSelectionChanged();
            }
        }

        public Frame ContentFrame;

        public InitialSetupViewModel()
        {
            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            Initialize();
        }

        private void HandleMessage(NavigationMessage message)
        {
            //if (message.LoadNetworkInitialSetup)
            //{
            //    SelectedMenuItem = MenutItems.First(s => s.Title == "Network");
            //    return;
            //}

            if (message.LoadDevicesInitialSetup)
            {
                SelectedMenuItem = MenutItems.First(s => s.Title == "Audio Device");
                return;
            }

            if (message.LoadLanguagesInitialSetup)
            {
                SelectedMenuItem = MenutItems.First(s => s.Title == "Language");
                return;
            }

            if (message.LoadPasscodeInitialSetup)
            {
                SelectedMenuItem = MenutItems.First(s => s.Title == "Passcode");
                return;
            }

            if (message.FinishAppSetup)
                GoToDashBoard();
        }

        private void Initialize()
        {
            ContentFrame = new Frame();

            MenutItems = new ObservableCollection<InitialSetupMenutItem>
            {
                // new InitialSetupMenutItem { Number = 1, Opacity = 1, Title = "Network", Ellipsis = ".................................." },
                new InitialSetupMenutItem { Number = 1, Opacity = 0.7, Title = "Audio Device", Ellipsis = "........................." },
                new InitialSetupMenutItem { Number = 2, Opacity = 0.7, Title = "Language", Ellipsis = "........................." },
                new InitialSetupMenutItem { Number = 3, Opacity = 0.7, Title = "Passcode" },
            };

            SelectedMenuItem = MenutItems.First();
        }

        private void OnMenuItemSelectionChanged()
        {
            foreach(var item in MenutItems)
            {
                item.Opacity = 0.5;
            }

            SelectedMenuItem.Opacity = 1;
            Navigate();
        }

        private void Navigate()
        {
            if (ContentFrame != null)
                switch (SelectedMenuItem.Title)
                {
                    //case "Network":
                    //    ContentFrame.Navigate(typeof(Network));
                    //    break;
                    case "Audio Device":
                        ContentFrame.Navigate(typeof(AudioDevice));
                        break;
                    case "Language":
                        ContentFrame.Navigate(typeof(Languages));
                        break;
                    case "Passcode":
                        ContentFrame.Navigate(typeof(Passcode));
                        break;
                    default:
                        break;
                }
        }

        private void GoToDashBoard()
        {
            NavigationService.Navigate(typeof(DashboardPage));
        }

        private RelayCommand _skipSetupCommand = null;
        public RelayCommand SkipSetupCommand
        {
            get
            {
                return _skipSetupCommand ?? (_skipSetupCommand = new RelayCommand(() => { GoToDashBoard(); }));
            }
        }
    }
}
