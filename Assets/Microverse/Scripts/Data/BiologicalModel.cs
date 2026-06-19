using UnityEngine;

namespace Microverse.Data
{
    [System.Serializable]
    public class BiologicalModel
    {
        public string Id;
        public LocalizedText Name;
        public LocalizedText Subtitle;
        public LocalizedText Category;
        public LocalizedText Description;
        public string ScientificName;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public int VisualSeed;
        public bool IsElongated;

        public string ModelFileUrl;
        public string PreviewUrl;
        [System.NonSerialized]
        public Sprite LoadedPreviewSprite;

        public BiologicalModel(
            string id,
            LocalizedText name,
            LocalizedText subtitle,
            LocalizedText category,
            LocalizedText description,
            string scientificName,
            Color primaryColor,
            Color secondaryColor,
            int visualSeed,
            bool isElongated = false,
            string modelFileUrl = "",
            string previewUrl = "")
        {
            Id = id;
            Name = name;
            Subtitle = subtitle;
            Category = category;
            Description = description;
            ScientificName = scientificName;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
            VisualSeed = visualSeed;
            IsElongated = isElongated;
            ModelFileUrl = modelFileUrl;
            PreviewUrl = previewUrl;
            LoadedPreviewSprite = null;
        }
    }
}
