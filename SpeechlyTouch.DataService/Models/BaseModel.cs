using GalaSoft.MvvmLight;
using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpeechlyTouch.DataService.Models
{
    public abstract class BaseModel : ObservableObject
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
