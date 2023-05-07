using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.Events;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class SessionMetaDataViewModel : ObservableObject
    {
        private string _sessionName = string.Empty;
        public string SessionName
        {
            get { return _sessionName; }
            set
            {
                SetProperty(ref _sessionName, value);
                ValidateInputs();
            }
        }

        private ObservableCollection<string> _customTags;
        public ObservableCollection<string> CustomTags
        {
            get { return _customTags; }
            set
            {
                SetProperty(ref _customTags, value);
            }
        }

        private ObservableCollection<string> _organizationCustomTags;
        public ObservableCollection<string> OrganizationCustomTags
        {
            get { return _organizationCustomTags; }
            set
            {
                SetProperty(ref _organizationCustomTags, value);
            }
        }

        private ObservableCollection<SessionTag> _sessionTags;
        public ObservableCollection<SessionTag> SessionTags
        {
            get { return _sessionTags; }
            set
            {
                SetProperty(ref _sessionTags, value);
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                SetProperty(ref _isEnabled, value);
            }
        }

        private double _opacity;
        public double Opacity
        {
            get { return _opacity; }
            set
            {
                SetProperty(ref _opacity, value);
            }
        }

        private Visibility _organizationTagsVisibility;
        public Visibility OrganizationTagsVisibility
        {
            get { return _organizationTagsVisibility; }
            set
            {
                SetProperty(ref _organizationTagsVisibility, value);
            }
        }

        private Visibility _organizationCustomTagsVisibility;
        public Visibility OrganizationCustomTagsVisibility
        {
            get { return _organizationCustomTagsVisibility; }
            set
            {
                SetProperty(ref _organizationCustomTagsVisibility, value);
            }
        }

        private readonly IDataService _dataService;

        public SessionMetaDataViewModel(IDataService dataService)
        {
            _dataService = dataService;
            StrongReferenceMessenger.Default.Register<CustomTagMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            IsEnabled = false;
            Opacity = 0.5;
            SessionName = string.Empty;
            CustomTags = new ObservableCollection<string>();
            SessionTags = new ObservableCollection<SessionTag>();
            OrganizationCustomTags = new ObservableCollection<string>();
            OrganizationTagsVisibility = Visibility.Collapsed;
            OrganizationCustomTagsVisibility = Visibility.Collapsed;
        }

        public async Task Initialize()
        {
            IsEnabled = false;
            Opacity = 0.5;
            SessionName = string.Empty;
            CustomTags = new ObservableCollection<string>();
            SessionTags = new ObservableCollection<SessionTag>();
            OrganizationCustomTags = new ObservableCollection<string>();
            OrganizationTagsVisibility = Visibility.Collapsed;
            OrganizationCustomTagsVisibility = Visibility.Collapsed;

            var organizationTags = await _dataService.GetOrganizationTagsAsync();
            var organizationCustomTags = await _dataService.GetCustomTagsAsync();

            if (organizationTags != null && organizationTags.Any())
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var tag in organizationTags.Where(i => i.IsShownOnApp))
                    {
                        if (tag.IsShownOnApp)
                        {
                            var sessionTag = new SessionTag
                            {
                                IsMandatory = tag.IsMandatory,
                                ShowInApp = tag.IsShownOnApp,
                                OrganizationTagId = tag.OrganizationTagId,
                                TagName = tag.TagName
                            };
                            sessionTag.SessionTagChanged += OnTagValueChanged;

                            SessionTags.Add(sessionTag);
                        }
                    }

                    if (SessionTags.Any())
                    {
                        OrganizationTagsVisibility = Visibility.Visible;
                    }
                    else
                    {
                        OrganizationTagsVisibility = Visibility.Collapsed;
                    }
                });
            }
            else
            {
                OrganizationTagsVisibility = Visibility.Collapsed;
            }

            if (organizationCustomTags != null && organizationCustomTags.Any())
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var tag in organizationCustomTags)
                    {
                        OrganizationCustomTags.Add(tag.TagName);
                    }

                    if (organizationCustomTags.Any())
                        OrganizationCustomTagsVisibility = Visibility.Visible;
                });
            }
        }

        private void OnTagValueChanged(object sender, SessionTagChangedArgs args)
        {
            ValidateInputs();
        }

        private void HandleMessage(CustomTagMessage message)
        {
            if (message.Initialize)
            {
                Initialize();
                return;
            }

            if (message.AddTag && !string.IsNullOrEmpty(message.TagValue))
            {
                CustomTags.Add(message.TagValue);
            }
        }

        private void ValidateInputs()
        {
            if (SessionTags != null && SessionTags.Any())
            {
                bool mandatoryTagsMissing = false;
                var mandtoryTags = SessionTags.Where(s => s.IsMandatory == true && s.ShowInApp == true);

                if (mandtoryTags.Any())
                {
                    mandatoryTagsMissing = mandtoryTags.Any(s => string.IsNullOrEmpty(s.TagValue));
                }

                if (!mandatoryTagsMissing && !string.IsNullOrEmpty(SessionName))
                {
                    IsEnabled = true;
                    Opacity = 1;
                }
                else
                {
                    IsEnabled = false;
                    Opacity = 0.5;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(SessionName))
                {
                    IsEnabled = false;
                    Opacity = 0.5;
                }
                else
                {
                    IsEnabled = true;
                    Opacity = 1;
                }
            }
        }

        private void SendMetadata()
        {
            StrongReferenceMessenger.Default.Send(new SessionMetadataMessage { SessionName = SessionName, SessionTags = SessionTags, CustomTags = CustomTags });

            //Clear values
            SessionName = string.Empty;
            CustomTags = new ObservableCollection<string>();
            SessionTags = new ObservableCollection<SessionTag>();

            StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseSessionMetadataDialog = true });
        }

        private RelayCommand _addMetadataCommand = null;
        public RelayCommand AddMetadataCommand
        {
            get
            {
                return _addMetadataCommand ?? (_addMetadataCommand = new RelayCommand(() =>
                {
                    SendMetadata();
                }));
            }
        }

        private RelayCommand _closeDialogCommand = null;
        public RelayCommand CloseDialogCommand
        {
            get
            {
                return _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(() =>
                {
                    SessionName = string.Empty;
                    StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseSessionMetadataDialog = true });
                }));
            }
        }

        private void RemoveTag(object obj)
        {
            var str = obj as string;
            CustomTags.Remove(str);
        }

        private void AddTag(object obj)
        {
            var str = obj as string;
            CustomTags.Add(str);
        }

        private RelayCommand<object> _removeTagCommand = null;

        public RelayCommand<object> RemoveTagCommand
        {
            get
            {
                return _removeTagCommand ?? (_removeTagCommand = new RelayCommand<object>(RemoveTag));
            }
        }

        private RelayCommand<object> _addTagCommand = null;

        public RelayCommand<object> AddTagCommand
        {
            get
            {
                return _addTagCommand ?? (_addTagCommand = new RelayCommand<object>(AddTag));
            }
        }
    }
}
