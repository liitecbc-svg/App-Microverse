/**
 * BiologicalModel.cs
 *
 * Representa un modelo biologico disponible en la aplicacion, incluyendo textos localizados, colores, rutas de archivos y estado de contenido incluido.
 *
 * Main responsibilities:
 * - Centralizar los datos que describen cada modelo 3D o procedimental.
 * - Guardar referencias a previews, modelos descargables o recursos incluidos.
 * - Exponer metadatos visuales usados por la UI y por el visor AR.
 *
 * Related elements:
 * - LocalizedText
 * - IModelCatalogService
 * - BiologyVisualFactory
 */
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
        public bool IsBundledModel;
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
            string previewUrl = "",
            bool isBundledModel = false)
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
            IsBundledModel = isBundledModel;
            LoadedPreviewSprite = null;
        }
    }
}
