/**
 * LocalModelCatalogService.cs
 *
 * Entrega el catalogo base incluido dentro de la aplicacion para que Microverse funcione sin conexion.
 *
 * Main responsibilities:
 * - Declarar modelos demo empaquetados como recursos locales.
 * - Resolver categorias locales desde los modelos incluidos.
 * - Responder inmediatamente a solicitudes de carga del catalogo local.
 *
 * Related elements:
 * - BiologicalModel
 * - CompositeModelCatalogService
 * - Resources/Models
 */
using System.Collections.Generic;
using Microverse.Data;
using UnityEngine;

namespace Microverse.Services
{
    public class LocalModelCatalogService : IModelCatalogService
    {
        private readonly List<BiologicalModel> models = new List<BiologicalModel>
        {
            new BiologicalModel(
                "cromossomo",
                new LocalizedText("Cromosoma", "Chromosome", "Cromossomo"),
                new LocalizedText("Demo", "Demo", "Demo"),
                new LocalizedText("Demo", "Demo", "Demo"),
                new LocalizedText(
                    "Los cromosomas son estructuras de ADN y proteinas que contienen la informacion genetica y ayudan a distribuirla durante la division celular.",
                    "Chromosomes are DNA and protein structures that contain genetic information and help distribute it during cell division.",
                    "Os cromossomos sao estruturas de DNA e proteinas que contem informacao genetica e ajudam a distribui-la durante a divisao celular."),
                "Chromosoma",
                new Color(0.88f, 0.22f, 0.44f),
                new Color(0.36f, 0.65f, 1.0f),
                7,
                false,
                "resource:Models/Cromossomo",
                "resource:ModelPreviews/cromossomo-preview",
                true),
            new BiologicalModel(
                "celula-animal",
                new LocalizedText("Celula Animal", "Animal Cell", "Celula Animal"),
                new LocalizedText("Demo", "Demo", "Demo"),
                new LocalizedText("Demo", "Demo", "Demo"),
                new LocalizedText(
                    "La celula animal es una unidad eucariota con organelos especializados, membrana plasmatica y nucleo definido.",
                    "The animal cell is a eukaryotic unit with specialized organelles, a plasma membrane, and a defined nucleus.",
                    "A celula animal e uma unidade eucariotica com organelos especializados, membrana plasmatica e nucleo definido."),
                "Animal cell",
                new Color(0.25f, 0.68f, 0.80f),
                new Color(0.86f, 0.36f, 0.72f),
                23,
                false,
                "resource:Models/685dc3ac-df6a-4771-ab12-cecb8448d745-celula-animal",
                "resource:ModelPreviews/celula-animal-preview",
                true)
        };

        public IReadOnlyList<BiologicalModel> GetModels()
        {
            return models;
        }

        public void LoadModels(System.Action<IReadOnlyList<BiologicalModel>> onComplete, System.Action<string> onError)
        {
            onComplete?.Invoke(models);
        }

        public IReadOnlyList<string> GetCategories()
        {
            List<string> cats = new List<string>();
            foreach (BiologicalModel model in models)
            {
                string catName = model.Category.Get(MicroverseLanguage.Spanish);
                if (!cats.Contains(catName))
                {
                    cats.Add(catName);
                }
            }

            return cats;
        }
    }
}
