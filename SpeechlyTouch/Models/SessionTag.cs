using Microsoft.Toolkit.Mvvm.ComponentModel;
using SpeechlyTouch.Events;
using Windows.UI.Xaml;

namespace SpeechlyTouch.Models
{
    public class SessionTag : ObservableObject
    {
        public event SessionTagChangedEvent SessionTagChanged;
        public int OrganizationTagId { get; set; }
        public int SessionId { get; set; }
        public int OrganizationId { get; set; }

        private string _tagValue;
        public string TagValue
        {
            get { return _tagValue; }
            set
            {
                SetProperty(ref _tagValue, value);
                var args = new SessionTagChangedArgs { TagValue = TagValue };
                SessionTagChanged?.Invoke(this, args);
            }
        }

        private string _tagName;
        public string TagName
        {
            get { return _tagName; }
            set { SetProperty(ref _tagName, value); }
        }

        public bool ShowInApp { get; set; }

        private Visibility _mandatoryVisibility;

        public Visibility MandatoryVisibility
        {
            get { return _mandatoryVisibility; }
            set { SetProperty(ref _mandatoryVisibility, value); }
        }

        private bool _isMandatory;
        public bool IsMandatory
        {
            get { return _isMandatory; }
            set
            {
                _isMandatory = value;
                if (IsMandatory)
                {
                    MandatoryVisibility = Visibility.Visible;
                }
                else
                {
                    MandatoryVisibility = Visibility.Collapsed;
                }
            }
        }
    }
}
