namespace SpeechlyTouch.Core.Domain
{
    public class LanguageFlag
    {
        /// <summary>
        /// ISO Code identifying a language
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Uri to flag image
        /// In WPF, it is a resource path
        /// It can be a URL too
        /// </summary>
        public string FlagUri { get; set; }
    }
}
