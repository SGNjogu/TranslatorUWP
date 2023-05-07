using SQLite;

namespace SpeechlyTouch.DataService.Models
{
    public class OutputDevice
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public string ContainerId { get; set; }
        public bool IsJabra { get; set; }
        public string Participant { get; set; }
    }
}
