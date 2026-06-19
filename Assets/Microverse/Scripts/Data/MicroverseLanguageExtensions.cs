namespace Microverse.Data
{
    public static class MicroverseLanguageExtensions
    {
        public static string ToLanguageCode(this MicroverseLanguage language)
        {
            switch (language)
            {
                case MicroverseLanguage.English:
                    return "en";
                case MicroverseLanguage.Portuguese:
                    return "pt";
                default:
                    return "es";
            }
        }
    }
}
