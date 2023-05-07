using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Enums;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.DataSync.Interfaces;
using SpeechlyTouch.Services.Languages;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Views.Popups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class OrgQuestionsViewModel : ObservableObject
    {
        private ObservableCollection<OrgQuestion> _questions;
        public ObservableCollection<OrgQuestion> Questions
        {
            get { return _questions; }
            set { SetProperty(ref _questions, value); }
        }

        private OrgQuestion _selectedQuestion;
        public OrgQuestion SelectedQuestion
        {
            get { return _selectedQuestion; }
            set
            {
                SetProperty(ref _selectedQuestion, value);
                HandleQuestionChange();
            }
        }

        private Visibility _DeleteButtonVisibility;
        public Visibility DeleteButtonVisibility
        {
            get { return _DeleteButtonVisibility; }
            set { SetProperty(ref _DeleteButtonVisibility, value); }
        }

        private Visibility _editButtonVisibility;
        public Visibility EditButtonVisibility
        {
            get { return _editButtonVisibility; }
            set { SetProperty(ref _editButtonVisibility, value); }
        }

        private ObservableCollection<OrgQuestion> _settingsQuestions;
        public ObservableCollection<OrgQuestion> SettingsQuestions
        {
            get { return _settingsQuestions; }
            set { SetProperty(ref _settingsQuestions, value); }
        }

        private Visibility _isVisibleValidationError;
        public Visibility IsVisibleValidationError
        {
            get { return _isVisibleValidationError; }
            set { SetProperty(ref _isVisibleValidationError, value); }
        }

        private readonly IDataService _dataService;
        private readonly IAppAnalytics _appAnalytics;
        private readonly ISettingsService _settingsService;
        private readonly ILanguagesService _languagesService;
        private readonly IPushDataService _pushDataService;
        private readonly IDialogService _dialogService;
        private ResourceLoader _resourceLoader;
        public  AddNewQuestionDialog addNewQuestionDialog;


        public OrgQuestionsViewModel(IDataService dataService, IAppAnalytics appAnalytics, ISettingsService settingsService, ILanguagesService languagesService, IPushDataService pushDataService,IDialogService dialogService)
        {
            _dataService = dataService;
            _appAnalytics = appAnalytics;
            _settingsService = settingsService;
            _languagesService = languagesService;
            _pushDataService = pushDataService;
            _dialogService = dialogService;
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
            StrongReferenceMessenger.Default.Register<OrgQuestionsMessage>(this, (r, message) => HandleMessage(message));
            StrongReferenceMessenger.Default.Register<ErrorMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<NewQuestionMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            Questions = new ObservableCollection<OrgQuestion>();
            IsVisibleValidationError = Visibility.Collapsed;
            SelectedQuestion = null;
            _ = Initialize();
        }

        private async Task HandleMessage(NewQuestionMessage m)
        {
            if (!string.IsNullOrEmpty(m.Question))
            {
              await  AddNewQuestion(m.Question,m.LanguageCode);
            }
        }
        private async void HandleMessage(ErrorMessage m)
        {
            if (m.SettingsType == Enums.SettingsType.Questions && m.EnableNavigation)
            {
                await SaveShortCuts();
            }
        }

        public async Task Initialize()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                addNewQuestionDialog = new AddNewQuestionDialog();
            });
            var questions = await _dataService.GetOrgQuestionsAsync();
            var availableQuestions = questions.Where(x => x.QuestionStatus == (int)UserQuestionStatus.Available).ToList();
            Questions = new ObservableCollection<OrgQuestion>();
            SettingsQuestions = new ObservableCollection<OrgQuestion>();
            foreach (var question in availableQuestions)
            {
                int result = 0;
                bool isInteger = Int32.TryParse(question.Shortcut, out result);

                Questions.Add(new OrgQuestion
                {
                    QuestionID = question.ID,
                    Question = question.Question,
                    LanguageCode = question.LanguageCode,
                    ShortCut = question.Shortcut,
                    IsVisibleShortCut = isInteger && result != 0 ? Visibility.Visible : Visibility.Collapsed
                });

                SettingsQuestions.Add(new OrgQuestion
                {
                    Question = question.Question,
                    LanguageCode = question.LanguageCode,
                    ShortCut = result == 0 && isInteger ? string.Empty : question.Shortcut,
                    QuestionID = question.ID,
                    Synced = question.SyncedToServer,
                    QuestionType = question.QuestionType
                });
            }
        }

        private async void HandleMessage(OrgQuestionsMessage message)
        {
            if (message != null && message.ReloadQuestions)
            {
                await Initialize();
            }
        }

        private void CloseDialog()
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseQuestionsDialog = true });
        }

        private void HandleQuestionChange()
        {
            if (SelectedQuestion != null)
            {
                var selectedQuestion = SelectedQuestion as OrgQuestion;
                SelectedQuestion = null;

                StrongReferenceMessenger.Default.Send(new OrgQuestionsMessage { TranslateQuestion = selectedQuestion.Question, LanguageCode = selectedQuestion.LanguageCode });

                CloseDialog();
            }
        }

        public async Task AddNewQuestion(string question, string code)
        {
            var newQuestion = new OrgQuestions
            {
                Question = question,
                LanguageCode = code,
                QuestionStatus = (int)UserQuestionStatus.Available,
                SyncedToServer = false,
                QuestionType = (int)QuestionType.UserQuestion
            };

            await _dataService.AddItemAsync<DataService.Models.OrgQuestions>(newQuestion);
            Questions.Add(new OrgQuestion
            {
                QuestionID = newQuestion.ID,
                Question = newQuestion.Question,
                LanguageCode = newQuestion.LanguageCode,
                QuestionStatus = (int)UserQuestionStatus.Available,
                Synced = false,
            });
            SettingsQuestions.Add(new OrgQuestion
            {
                QuestionID = newQuestion.ID,
                Question = newQuestion.Question,
                LanguageCode = newQuestion.LanguageCode,
                QuestionStatus = (int)UserQuestionStatus.Available,
                Synced = false,
            });
            StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("QuestionAdded") });
            _dialogService.HideDialog();
            Thread pushThread = new Thread(_pushDataService.BeginDataSync);
            pushThread.Start();
           

        }


        private async Task ShowAddQuestionDialog()
        {
            await _dialogService.ShowDialog(addNewQuestionDialog);
        }
        

        public async Task DeleteQuestion(object obj)
        {

            var question = (OrgQuestion)obj;
            var questions = await _dataService.GetOrgQuestionsAsync();
            var deletedQuestion = questions.Where(x => x.ID == question.QuestionID).FirstOrDefault();
            if (deletedQuestion != null)
            {
                if (deletedQuestion.SyncedToServer == true)
                {
                    var updateQuestion = new OrgQuestions()
                    {
                        ID = deletedQuestion.ID,
                        Question = deletedQuestion.Question,
                        LanguageCode = deletedQuestion.LanguageCode,
                        SyncedToServer = true,
                        QuestionStatus = (int)UserQuestionStatus.Deleted,
                        QuestionID = deletedQuestion.QuestionID,
                    };

                    await _dataService.UpdateItemAsync<DataService.Models.OrgQuestions>(updateQuestion);
                    SettingsQuestions.Remove(question);
                    var removedQuestion = Questions.Where(x => x.QuestionID == question.QuestionID).FirstOrDefault();
                    Questions.Remove(removedQuestion);
                    StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("QuestionDeleted") });
                }

                if (deletedQuestion.SyncedToServer == false)
                {
                    var existingQuestion = await _dataService.GetQuestionAsync(question.QuestionID);
                    if (existingQuestion != null)
                    {
                        await _dataService.DeleteItemAsync<OrgQuestions>(existingQuestion);
                        SettingsQuestions.Remove(question);
                        var removedQuestion = Questions.Where(x => x.QuestionID == question.QuestionID).FirstOrDefault();
                        Questions.Remove(removedQuestion);
                        StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("QuestionDeleted") });
                    }

                }
            }
            
            Thread pushThread = new Thread(_pushDataService.BeginDataSync);
            pushThread.Start();
        }


        public async Task SaveShortCuts()
        {
            bool invalidEntry = false;
            var user = await _settingsService.GetUser();
            //Validations
            IsVisibleValidationError = Visibility.Collapsed;
            List<bool> checkFalse = new List<bool>();

            //Loop through shortcuts in each question
            List<int> shortcuts = new List<int>();
            foreach (var question in SettingsQuestions)
            {
                int result;
                bool isValid = Int32.TryParse(question.ShortCut, out result);
                if(isValid && result > 0)
                {
                    shortcuts.Add(result);
                }

                Regex regex = new Regex(@"([1-9])");
                if (!regex.IsMatch(question.ShortCut) && !string.IsNullOrWhiteSpace(question.ShortCut))
                {
                    invalidEntry = true;
                }

                if (!isValid || result == 0)
                    question.ShortCut = string.Empty;

                //Check duplicates
                var items = shortcuts.Count(i => i == result && i != 0);
                if (items > 1)
                    checkFalse.Add(false);
            }

            //Check for numbers greater than 9
            var invalidKeys = shortcuts.Any(q => q > 9);

            var duplicatesFound = checkFalse.Any(i => i == false);

            if (!invalidKeys && !duplicatesFound && !invalidEntry)
            {
                var questions = await _dataService.GetOrgQuestionsAsync();
                foreach (var question in SettingsQuestions)
                {
                    var targetQuestion = questions.FirstOrDefault(q => q.Question == question.Question);
                    targetQuestion.Shortcut = question.ShortCut;
                    await _dataService.UpdateItemAsync<DataService.Models.OrgQuestions>(targetQuestion);
                }

                StrongReferenceMessenger.Default.Send(new OrgQuestionsMessage { ReloadQuestions = true });
                StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true });
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("ShortcutSuccess") });
                _appAnalytics.CaptureCustomEvent("Settings Changes",
                        new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "ShortCut Changes" }
                        });
            }
            else
            {
                IsVisibleValidationError = Visibility.Visible;
                StrongReferenceMessenger.Default.Send(new OrgQuestionsMessage { ReloadQuestions = true });
                StrongReferenceMessenger.Default.Send(new NotificationMessage { Visible = Visibility.Visible, DisplayMessage = _resourceLoader.GetString("ErrorMessage") });
            }
        }

        private RelayCommand _closeQuestionsCommand = null;
        public RelayCommand CloseQuestionsCommand
        {
            get
            {
                return _closeQuestionsCommand ?? (_closeQuestionsCommand = new RelayCommand(() => { CloseDialog(); }));
            }
        }

        private RelayCommand _saveShortCutsCommand = null;
        public RelayCommand SaveShortCutsCommand
        {
            get
            {
                return _saveShortCutsCommand ?? (_saveShortCutsCommand = new RelayCommand(async () => { await SaveShortCuts(); }));
            }
        }

        private RelayCommand<object> _deleteQuestionCommand = null;
        public RelayCommand<object> DeleteQuestionCommand
        {
            get
            {
                return _deleteQuestionCommand ?? (_deleteQuestionCommand = new RelayCommand<object>(async (object obj) => { await DeleteQuestion(obj); }));
            }
        }


        private RelayCommand _openQuestionDialogCommand = null;
        public RelayCommand OpenQuestionDialogCommand
        {
            get
            {
                return _openQuestionDialogCommand ?? (_openQuestionDialogCommand = new RelayCommand(async () => { await ShowAddQuestionDialog(); }));
            }
        }
    }

}
