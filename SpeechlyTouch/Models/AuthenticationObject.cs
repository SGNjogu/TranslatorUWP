using Microsoft.Identity.Client;

namespace SpeechlyTouch.Models
{
    public class AuthenticationObject
    {
        public AuthenticationResult AuthResult { get; set; }
        public SpeechlyUserData SpeechlyUserData { get; set; } = null;
    }
}
