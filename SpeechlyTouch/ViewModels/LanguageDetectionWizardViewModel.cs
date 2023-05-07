using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.FlagLanguage;
using SpeechlyTouch.Views.ContentControls.AutoDetection;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

namespace SpeechlyTouch.ViewModels
{
    public class LanguageDetectionWizardViewModel : ObservableObject
    {
        public Frame ContentFrame;

        private ObservableCollection<UserControl> _views;
        public ObservableCollection<UserControl> Views
        {
            get
            {
                return _views;
            }
            set
            {
                _views = value;
                SetProperty(ref _views, value);
            }
        }

        private UserControl _selectedView;
        public UserControl SelectedView
        {
            get { return _selectedView; }
            set { SetProperty(ref _selectedView, value); }
        }

        private int _selectedIndex = 0;
        private readonly IFlagLanguageService _flagLanguageService;

        public LanguageDetectionWizardViewModel(IFlagLanguageService flagLanguageService)
        {

            _flagLanguageService = flagLanguageService;
            StrongReferenceMessenger.Default.Register<AutoDetectMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            Initialize();
        }

        private void HandleMessage(AutoDetectMessage message)
        {
            if (message.GoToLanguages)
            {
                _selectedIndex = 1;
                SelectView();
            }

            if (message.GoToFlags)
            {
                _selectedIndex = 0;
                SelectView();
            }
        }

        private void Initialize()
        {
            Views = new ObservableCollection<UserControl>
            {
                new AutoDetectionFlags(),
                new AutoDetectionLanguages(),
            };

            SelectedView = Views[_selectedIndex];
        }

        private void NextView()
        {
            if (_selectedIndex != 1)
            {
                _selectedIndex++;
                SelectView();
            }
        }

        private void PreviousView()
        {
            if (_selectedIndex != 0)
            {
                _selectedIndex--;
                SelectView();
            }
        }

        private void SelectView()
        {

            SelectedView = Views[_selectedIndex];
        }
    }
}
