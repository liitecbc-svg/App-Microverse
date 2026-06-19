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
                "animal-cell",
                new LocalizedText("Celula animal", "Animal Cell", "Celula animal"),
                new LocalizedText("Celula eucariota", "Eukaryotic Cell", "Celula eucariotica"),
                new LocalizedText("Tipos de celulas", "Types of Cells", "Tipos de celulas"),
                new LocalizedText(
                    "Las celulas animales son unidades basicas de vida con nucleo y organelos rodeados por membrana.",
                    "Animal cells are basic units of life with a nucleus and membrane-bound organelles.",
                    "As celulas animais sao unidades basicas da vida com nucleo e organelas membranosas."),
                "Cellula animalis",
                new Color(0.0f, 0.63f, 1.0f),
                new Color(0.75f, 0.24f, 1.0f),
                14),
            new BiologicalModel(
                "plant-cell",
                new LocalizedText("Celula vegetal", "Plant Cell", "Celula vegetal"),
                new LocalizedText("Celula eucariota", "Eukaryotic Cell", "Celula eucariotica"),
                new LocalizedText("Tipos de celulas", "Types of Cells", "Tipos de celulas"),
                new LocalizedText(
                    "La celula vegetal posee pared celular, cloroplastos y una vacuola central que ayuda a mantener su estructura.",
                    "Plant cells contain a cell wall, chloroplasts, and a central vacuole that helps maintain structure.",
                    "A celula vegetal possui parede celular, cloroplastos e vacuolo central."),
                "Cellula plantae",
                new Color(0.24f, 0.82f, 0.24f),
                new Color(0.08f, 0.55f, 0.86f),
                22),
            new BiologicalModel(
                "yeast-cell",
                new LocalizedText("Levadura", "Yeast Cell", "Celula de levedura"),
                new LocalizedText("Hongo", "Fungus", "Fungo"),
                new LocalizedText("Microorganismos", "Microorganisms", "Microorganismos"),
                new LocalizedText(
                    "La levadura es un hongo unicelular usado para estudiar procesos celulares y fermentacion.",
                    "Yeast is a single-celled fungus used to study cellular processes and fermentation.",
                    "A levedura e um fungo unicelular usado para estudar processos celulares."),
                "Saccharomyces cerevisiae",
                new Color(0.44f, 0.56f, 1.0f),
                new Color(1.0f, 0.38f, 0.25f),
                31),
            new BiologicalModel(
                "paramecium",
                new LocalizedText("Paramecio", "Paramecium", "Paramecio"),
                new LocalizedText("Protozoo", "Protozoan", "Protozoario"),
                new LocalizedText("Protozoos", "Protozoans", "Protozoarios"),
                new LocalizedText(
                    "El paramecio se desplaza mediante cilios y vive en ambientes acuaticos.",
                    "Paramecium moves using cilia and lives in aquatic environments.",
                    "O paramecio se move por cilios e vive em ambientes aquaticos."),
                "Paramecium caudatum",
                new Color(0.08f, 0.86f, 0.58f),
                new Color(0.92f, 0.78f, 0.16f),
                44,
                true),
            new BiologicalModel(
                "amoeba",
                new LocalizedText("Ameba", "Amoeba", "Ameba"),
                new LocalizedText("Protozoo", "Protozoan", "Protozoario"),
                new LocalizedText("Protozoos", "Protozoans", "Protozoarios"),
                new LocalizedText(
                    "La ameba cambia de forma para desplazarse y capturar alimento mediante seudopodos.",
                    "Amoeba changes shape to move and capture food using pseudopods.",
                    "A ameba muda de forma para se mover e capturar alimento."),
                "Amoeba proteus",
                new Color(0.53f, 0.74f, 1.0f),
                new Color(1.0f, 0.45f, 0.14f),
                51),
            new BiologicalModel(
                "euglena",
                new LocalizedText("Euglena", "Euglena", "Euglena"),
                new LocalizedText("Protozoo", "Protozoan", "Protozoario"),
                new LocalizedText("Protozoos", "Protozoans", "Protozoarios"),
                new LocalizedText(
                    "La euglena combina rasgos vegetales y animales, y puede moverse con un flagelo.",
                    "Euglena combines plant-like and animal-like traits and can move using a flagellum.",
                    "A euglena combina caracteristicas vegetais e animais e se move com flagelo."),
                "Euglena gracilis",
                new Color(0.48f, 0.91f, 0.08f),
                new Color(0.0f, 0.78f, 0.62f),
                63,
                true),
            new BiologicalModel(
                "e-coli",
                new LocalizedText("Escherichia coli", "Escherichia coli", "Escherichia coli"),
                new LocalizedText("Bacteria", "Bacterium", "Bacteria"),
                new LocalizedText("Bacterias", "Bacteria", "Bacterias"),
                new LocalizedText(
                    "E. coli es una bacteria frecuente en estudios de genetica, salud y microbiologia.",
                    "E. coli is a bacterium commonly used in genetics, health, and microbiology studies.",
                    "E. coli e uma bacteria comum em estudos de genetica, saude e microbiologia."),
                "Escherichia coli",
                new Color(0.73f, 0.95f, 0.25f),
                new Color(0.17f, 0.76f, 0.27f),
                72,
                true),
            new BiologicalModel(
                "adenovirus",
                new LocalizedText("Adenovirus", "Adenovirus", "Adenovirus"),
                new LocalizedText("Virus", "Virus", "Virus"),
                new LocalizedText("Virus", "Viruses", "Virus"),
                new LocalizedText(
                    "Los adenovirus son virus de ADN que pueden causar infecciones respiratorias y oculares.",
                    "Adenoviruses are DNA viruses that can cause respiratory and eye infections.",
                    "Os adenovirus sao virus de DNA que podem causar infeccoes respiratorias e oculares."),
                "Adenoviridae",
                new Color(0.55f, 0.27f, 1.0f),
                new Color(0.07f, 0.62f, 1.0f),
                83),
            new BiologicalModel(
                "influenza",
                new LocalizedText("Virus influenza", "Influenza Virus", "Virus influenza"),
                new LocalizedText("Virus", "Virus", "Virus"),
                new LocalizedText("Virus", "Viruses", "Virus"),
                new LocalizedText(
                    "El virus influenza contiene ARN y posee proteinas de superficie relevantes para su identificacion.",
                    "Influenza virus contains RNA and surface proteins that are relevant for identification.",
                    "O virus influenza contem RNA e proteinas de superficie importantes para identificacao."),
                "Orthomyxoviridae",
                new Color(0.0f, 0.77f, 1.0f),
                new Color(1.0f, 0.23f, 0.86f),
                95)
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
            foreach (var m in models)
            {
                string catName = m.Category.Get(MicroverseLanguage.Spanish);
                if (!cats.Contains(catName))
                {
                    cats.Add(catName);
                }
            }
            return cats;
        }
    }
}
