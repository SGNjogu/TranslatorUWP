using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.Languages;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class AddNewQuestionViewModel : ObservableObject
    {
        private string _newQuestion;
        public string NewQuestion
        {
            get { return _newQuestion; }
            set
            {
                SetProperty(ref _newQuestion, value);
            }
        }


        private ObservableCollection<Language> _defaultLanguages;
        public ObservableCollection<Language> DefaultLanguages
        {
            get { return _defaultLanguages; }
            set { SetProperty(ref _defaultLanguages, value); }
        }

        private Language _selectedTranslationDefaultLanguage;
        public Language SelectedTranslationDefaultLanguage
        {
            get { return _selectedTranslationDefaultLanguage; }
            set
            {
                SetProperty(ref _selectedTranslationDefaultLanguage, value);
            }
        }

        private readonly ILanguagesService _languagesService;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        public AddNewQuestionViewModel(ILanguagesService languagesService, IDialogService dialogService, ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _languagesService = languagesService;
            _dialogService = dialogService;

            _ = LoadLanguages();
        }

      

        public async Task LoadLanguages()
        {
            string defaultLanguageCode = _settingsService.DefaultTranslationLanguageCode;
            if (string.IsNullOrEmpty(defaultLanguageCode))
                defaultLanguageCode = "en-GB";
            var languages = await _languagesService.GetSupportedLanguagesAsync();

            DefaultLanguages = new ObservableCollection<Language>(languages.OrderBy(l => l.DisplayName));

            SelectedTranslationDefaultLanguage = DefaultLanguages.FirstOrDefault(s => s.Code == defaultLanguageCode);
        }

        private void AddNewQuestion()
        {
            if(!string.IsNullOrEmpty(NewQuestion) && !string.IsNullOrEmpty(SelectedTranslationDefaultLanguage.Code))
            {
                StrongReferenceMessenger.Default.Send(new NewQuestionMessage { Question = NewQuestion, LanguageCode = SelectedTranslationDefaultLanguage.Code });
            }
            else
            {
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = "Invalid input" });
            }
           
        }

        private void CloseDialog()
        {
            _dialogService.HideDialog();
        }

        private RelayCommand _addNewQuestionCommand = null;
        public RelayCommand AddNewQuestionCommand
        {
            get
            {
                return _addNewQuestionCommand ?? (_addNewQuestionCommand = new RelayCommand( () => {  AddNewQuestion(); }));
            }
        }

        private RelayCommand _closeDialogCommand = null;
        public RelayCommand CloseDialogCommand
        {
            get
            {
                return _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(() => { CloseDialog(); }));
            }
        }


    }
}
