namespace Microverse.Data
{
    [System.Serializable]
    public class LocalizedText
    {
        public string Spanish;
        public string English;
        public string Portuguese;

        public LocalizedText(string spanish, string english, string portuguese)
        {
            Spanish = spanish;
            English = english;
            Portuguese = portuguese;
        }

        public static LocalizedText FromEnglish(string english)
        {
            return new LocalizedText(string.Empty, english, string.Empty);
        }

        public string Get(MicroverseLanguage language)
        {
            string preferred;
            switch (language)
            {
                case MicroverseLanguage.English:
                    preferred = English;
                    break;
                case MicroverseLanguage.Portuguese:
                    preferred = Portuguese;
                    break;
                default:
                    preferred = Spanish;
                    break;
            }

            return FirstAvailable(preferred, English, Spanish, Portuguese);
        }

        public string GetSource(MicroverseLanguage sourceLanguage)
        {
            return Get(sourceLanguage);
        }

        public void Set(MicroverseLanguage language, string value)
        {
            switch (language)
            {
                case MicroverseLanguage.English:
                    English = value;
                    break;
                case MicroverseLanguage.Portuguese:
                    Portuguese = value;
                    break;
                default:
                    Spanish = value;
                    break;
            }
        }

        private static string FirstAvailable(params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                {
                    return values[i];
                }
            }

            return string.Empty;
        }
    }
}
