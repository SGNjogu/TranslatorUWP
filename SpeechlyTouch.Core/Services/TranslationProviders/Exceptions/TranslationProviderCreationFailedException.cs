using System;

namespace SpeechlyTouch.Core.Services.TranslationProviders.Exceptions
{
    public class TranslationProviderCreationFailedException : Exception
    {
        public TranslationProviderCreationFailedException()
        {
        }

        public TranslationProviderCreationFailedException(string message)
            : base(message)
        {
        }
    }
}
