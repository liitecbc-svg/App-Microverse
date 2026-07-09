/**
 * HomeScreenView.cs
 *
 * Construye la pantalla de catalogo principal con busqueda, filtros, favoritos, descargas y tarjetas de modelos.
 *
 * Main responsibilities:
 * - Filtrar modelos por modo, categoria y busqueda.
 * - Actualizar progreso de descarga por tarjeta sin reconstruir toda la UI.
 * - Coordinar apertura, descarga y favoritos desde el catalogo.
 *
 * Related elements:
 * - ModelCardView
 * - ModelDownloadStore
 * - FavoriteModelsStore
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Microverse.Data;
using Microverse.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Microverse.UI
{
    public class HomeScreenView
    {
        private const float FilterRowWidth = 972f;
        private const float FilterSpacing = 12f;
        private const float FilterMinWidth = 132f;
        private const float FilterMaxWidth = 226f;
        private const float FilterMoreWidth = 142f;
        private const float FilterRowSafetyInset = 34f;
        private const int PreferredCatalogColumns = 3;
        private const float CatalogCardMaxWidth = 300f;
        private const float CatalogCardMinWidth = 260f;
        private const float CatalogCardAspect = 350f / 300f;
        private static readonly Vector2 CatalogGridSpacing = new Vector2(24f, 24f);

        public enum CatalogMode
        {
            Ar,
            Library,
            Favorites
        }

        public GameObject Root { get; private set; }

        private readonly IReadOnlyList<BiologicalModel> models;
        private readonly MicroverseLanguage language;
        private readonly Action<BiologicalModel> onOpenModel;
        private readonly Action onCycleLanguage;
        private readonly Func<string, string> getText;
        private readonly Transform gridContent;
        private GridLayoutGroup catalogGrid;
        private RectTransform catalogViewportRect;
        private readonly Dictionary<string, Image> filterImages = new Dictionary<string, Image>();
        private readonly Dictionary<string, TextMeshProUGUI> filterLabels = new Dictionary<string, TextMeshProUGUI>();
        private readonly Dictionary<string, ModelCardView> modelCardsById = new Dictionary<string, ModelCardView>();
        private readonly HashSet<string> inlineFilterValues = new HashSet<string>();
        private readonly HashSet<string> downloadingModelIds = new HashSet<string>();
        private readonly Dictionary<string, float> downloadProgressById = new Dictionary<string, float>();
        private TextMeshProUGUI emptyStateText;
        private Image moreFilterImage;
        private TextMeshProUGUI moreFilterLabel;
        private GameObject categoryPickerOverlay;
        private string searchTerm = string.Empty;
        private string categoryFilter = string.Empty;
        private readonly CatalogMode catalogMode;

        public HomeScreenView(Transform parent, IReadOnlyList<BiologicalModel> models, IReadOnlyList<string> categories, MicroverseLanguage language, Action<BiologicalModel> onOpenModel, Action onCycleLanguage, Func<string, string> getText, CatalogMode catalogMode = CatalogMode.Ar)
        {
            this.models = models;
            this.language = language;
            this.onOpenModel = onOpenModel;
            this.onCycleLanguage = onCycleLanguage;
            this.getText = getText;
            this.catalogMode = catalogMode;

            Root = new GameObject("HomeScreen", typeof(RectTransform));
            Root.transform.SetParent(parent, false);
            UiFactory.Stretch(Root.GetComponent<RectTransform>());

            BuildHeader();
            BuildSearchAndFilters();
            gridContent = BuildCatalogGrid();
            RefreshGrid();
        }

        private void BuildHeader()
        {
            Texture2D logoTexture = Resources.Load<Texture2D>("AppLogo/microverse-logo-foreground");
            Sprite logoSprite = logoTexture == null
                ? null
                : Sprite.Create(logoTexture, new Rect(0f, 0f, logoTexture.width, logoTexture.height), new Vector2(0.5f, 0.5f));
            Image logo = UiFactory.Image("Logo", Root.transform, logoSprite, Color.white);
            RectTransform logoRect = logo.rectTransform;
            logoRect.anchorMin = new Vector2(0f, 1f);
            logoRect.anchorMax = new Vector2(0f, 1f);
            logoRect.pivot = new Vector2(0f, 1f);
            logoRect.anchoredPosition = new Vector2(54f, -24f);
            logoRect.sizeDelta = new Vector2(126f, 126f);

            TextMeshProUGUI hero = UiFactory.Text("Hero", Root.transform, getText("home.hero"), 32, FontStyles.Bold, MicroverseTheme.Text);
            RectTransform heroRect = hero.rectTransform;
            heroRect.anchorMin = new Vector2(0f, 1f);
            heroRect.anchorMax = new Vector2(1f, 1f);
            heroRect.offsetMin = new Vector2(54f, -275f);
            heroRect.offsetMax = new Vector2(-54f, -175f);
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
            filtersRect.offsetMin = new Vector2(54f, -464f);
            filtersRect.offsetMax = new Vector2(-54f, -406f);

            HorizontalLayoutGroup layout = filters.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = FilterSpacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            float availableFilterWidth = AvailableFilterRowWidth(filtersRect);
            float usedWidth = AddFilter(filters.transform, getText("home.filter.all"), string.Empty);
            int hiddenCategoryCount = 0;
            List<CategoryFilterOption> categoryOptions = BuildCategoryFilterOptions();
            if (categoryOptions.Count > 0)
            {
                for (int i = 0; i < categoryOptions.Count; i++)
                {
                    CategoryFilterOption option = categoryOptions[i];
                    string label = option.Label;
                    float width = PreferredFilterWidth(label);
                    bool hasMoreAfterThis = i < categoryOptions.Count - 1;
                    float reserveForMore = hiddenCategoryCount > 0 || hasMoreAfterThis ? FilterSpacing + FilterMoreWidth : 0f;
                    float spacing = FilterSpacing;

                    if (!CanShowInlineFilter(label) || usedWidth + spacing + width + reserveForMore > availableFilterWidth)
                    {
                        hiddenCategoryCount++;
                        continue;
                    }

                    usedWidth += spacing + AddFilter(filters.transform, label, option.Value, width);
                }

                if (hiddenCategoryCount > 0)
                {
                    AddMoreFilter(filters.transform);
                }
            }
            RefreshFilterSelection();
        }

        private Transform BuildCatalogGrid()
        {
            GameObject viewport = UiFactory.Panel("CatalogViewport", Root.transform, new Color(0f, 0f, 0f, 0f), 0);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0f, 0f);
            viewportRect.anchorMax = new Vector2(1f, 1f);
            viewportRect.offsetMin = new Vector2(54f, 172f);
            viewportRect.offsetMax = new Vector2(-54f, -498f);

            ScrollRect scroll = viewport.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            GameObject mask = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            mask.transform.SetParent(viewport.transform, false);
            catalogViewportRect = mask.GetComponent<RectTransform>();
            UiFactory.Stretch(catalogViewportRect);
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

            catalogGrid = content.AddComponent<GridLayoutGroup>();
            catalogGrid.cellSize = new Vector2(CatalogCardMaxWidth, CatalogCardMaxWidth * CatalogCardAspect);
            catalogGrid.spacing = CatalogGridSpacing;
            catalogGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            catalogGrid.constraintCount = PreferredCatalogColumns;
            catalogGrid.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = catalogViewportRect;
            scroll.content = contentRect;

            emptyStateText = UiFactory.Text("EmptyState", Root.transform, EmptyStateText(), 22, FontStyles.Bold, MicroverseTheme.MutedText, TextAlignmentOptions.Center);
            RectTransform emptyRect = emptyStateText.rectTransform;
            emptyRect.anchorMin = new Vector2(0f, 0f);
            emptyRect.anchorMax = new Vector2(1f, 1f);
            emptyRect.offsetMin = new Vector2(70f, 172f);
            emptyRect.offsetMax = new Vector2(-70f, -498f);
            emptyStateText.gameObject.SetActive(false);
            return content.transform;
        }

        private Vector2 ApplyResponsiveCatalogGrid()
        {
            if (catalogGrid == null)
            {
                return new Vector2(CatalogCardMaxWidth, CatalogCardMaxWidth * CatalogCardAspect);
            }

            Canvas.ForceUpdateCanvases();

            float availableWidth = catalogViewportRect != null ? catalogViewportRect.rect.width : 0f;
            if (availableWidth <= 1f)
            {
                RectTransform rootRect = Root != null ? Root.GetComponent<RectTransform>() : null;
                availableWidth = rootRect != null ? rootRect.rect.width - 108f : FilterRowWidth;
            }

            if (availableWidth <= 1f)
            {
                availableWidth = FilterRowWidth;
            }

            int columns = PreferredCatalogColumns;
            while (columns > 1)
            {
                float candidateWidth = (availableWidth - CatalogGridSpacing.x * (columns - 1)) / columns;
                if (candidateWidth >= CatalogCardMinWidth)
                {
                    break;
                }

                columns--;
            }

            float cardWidth = (availableWidth - CatalogGridSpacing.x * (columns - 1)) / columns;
            cardWidth = Mathf.Clamp(cardWidth, CatalogCardMinWidth, CatalogCardMaxWidth);
            float cardHeight = Mathf.Round(cardWidth * CatalogCardAspect);

            catalogGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            catalogGrid.constraintCount = columns;
            catalogGrid.spacing = CatalogGridSpacing;
            catalogGrid.cellSize = new Vector2(cardWidth, cardHeight);
            return catalogGrid.cellSize;
        }

        private void RefreshGrid()
        {
            Vector2 cardSize = ApplyResponsiveCatalogGrid();
            for (int i = gridContent.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(gridContent.GetChild(i).gameObject);
            }
            modelCardsById.Clear();

            IEnumerable<BiologicalModel> filtered = models.Where(MatchesCatalogMode).Where(MatchesSearch).Where(MatchesCategory);
            List<BiologicalModel> visibleModels = filtered.ToList();
            foreach (BiologicalModel model in visibleModels)
            {
                bool available = ModelDownloadStore.IsAvailable(model);
                bool isDownloading = IsDownloading(model);
                bool canDownload = CanDownload(model, available) && !isDownloading;
                bool showDownloadControl = canDownload || isDownloading;
                bool openable = !isDownloading && (available || (catalogMode == CatalogMode.Favorites && canDownload));
                Action<BiologicalModel> openAction = available ? onOpenModel : DownloadModelThenOpen;
                float progress = DownloadProgress(model);
                ModelCardView card = new ModelCardView(gridContent, model, language, openAction, getText, RefreshGrid, openable, showDownloadControl, canDownload ? DownloadModel : null, isDownloading, progress, cardSize.x, cardSize.y);
                if (!string.IsNullOrWhiteSpace(model.Id))
                {
                    modelCardsById[model.Id] = card;
                }
            }

            if (emptyStateText != null)
            {
                emptyStateText.text = EmptyStateText();
                emptyStateText.gameObject.SetActive(visibleModels.Count == 0);
            }
        }

        private bool MatchesCatalogMode(BiologicalModel model)
        {
            switch (catalogMode)
            {
                case CatalogMode.Library:
                    return IsPendingDownload(model);
                case CatalogMode.Favorites:
                    return FavoriteModelsStore.IsFavorite(model.Id);
                default:
                    return ModelDownloadStore.IsAvailable(model);
            }
        }

        private bool IsPendingDownload(BiologicalModel model)
        {
            return model != null
                && !model.IsBundledModel
                && !ModelDownloadStore.IsAvailable(model)
                && !string.IsNullOrWhiteSpace(model.ModelFileUrl);
        }

        private bool CanDownload(BiologicalModel model, bool available)
        {
            return !available
                && !string.IsNullOrWhiteSpace(model.ModelFileUrl)
                && (catalogMode == CatalogMode.Library || catalogMode == CatalogMode.Favorites);
        }

        private bool IsDownloading(BiologicalModel model)
        {
            return model != null && !string.IsNullOrWhiteSpace(model.Id) && downloadingModelIds.Contains(model.Id);
        }

        private float DownloadProgress(BiologicalModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Id))
            {
                return 0f;
            }

            return downloadProgressById.TryGetValue(model.Id, out float progress) ? progress : 0f;
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

        private void DownloadModel(BiologicalModel model)
        {
            StartModelDownload(model, false);
        }

        private void DownloadModelThenOpen(BiologicalModel model)
        {
            StartModelDownload(model, true);
        }

        private void StartModelDownload(BiologicalModel model, bool openWhenDone)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Id) || downloadingModelIds.Contains(model.Id))
            {
                return;
            }

            MonoBehaviour runner = Root.GetComponentInParent<MonoBehaviour>();
            if (runner == null)
            {
                Debug.LogWarning("Cannot download model without a coroutine runner.");
                return;
            }

            downloadingModelIds.Add(model.Id);
            downloadProgressById[model.Id] = 0f;
            if (modelCardsById.TryGetValue(model.Id, out ModelCardView card))
            {
                card.BeginDownloadProgress(0f);
            }

            runner.StartCoroutine(ModelDownloadStore.DownloadModelRoutine(model, (success, error) =>
            {
                downloadingModelIds.Remove(model.Id);
                downloadProgressById.Remove(model.Id);

                if (!success)
                {
                    Debug.LogWarning("Model download failed: " + error);
                    ReplaceModelCard(model);
                    return;
                }

                if (openWhenDone)
                {
                    CompleteDownloadedModelCard(model);
                    onOpenModel?.Invoke(model);
                    return;
                }

                CompleteDownloadedModelCard(model);
            },
            progress => UpdateDownloadProgress(model.Id, progress)));
        }

        private void UpdateDownloadProgress(string modelId, float progress)
        {
            if (string.IsNullOrWhiteSpace(modelId) || !downloadingModelIds.Contains(modelId))
            {
                return;
            }

            downloadProgressById[modelId] = Mathf.Clamp01(progress);
            if (modelCardsById.TryGetValue(modelId, out ModelCardView card))
            {
                card.SetDownloadProgress(progress);
            }
        }

        private void CompleteDownloadedModelCard(BiologicalModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Id))
            {
                RefreshGrid();
                return;
            }

            if (catalogMode == CatalogMode.Library && modelCardsById.TryGetValue(model.Id, out ModelCardView card))
            {
                modelCardsById.Remove(model.Id);
                UnityEngine.Object.Destroy(card.Root);
                UpdateEmptyStateForCurrentGrid();
                return;
            }

            ReplaceModelCard(model);
        }

        private void ReplaceModelCard(BiologicalModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Id) || !modelCardsById.TryGetValue(model.Id, out ModelCardView oldCard))
            {
                RefreshGrid();
                return;
            }

            int siblingIndex = oldCard.Root.transform.GetSiblingIndex();
            UnityEngine.Object.Destroy(oldCard.Root);
            modelCardsById.Remove(model.Id);

            bool available = ModelDownloadStore.IsAvailable(model);
            bool isDownloading = IsDownloading(model);
            bool canDownload = CanDownload(model, available) && !isDownloading;
            bool showDownloadControl = canDownload || isDownloading;
            bool openable = !isDownloading && (available || (catalogMode == CatalogMode.Favorites && canDownload));
            Action<BiologicalModel> openAction = available ? onOpenModel : DownloadModelThenOpen;
            float progress = DownloadProgress(model);
            Vector2 cardSize = catalogGrid != null ? catalogGrid.cellSize : new Vector2(CatalogCardMaxWidth, CatalogCardMaxWidth * CatalogCardAspect);

            ModelCardView newCard = new ModelCardView(gridContent, model, language, openAction, getText, RefreshGrid, openable, showDownloadControl, canDownload ? DownloadModel : null, isDownloading, progress, cardSize.x, cardSize.y);
            newCard.Root.transform.SetSiblingIndex(siblingIndex);
            modelCardsById[model.Id] = newCard;
        }

        private void UpdateEmptyStateForCurrentGrid()
        {
            if (emptyStateText == null)
            {
                return;
            }

            emptyStateText.text = EmptyStateText();
            emptyStateText.gameObject.SetActive(gridContent.childCount == 0);
        }

        private float AddFilter(Transform parent, string label, string value)
        {
            return AddFilter(parent, label, value, PreferredFilterWidth(label));
        }

        private float AddFilter(Transform parent, string label, string value, float width)
        {
            Button button = UiFactory.Button("Filter-" + label, parent, label, () =>
            {
                categoryFilter = value;
                RefreshFilterSelection();
                RefreshGrid();
            }, new Color(0.03f, 0.08f, 0.17f, 0.88f), MicroverseTheme.Text, 16);

            filterImages[value] = button.GetComponent<Image>();
            TextMeshProUGUI labelText = button.GetComponentInChildren<TextMeshProUGUI>();
            UiFactory.ConfigureButtonLabel(labelText, 16, 12);
            labelText.overflowMode = TextOverflowModes.Overflow;
            filterLabels[value] = labelText;
            inlineFilterValues.Add(value);
            LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = width;
            layout.preferredWidth = width;
            layout.flexibleWidth = 0f;
            return width;
        }

        private void AddMoreFilter(Transform parent)
        {
            Button button = UiFactory.Button("Filter-More", parent, getText("home.filter.more"), ShowCategoryPicker, new Color(0.03f, 0.08f, 0.17f, 0.88f), MicroverseTheme.Text, 16);
            moreFilterImage = button.GetComponent<Image>();
            moreFilterLabel = button.GetComponentInChildren<TextMeshProUGUI>();
            UiFactory.ConfigureButtonLabel(moreFilterLabel, 16, 10);
            LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = FilterMoreWidth;
            layout.preferredWidth = FilterMoreWidth;
            layout.flexibleWidth = 0f;
        }

        private float AvailableFilterRowWidth(RectTransform filtersRect)
        {
            Canvas.ForceUpdateCanvases();

            float width = filtersRect != null ? filtersRect.rect.width : 0f;
            if (width <= 1f)
            {
                RectTransform rootRect = Root != null ? Root.GetComponent<RectTransform>() : null;
                width = rootRect != null ? rootRect.rect.width - 108f : FilterRowWidth;
            }

            if (width <= 1f)
            {
                width = FilterRowWidth;
            }

            return Mathf.Max(FilterMinWidth + FilterSpacing + FilterMoreWidth, width - FilterRowSafetyInset);
        }

        private void RefreshFilterSelection()
        {
            foreach (KeyValuePair<string, Image> entry in filterImages)
            {
                bool active = entry.Key == categoryFilter;
                entry.Value.color = active ? new Color(0.0f, 0.24f, 0.48f, 0.95f) : new Color(0.03f, 0.08f, 0.17f, 0.88f);
            }

            foreach (KeyValuePair<string, TextMeshProUGUI> entry in filterLabels)
            {
                bool active = entry.Key == categoryFilter;
                entry.Value.color = active ? MicroverseTheme.Cyan : MicroverseTheme.Text;
            }

            if (moreFilterImage != null && moreFilterLabel != null)
            {
                bool active = !string.IsNullOrWhiteSpace(categoryFilter) && !inlineFilterValues.Contains(categoryFilter);
                moreFilterImage.color = active ? new Color(0.0f, 0.24f, 0.48f, 0.95f) : new Color(0.03f, 0.08f, 0.17f, 0.88f);
                moreFilterLabel.color = active ? MicroverseTheme.Cyan : MicroverseTheme.Text;
                moreFilterLabel.text = active ? SelectedCategoryLabel() : getText("home.filter.more");
                UiFactory.ConfigureButtonLabel(moreFilterLabel, 16, 10);
            }
        }

        private void ShowCategoryPicker()
        {
            if (categoryPickerOverlay != null)
            {
                UnityEngine.Object.Destroy(categoryPickerOverlay);
            }

            categoryPickerOverlay = UiFactory.Panel("CategoryPickerOverlay", Root.transform, new Color(0f, 0f, 0f, 0.58f), 0);
            UiFactory.Stretch(categoryPickerOverlay.GetComponent<RectTransform>());

            GameObject panel = UiFactory.Panel("CategoryPickerPanel", categoryPickerOverlay.transform, new Color(0.02f, 0.06f, 0.14f, 0.98f), 26);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.08f, 0.22f);
            panelRect.anchorMax = new Vector2(0.92f, 0.72f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI title = UiFactory.Text("Title", panel.transform, getText("nav.categories"), 28, FontStyles.Bold, MicroverseTheme.Text);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(34f, -74f);
            titleRect.offsetMax = new Vector2(-170f, -24f);

            Button close = UiFactory.Button("Close", panel.transform, getText("common.close"), CloseCategoryPicker, MicroverseTheme.PanelLight, MicroverseTheme.Text, 16);
            RectTransform closeRect = close.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-28f, -24f);
            closeRect.sizeDelta = new Vector2(124f, 50f);

            GameObject viewport = UiFactory.Panel("CategoryListViewport", panel.transform, new Color(0f, 0f, 0f, 0f), 0);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0f, 0f);
            viewportRect.anchorMax = new Vector2(1f, 1f);
            viewportRect.offsetMin = new Vector2(28f, 28f);
            viewportRect.offsetMax = new Vector2(-28f, -92f);

            ScrollRect scroll = viewport.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

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

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            AddPickerRow(content.transform, getText("home.filter.all"), string.Empty);
            List<CategoryFilterOption> categoryOptions = BuildCategoryFilterOptions();
            if (categoryOptions.Count > 0)
            {
                foreach (CategoryFilterOption option in categoryOptions)
                {
                    AddPickerRow(content.transform, option.Label, option.Value);
                }
            }

            scroll.viewport = mask.GetComponent<RectTransform>();
            scroll.content = contentRect;
        }

        private void AddPickerRow(Transform parent, string label, string value)
        {
            bool active = value == categoryFilter;
            Button button = UiFactory.Button("Category-" + label, parent, label, () =>
            {
                categoryFilter = value;
                CloseCategoryPicker();
                RefreshFilterSelection();
                RefreshGrid();
            }, active ? new Color(0.0f, 0.24f, 0.48f, 0.95f) : MicroverseTheme.PanelLight, active ? MicroverseTheme.Cyan : MicroverseTheme.Text, 18);

            TextMeshProUGUI labelText = button.GetComponentInChildren<TextMeshProUGUI>();
            UiFactory.ConfigureButtonLabel(labelText, 18, 12, true);
            labelText.overflowMode = TextOverflowModes.Overflow;

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 68f);

            LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 68f;
        }

        private void CloseCategoryPicker()
        {
            if (categoryPickerOverlay == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(categoryPickerOverlay);
            categoryPickerOverlay = null;
        }

        private string SelectedCategoryLabel()
        {
            List<CategoryFilterOption> categoryOptions = BuildCategoryFilterOptions();
            foreach (CategoryFilterOption option in categoryOptions)
            {
                if (option.Value == categoryFilter)
                {
                    return option.Label;
                }
            }

            return getText("home.filter.more");
        }

        private string EmptyStateText()
        {
            if (catalogMode == CatalogMode.Favorites)
            {
                return getText("home.empty.favorites");
            }

            if (catalogMode == CatalogMode.Library)
            {
                return getText("home.empty.library");
            }

            return getText("home.empty.ar");
        }

        private List<CategoryFilterOption> BuildCategoryFilterOptions()
        {
            List<CategoryFilterOption> options = new List<CategoryFilterOption>();
            HashSet<string> knownValues = new HashSet<string>();
            if (models == null)
            {
                return options;
            }

            foreach (BiologicalModel model in models.Where(MatchesCatalogMode))
            {
                if (model == null || model.Category == null)
                {
                    continue;
                }

                string value = CategoryFilterValue(model.Category);
                if (string.IsNullOrWhiteSpace(value) || knownValues.Contains(value))
                {
                    continue;
                }

                knownValues.Add(value);
                options.Add(new CategoryFilterOption(value, model.Category.Get(language)));
            }

            return options;
        }

        private string CategoryFilterValue(LocalizedText category)
        {
            string value = category.Get(MicroverseLanguage.English);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = category.Get(MicroverseLanguage.Spanish);
            }

            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.ToLowerInvariant();
        }

        private float PreferredFilterWidth(string label)
        {
            int length = string.IsNullOrWhiteSpace(label) ? 0 : label.Trim().Length;
            return Mathf.Clamp(80f + length * 8.5f, FilterMinWidth, FilterMaxWidth);
        }

        private bool CanShowInlineFilter(string label)
        {
            return string.IsNullOrWhiteSpace(label) || label.Trim().Length <= 17;
        }

        private readonly struct CategoryFilterOption
        {
            public readonly string Value;
            public readonly string Label;

            public CategoryFilterOption(string value, string label)
            {
                Value = value;
                Label = label;
            }
        }



    }
}
