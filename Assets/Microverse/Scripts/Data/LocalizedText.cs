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

        public string Get(MicroverseLanguage language)
        {
            switch (language)
            {
                case MicroverseLanguage.English:
                    return English;
                case MicroverseLanguage.Portuguese:
                    return Portuguese;
                default:
                    return Spanish;
            }
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
    }
}
