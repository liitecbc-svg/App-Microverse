namespace Microverse.Services
{
    public readonly struct TranslationRequest
    {
        public readonly string Text;
        public readonly string SourceLanguage;
        public readonly string TargetLanguage;

        public TranslationRequest(string text, string sourceLanguage, string targetLanguage)
        {
            Text = text;
            SourceLanguage = sourceLanguage;
            TargetLanguage = targetLanguage;
        }
    }
}
