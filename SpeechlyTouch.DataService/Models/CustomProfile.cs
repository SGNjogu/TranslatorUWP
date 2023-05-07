using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechlyTouch.DataService.Models
{
    public class CustomProfile : BaseModel
    {
        public string Name { get; set; }
        public bool IsSingleDevice { get; set; }
        public string PersonOneInputDevice { get; set; }
        public string PersonOneOutputDevice { get; set; }
        public string PersonTwoInputDevice { get; set; }
        public string PersonTwoOutputDevice { get; set; }
        public bool IsDefault { get; set; }
    }
}
