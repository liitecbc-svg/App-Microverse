using System;
using System.Collections.Generic;
using System.Linq;
using Microverse.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Microverse.UI
{
    public class HomeScreenView
    {
        public GameObject Root { get; private set; }

        private readonly IReadOnlyList<BiologicalModel> models;
        private readonly IReadOnlyList<string> categories;
        private readonly MicroverseLanguage language;
        private readonly Action<BiologicalModel> onOpenModel;
        private readonly Action onCycleLanguage;
        private readonly Func<string, string> getText;
        private readonly Transform gridContent;
        private string searchTerm = string.Empty;
        private string categoryFilter = string.Empty;

        public HomeScreenView(Transform parent, IReadOnlyList<BiologicalModel> models, IReadOnlyList<string> categories, MicroverseLanguage language, Action<BiologicalModel> onOpenModel, Action onCycleLanguage, Func<string, string> getText)
        {
            this.models = models;
            this.categories = categories;
            this.language = language;
            this.onOpenModel = onOpenModel;
            this.onCycleLanguage = onCycleLanguage;
            this.getText = getText;

            Root = new GameObject("HomeScreen", typeof(RectTransform));
            Root.transform.SetParent(parent, false);
            UiFactory.Stretch(Root.GetComponent<RectTransform>());

            BuildHeader();
            BuildFeatureRow();
            BuildSearchAndFilters();
            gridContent = BuildCatalogGrid();
            RefreshGrid();
        }

        private void BuildHeader()
        {
            TextMeshProUGUI logo = UiFactory.Text("Logo", Root.transform, "MicroVerse\nAR", 46, FontStyles.Bold, MicroverseTheme.Text);
            logo.enableWordWrapping = false;
            RectTransform logoRect = logo.rectTransform;
            logoRect.anchorMin = new Vector2(0f, 1f);
            logoRect.anchorMax = new Vector2(0f, 1f);
            logoRect.pivot = new Vector2(0f, 1f);
            logoRect.anchoredPosition = new Vector2(54f, -34f);
            logoRect.sizeDelta = new Vector2(360f, 110f);

            Button search = UiFactory.Button("SearchButton", Root.transform, getText("common.search"), () => { }, MicroverseTheme.PanelLight, MicroverseTheme.Text, 18);
            RectTransform searchRect = search.GetComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(1f, 1f);
            searchRect.anchorMax = new Vector2(1f, 1f);
            searchRect.pivot = new Vector2(1f, 1f);
            searchRect.anchoredPosition = new Vector2(-154f, -44f);
            searchRect.sizeDelta = new Vector2(104f, 70f);

            Button settings = UiFactory.Button("LanguageButton", Root.transform, LanguageLabel(), () => onCycleLanguage(), MicroverseTheme.PanelLight, MicroverseTheme.Text, 18);
            RectTransform settingsRect = settings.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1f, 1f);
            settingsRect.anchorMax = new Vector2(1f, 1f);
            settingsRect.pivot = new Vector2(1f, 1f);
            settingsRect.anchoredPosition = new Vector2(-42f, -44f);
            settingsRect.sizeDelta = new Vector2(96f, 70f);

            TextMeshProUGUI hero = UiFactory.Text("Hero", Root.transform, getText("home.hero"), 32, FontStyles.Bold, MicroverseTheme.Text);
            RectTransform heroRect = hero.rectTransform;
            heroRect.anchorMin = new Vector2(0f, 1f);
            heroRect.anchorMax = new Vector2(1f, 1f);
            heroRect.offsetMin = new Vector2(54f, -220f);
            heroRect.offsetMax = new Vector2(-54f, -130f);
        }

        private void BuildFeatureRow()
        {
            GameObject row = new GameObject("FeatureRow", typeof(RectTransform));
            row.transform.SetParent(Root.transform, false);
            RectTransform rect = row.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(54f, -320f);
            rect.offsetMax = new Vector2(-54f, -242f);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 18;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            AddFeature(row.transform, getText("home.feature.ar.title"), getText("home.feature.ar.subtitle"), true);
            AddFeature(row.transform, getText("home.feature.library.title"), getText("home.feature.library.subtitle"), false);
            AddFeature(row.transform, getText("home.feature.quiz.title"), getText("home.feature.quiz.subtitle"), false);
            AddFeature(row.transform, getText("home.feature.favorites.title"), getText("home.feature.favorites.subtitle"), false);
        }

        private void BuildSearchAndFilters()
        {
            TMP_InputField input = UiFactory.Input("SearchInput", Root.transform, getText("home.search.placeholder"));
            RectTransform inputRect = input.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0f, 1f);
            inputRect.anchorMax = new Vector2(1f, 1f);
            inputRect.offsetMin = new Vector2(54f, -392f);
            inputRect.offsetMax = new Vector2(-54f, -336f);
            input.onValueChanged.AddListener(value =>
            {
                searchTerm = value.ToLowerInvariant();
                RefreshGrid();
            });

            GameObject filters = new GameObject("Filters", typeof(RectTransform));
            filters.transform.SetParent(Root.transform, false);
            RectTransform filtersRect = filters.GetComponent<RectTransform>();
            filtersRect.anchorMin = new Vector2(0f, 1f);
            filtersRect.anchorMax = new Vector2(1f, 1f);
            filtersRect.offsetMin = new Vector2(54f, -458f);
            filtersRect.offsetMax = new Vector2(-54f, -404f);

            HorizontalLayoutGroup layout = filters.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            AddFilter(filters.transform, getText("home.filter.all"), string.Empty);
            if (categories != null)
            {
                foreach (string cat in categories)
                {
                    AddFilter(filters.transform, cat, cat.ToLowerInvariant());
                }
            }
        }

        private Transform BuildCatalogGrid()
        {
            TextMeshProUGUI title = UiFactory.Text("SectionTitle", Root.transform, getText("home.section.life"), 28, FontStyles.Bold, MicroverseTheme.Text);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(54f, -520f);
            titleRect.offsetMax = new Vector2(-54f, -470f);

            GameObject viewport = UiFactory.Panel("CatalogViewport", Root.transform, new Color(0f, 0f, 0f, 0f), 0);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0f, 0f);
            viewportRect.anchorMax = new Vector2(1f, 1f);
            viewportRect.offsetMin = new Vector2(54f, 172f);
            viewportRect.offsetMax = new Vector2(-54f, -528f);

            ScrollRect scroll = viewport.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            GameObject mask = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            mask.transform.SetParent(viewport.transform, false);
            UiFactory.Stretch(mask.GetComponent<RectTransform>());
            Image maskImage = mask.GetComponent<Image>();
            maskImage.color = new Color(1f, 1f, 1f, 0.02f);
            mask.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(mask.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 1000f);

            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(294f, 324f);
            grid.spacing = new Vector2(24f, 24f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = mask.GetComponent<RectTransform>();
            scroll.content = contentRect;
            return content.transform;
        }

        private void RefreshGrid()
        {
            for (int i = gridContent.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(gridContent.GetChild(i).gameObject);
            }

            IEnumerable<BiologicalModel> filtered = models.Where(MatchesSearch).Where(MatchesCategory);
            foreach (BiologicalModel model in filtered)
            {
                new ModelCardView(gridContent, model, language, onOpenModel, getText);
            }
        }

        private bool MatchesSearch(BiologicalModel model)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return true;
            }

            string value = (model.Name.Get(language) + " " + model.Subtitle.Get(language) + " " + model.ScientificName).ToLowerInvariant();
            return value.Contains(searchTerm);
        }

        private bool MatchesCategory(BiologicalModel model)
        {
            if (string.IsNullOrWhiteSpace(categoryFilter))
            {
                return true;
            }

            string value = (
                model.Category.Get(MicroverseLanguage.Spanish) + " " +
                model.Category.Get(MicroverseLanguage.English) + " " +
                model.Category.Get(MicroverseLanguage.Portuguese) + " " +
                model.Subtitle.Get(MicroverseLanguage.Spanish) + " " +
                model.Subtitle.Get(MicroverseLanguage.English) + " " +
                model.Subtitle.Get(MicroverseLanguage.Portuguese)).ToLowerInvariant();
            return value.Contains(categoryFilter);
        }

        private void AddFeature(Transform parent, string title, string subtitle, bool active)
        {
            GameObject item = UiFactory.Panel("Feature-" + title, parent, active ? new Color(0.02f, 0.14f, 0.30f, 0.95f) : MicroverseTheme.Panel, 18);
            VerticalLayoutGroup layout = item.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 18, 12, 8);
            layout.spacing = 0;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            UiFactory.Text("Title", item.transform, title, 21, FontStyles.Bold, MicroverseTheme.Text);
            UiFactory.Text("Subtitle", item.transform, subtitle, 15, FontStyles.Normal, MicroverseTheme.MutedText);
        }

        private void AddFilter(Transform parent, string label, string value)
        {
            Button button = UiFactory.Button("Filter-" + label, parent, label, () =>
            {
                categoryFilter = value;
                RefreshGrid();
            }, MicroverseTheme.PanelLight, MicroverseTheme.Text, 17);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(160f, 46f);
        }

        private string LanguageLabel()
        {
            switch (language)
            {
                case MicroverseLanguage.English:
                    return "EN";
                case MicroverseLanguage.Portuguese:
                    return "PT";
                default:
                    return "ES";
            }
        }

    }
}
