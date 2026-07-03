using System;
using System.Collections.Generic;
using Microverse.Data;
using Microverse.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Microverse.UI
{
    public class CategoriesScreenView
    {
        public GameObject Root { get; private set; }

        private readonly IReadOnlyList<BiologicalModel> models;
        private readonly MicroverseLanguage language;
        private readonly Action<BiologicalModel> onOpenModel;
        private readonly Func<string, string> getText;

        public CategoriesScreenView(
            Transform parent, 
            IReadOnlyList<BiologicalModel> models, 
            IReadOnlyList<string> categories, 
            MicroverseLanguage language, 
            Action<BiologicalModel> onOpenModel, 
            Func<string, string> getText)
        {
            this.models = models;
            this.language = language;
            this.onOpenModel = onOpenModel;
            this.getText = getText;

            Root = new GameObject("CategoriesScreen", typeof(RectTransform));
            Root.transform.SetParent(parent, false);
            UiFactory.Stretch(Root.GetComponent<RectTransform>());

            BuildHeader();
            BuildCategoryRows();
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

            TextMeshProUGUI title = UiFactory.Text("Title", Root.transform, getText("nav.categories"), 34, FontStyles.Bold, MicroverseTheme.Text);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(54f, -220f);
            titleRect.offsetMax = new Vector2(-54f, -140f);
        }

        private void BuildCategoryRows()
        {
            // Vertical Scroll Container for the rows
            GameObject viewport = UiFactory.Panel("CategoriesViewport", Root.transform, new Color(0f, 0f, 0f, 0f), 0);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0f, 0f);
            viewportRect.anchorMax = new Vector2(1f, 1f);
            viewportRect.offsetMin = new Vector2(54f, 172f); // Aligned with home catalog grid bottom
            viewportRect.offsetMax = new Vector2(-54f, -242f); // Positioned under the header

            ScrollRect verticalScroll = viewport.AddComponent<ScrollRect>();
            verticalScroll.horizontal = false;
            verticalScroll.vertical = true;

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
            contentRect.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup verticalLayout = content.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 40;
            verticalLayout.childControlHeight = true;
            verticalLayout.childControlWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childForceExpandWidth = true;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            verticalScroll.viewport = mask.GetComponent<RectTransform>();
            verticalScroll.content = contentRect;

            // Generate one row per translated model category.
            Dictionary<string, List<BiologicalModel>> groupedModels = new Dictionary<string, List<BiologicalModel>>();
            if (models != null)
            {
                foreach (BiologicalModel model in models)
                {
                    if (model == null || model.Category == null || !ModelDownloadStore.IsAvailable(model))
                    {
                        continue;
                    }

                    string key = CategoryKey(model.Category);
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    if (!groupedModels.TryGetValue(key, out List<BiologicalModel> categoryModels))
                    {
                        categoryModels = new List<BiologicalModel>();
                        groupedModels[key] = categoryModels;
                    }

                    categoryModels.Add(model);
                }
            }

            foreach (KeyValuePair<string, List<BiologicalModel>> entry in groupedModels)
            {
                if (entry.Value.Count > 0)
                {
                    BuildCategoryRow(content.transform, DisplayCategoryLabel(entry.Value), entry.Value);
                }
            }
        }

        private void BuildCategoryRow(Transform parent, string categoryName, List<BiologicalModel> categoryModels)
        {
            GameObject rowContainer = new GameObject("Row-" + categoryName, typeof(RectTransform));
            rowContainer.transform.SetParent(parent, false);
            RectTransform rowRect = rowContainer.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 432f);

            VerticalLayoutGroup rowLayout = rowContainer.AddComponent<VerticalLayoutGroup>();
            rowLayout.spacing = 12;
            rowLayout.childControlHeight = false;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childForceExpandWidth = true;

            // Row Title
            TextMeshProUGUI title = UiFactory.Text("CategoryTitle", rowContainer.transform, categoryName, 26, FontStyles.Bold, MicroverseTheme.Text);
            RectTransform titleRect = title.rectTransform;
            titleRect.sizeDelta = new Vector2(0f, 40f);

            // Horizontal ScrollView for model cards
            GameObject horizontalViewport = UiFactory.Panel("HorizontalViewport", rowContainer.transform, new Color(0f, 0f, 0f, 0f), 0);
            RectTransform horizViewportRect = horizontalViewport.GetComponent<RectTransform>();
            horizViewportRect.sizeDelta = new Vector2(0f, 372f);

            ScrollRect horizontalScroll = horizontalViewport.AddComponent<ScrollRect>();
            horizontalScroll.horizontal = true;
            horizontalScroll.vertical = false;

            GameObject horizMask = new GameObject("Mask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            horizMask.transform.SetParent(horizontalViewport.transform, false);
            UiFactory.Stretch(horizMask.GetComponent<RectTransform>());
            Image horizMaskImage = horizMask.GetComponent<Image>();
            horizMaskImage.color = new Color(1f, 1f, 1f, 0.02f);
            horizMask.GetComponent<Mask>().showMaskGraphic = false;

            GameObject horizContent = new GameObject("Content", typeof(RectTransform));
            horizContent.transform.SetParent(horizMask.transform, false);
            RectTransform horizContentRect = horizContent.GetComponent<RectTransform>();
            horizContentRect.anchorMin = new Vector2(0f, 0.5f);
            horizContentRect.anchorMax = new Vector2(0f, 0.5f);
            horizContentRect.pivot = new Vector2(0f, 0.5f);
            horizContentRect.anchoredPosition = Vector2.zero;
            horizContentRect.sizeDelta = new Vector2(0f, 350f);

            HorizontalLayoutGroup horizLayout = horizContent.AddComponent<HorizontalLayoutGroup>();
            horizLayout.spacing = 18;
            horizLayout.childControlHeight = true;
            horizLayout.childControlWidth = true;
            horizLayout.childForceExpandHeight = true;
            horizLayout.childForceExpandWidth = false;

            ContentSizeFitter horizFitter = horizContent.AddComponent<ContentSizeFitter>();
            horizFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            horizontalScroll.viewport = horizMask.GetComponent<RectTransform>();
            horizontalScroll.content = horizContentRect;

            // Instantiate cards inside the row
            foreach (BiologicalModel model in categoryModels)
            {
                new ModelCardView(horizContent.transform, model, language, onOpenModel, getText);
            }
        }

        private string DisplayCategoryLabel(IReadOnlyList<BiologicalModel> categoryModels)
        {
            if (categoryModels == null)
            {
                return string.Empty;
            }

            foreach (BiologicalModel model in categoryModels)
            {
                if (model != null && model.Category != null)
                {
                    string translated = model.Category.Get(language);
                    if (!string.IsNullOrWhiteSpace(translated))
                    {
                        return translated;
                    }
                }
            }

            return string.Empty;
        }

        private string CategoryKey(LocalizedText category)
        {
            string key = category.Get(MicroverseLanguage.English);
            if (string.IsNullOrWhiteSpace(key))
            {
                key = category.Get(MicroverseLanguage.Spanish);
            }

            return string.IsNullOrWhiteSpace(key) ? string.Empty : key.ToLowerInvariant();
        }
    }
}
