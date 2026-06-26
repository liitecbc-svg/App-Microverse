using System;
using System.Collections.Generic;
using Microverse.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Microverse.UI
{
    public class DetailScreenView
    {
        public GameObject Root { get; private set; }

        private readonly IReadOnlyList<BiologicalModel> models;
        private readonly MicroverseLanguage language;
        private readonly Action onBack;
        private readonly Action<BiologicalModel> onViewAR;
        private readonly Func<string, string> getText;
        private int currentIndex;
        private Image mainVisual;
        private TextMeshProUGUI title;
        private TextMeshProUGUI subtitle;
        private TextMeshProUGUI counter;
        private TextMeshProUGUI aboutTitle;
        private TextMeshProUGUI aboutBody;
        private TextMeshProUGUI sideLeft;
        private TextMeshProUGUI sideRight;

        public DetailScreenView(Transform parent, IReadOnlyList<BiologicalModel> models, BiologicalModel selected, MicroverseLanguage language, Action onBack, Action<BiologicalModel> onViewAR, Func<string, string> getText)
        {
            this.models = models;
            this.language = language;
            this.onBack = onBack;
            this.onViewAR = onViewAR;
            this.getText = getText;
            currentIndex = Mathf.Max(0, IndexOf(selected));

            Root = new GameObject("DetailScreen", typeof(RectTransform));
            Root.transform.SetParent(parent, false);
            UiFactory.Stretch(Root.GetComponent<RectTransform>());

            BuildHeader();
            BuildCarousel();
            BuildActions();
            BuildAbout();
            Refresh();
        }

        private void BuildHeader()
        {
            TextMeshProUGUI logo = UiFactory.Text("Logo", Root.transform, "MicroVerse\nAR", 44, FontStyles.Bold, MicroverseTheme.Text);
            logo.enableWordWrapping = false;
            RectTransform logoRect = logo.rectTransform;
            logoRect.anchorMin = new Vector2(0f, 1f);
            logoRect.anchorMax = new Vector2(0f, 1f);
            logoRect.pivot = new Vector2(0f, 1f);
            logoRect.anchoredPosition = new Vector2(42f, -26f);
            logoRect.sizeDelta = new Vector2(330f, 110f);

            Button back = UiFactory.Button("Back", Root.transform, "<  " + getText("detail.back.types"), () => onBack(), new Color(0f, 0f, 0f, 0f), MicroverseTheme.Text, 25);
            RectTransform backRect = back.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 1f);
            backRect.anchorMax = new Vector2(0f, 1f);
            backRect.pivot = new Vector2(0f, 1f);
            backRect.anchoredPosition = new Vector2(38f, -166f);
            backRect.sizeDelta = new Vector2(340f, 70f);

            Button categories = UiFactory.Button("Categories", Root.transform, getText("detail.categories"), () => { }, MicroverseTheme.PanelLight, MicroverseTheme.Text, 22);
            RectTransform categoriesRect = categories.GetComponent<RectTransform>();
            categoriesRect.anchorMin = new Vector2(1f, 1f);
            categoriesRect.anchorMax = new Vector2(1f, 1f);
            categoriesRect.pivot = new Vector2(1f, 1f);
            categoriesRect.anchoredPosition = new Vector2(-42f, -162f);
            categoriesRect.sizeDelta = new Vector2(220f, 64f);

            Button search = UiFactory.Button("SearchButton", Root.transform, getText("common.search"), () => { }, MicroverseTheme.PanelLight, MicroverseTheme.Text, 18);
            RectTransform searchRect = search.GetComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(1f, 1f);
            searchRect.anchorMax = new Vector2(1f, 1f);
            searchRect.pivot = new Vector2(1f, 1f);
            searchRect.anchoredPosition = new Vector2(-152f, -34f);
            searchRect.sizeDelta = new Vector2(104f, 70f);

            Button settings = UiFactory.Button("Settings", Root.transform, getText("common.menu"), () => { }, MicroverseTheme.PanelLight, MicroverseTheme.Text, 18);
            RectTransform settingsRect = settings.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(1f, 1f);
            settingsRect.anchorMax = new Vector2(1f, 1f);
            settingsRect.pivot = new Vector2(1f, 1f);
            settingsRect.anchoredPosition = new Vector2(-40f, -34f);
            settingsRect.sizeDelta = new Vector2(96f, 70f);
        }

        private void BuildCarousel()
        {
            title = UiFactory.Text("Title", Root.transform, string.Empty, 50, FontStyles.Bold, MicroverseTheme.Text, TextAlignmentOptions.Center);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(80f, -322f);
            titleRect.offsetMax = new Vector2(-80f, -248f);

            subtitle = UiFactory.Text("Subtitle", Root.transform, string.Empty, 28, FontStyles.Normal, MicroverseTheme.MutedText, TextAlignmentOptions.Center);
            RectTransform subtitleRect = subtitle.rectTransform;
            subtitleRect.anchorMin = new Vector2(0f, 1f);
            subtitleRect.anchorMax = new Vector2(1f, 1f);
            subtitleRect.offsetMin = new Vector2(80f, -368f);
            subtitleRect.offsetMax = new Vector2(-80f, -320f);

            counter = UiFactory.Text("Counter", Root.transform, string.Empty, 24, FontStyles.Bold, MicroverseTheme.Text, TextAlignmentOptions.Center);
            GameObject counterBg = UiFactory.Panel("CounterBg", Root.transform, new Color(0.05f, 0.12f, 0.28f, 0.9f), 20);
            RectTransform counterBgRect = counterBg.GetComponent<RectTransform>();
            counterBgRect.anchorMin = new Vector2(0.5f, 1f);
            counterBgRect.anchorMax = new Vector2(0.5f, 1f);
            counterBgRect.pivot = new Vector2(0.5f, 1f);
            counterBgRect.anchoredPosition = new Vector2(0f, -374f);
            counterBgRect.sizeDelta = new Vector2(112f, 42f);
            counter.transform.SetParent(counterBg.transform, false);
            UiFactory.Stretch(counter.rectTransform);

            GameObject leftCard = UiFactory.Panel("LeftPreview", Root.transform, new Color(0.03f, 0.09f, 0.20f, 0.75f), 22);
            RectTransform leftRect = leftCard.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0f, 0.5f);
            leftRect.anchorMax = new Vector2(0f, 0.5f);
            leftRect.pivot = new Vector2(0f, 0.5f);
            leftRect.anchoredPosition = new Vector2(-120f, 245f);
            leftRect.sizeDelta = new Vector2(280f, 380f);
            sideLeft = UiFactory.Text("Label", leftCard.transform, string.Empty, 26, FontStyles.Bold, MicroverseTheme.MutedText, TextAlignmentOptions.Center);
            UiFactory.Stretch(sideLeft.rectTransform, 20, 22);

            GameObject rightCard = UiFactory.Panel("RightPreview", Root.transform, new Color(0.03f, 0.09f, 0.20f, 0.75f), 22);
            RectTransform rightRect = rightCard.GetComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(1f, 0.5f);
            rightRect.anchorMax = new Vector2(1f, 0.5f);
            rightRect.pivot = new Vector2(1f, 0.5f);
            rightRect.anchoredPosition = new Vector2(120f, 245f);
            rightRect.sizeDelta = new Vector2(280f, 380f);
            sideRight = UiFactory.Text("Label", rightCard.transform, string.Empty, 26, FontStyles.Bold, MicroverseTheme.MutedText, TextAlignmentOptions.Center);
            UiFactory.Stretch(sideRight.rectTransform, 20, 22);

            mainVisual = UiFactory.Image("MainVisual", Root.transform, null, Color.white);
            RectTransform visualRect = mainVisual.rectTransform;
            visualRect.anchorMin = new Vector2(0.5f, 1f);
            visualRect.anchorMax = new Vector2(0.5f, 1f);
            visualRect.pivot = new Vector2(0.5f, 1f);
            visualRect.anchoredPosition = new Vector2(0f, -430f);
            visualRect.sizeDelta = new Vector2(620f, 620f);

            Button previous = UiFactory.Button("Previous", Root.transform, "<", Previous, MicroverseTheme.PanelLight, MicroverseTheme.Text, 54);
            RectTransform prevRect = previous.GetComponent<RectTransform>();
            prevRect.anchorMin = new Vector2(0f, 0.5f);
            prevRect.anchorMax = new Vector2(0f, 0.5f);
            prevRect.pivot = new Vector2(0f, 0.5f);
            prevRect.anchoredPosition = new Vector2(132f, 100f);
            prevRect.sizeDelta = new Vector2(96f, 96f);

            Button next = UiFactory.Button("Next", Root.transform, ">", Next, MicroverseTheme.PanelLight, MicroverseTheme.Text, 54);
            RectTransform nextRect = next.GetComponent<RectTransform>();
            nextRect.anchorMin = new Vector2(1f, 0.5f);
            nextRect.anchorMax = new Vector2(1f, 0.5f);
            nextRect.pivot = new Vector2(1f, 0.5f);
            nextRect.anchoredPosition = new Vector2(-132f, 100f);
            nextRect.sizeDelta = new Vector2(96f, 96f);

            TextMeshProUGUI hint = UiFactory.Text("Hint", Root.transform, getText("detail.swipe"), 22, FontStyles.Normal, MicroverseTheme.MutedText, TextAlignmentOptions.Center);
            RectTransform hintRect = hint.rectTransform;
            hintRect.anchorMin = new Vector2(0f, 1f);
            hintRect.anchorMax = new Vector2(1f, 1f);
            hintRect.offsetMin = new Vector2(80f, -1028f);
            hintRect.offsetMax = new Vector2(-80f, -982f);
        }

        private void BuildActions()
        {
            Button view3d = UiFactory.Button("View3D", Root.transform, getText("detail.view_3d"), () => onViewAR?.Invoke(models[currentIndex]), MicroverseTheme.PanelLight, MicroverseTheme.Text, 26);
            RectTransform view3dRect = view3d.GetComponent<RectTransform>();
            view3dRect.anchorMin = new Vector2(0f, 0f);
            view3dRect.anchorMax = new Vector2(0f, 0f);
            view3dRect.pivot = new Vector2(0f, 0f);
            view3dRect.anchoredPosition = new Vector2(128f, 386f);
            view3dRect.sizeDelta = new Vector2(280f, 86f);

            Button viewAr = UiFactory.Button("ViewAR", Root.transform, getText("detail.view_ar"), () => onViewAR?.Invoke(models[currentIndex]), new Color(0.0f, 0.42f, 0.68f, 0.96f), MicroverseTheme.Text, 26);
            RectTransform viewArRect = viewAr.GetComponent<RectTransform>();
            viewArRect.anchorMin = new Vector2(1f, 0f);
            viewArRect.anchorMax = new Vector2(1f, 0f);
            viewArRect.pivot = new Vector2(1f, 0f);
            viewArRect.anchoredPosition = new Vector2(-128f, 386f);
            viewArRect.sizeDelta = new Vector2(280f, 86f);
        }

        private void BuildAbout()
        {
            GameObject card = UiFactory.Panel("AboutCard", Root.transform, new Color(0.02f, 0.07f, 0.16f, 0.94f), 24);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0f, 0f);
            cardRect.anchorMax = new Vector2(1f, 0f);
            cardRect.offsetMin = new Vector2(34f, 190f);
            cardRect.offsetMax = new Vector2(-34f, 360f);

            aboutTitle = UiFactory.Text("AboutTitle", card.transform, string.Empty, 28, FontStyles.Bold, MicroverseTheme.Text);
            RectTransform titleRect = aboutTitle.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(38f, -62f);
            titleRect.offsetMax = new Vector2(-240f, -20f);

            aboutBody = UiFactory.Text("AboutBody", card.transform, string.Empty, 22, FontStyles.Normal, MicroverseTheme.MutedText);
            RectTransform bodyRect = aboutBody.rectTransform;
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.offsetMin = new Vector2(38f, 22f);
            bodyRect.offsetMax = new Vector2(-270f, -72f);

            Button details = UiFactory.Button("Details", card.transform, getText("common.details"), () => { }, MicroverseTheme.PanelLight, MicroverseTheme.Text, 22);
            RectTransform detailsRect = details.GetComponent<RectTransform>();
            detailsRect.anchorMin = new Vector2(1f, 0.5f);
            detailsRect.anchorMax = new Vector2(1f, 0.5f);
            detailsRect.pivot = new Vector2(1f, 0.5f);
            detailsRect.anchoredPosition = new Vector2(-36f, 0f);
            detailsRect.sizeDelta = new Vector2(190f, 64f);
        }

        private void Refresh()
        {
            BiologicalModel current = models[currentIndex];
            BiologicalModel previous = models[(currentIndex - 1 + models.Count) % models.Count];
            BiologicalModel next = models[(currentIndex + 1) % models.Count];

            title.text = current.Name.Get(language);
            subtitle.text = current.Subtitle.Get(language);
            counter.text = (currentIndex + 1) + " / " + models.Count;
            if (!string.IsNullOrEmpty(current.PreviewUrl))
            {
                if (current.LoadedPreviewSprite != null)
                {
                    mainVisual.sprite = current.LoadedPreviewSprite;
                }
                else
                {
                    mainVisual.sprite = BiologyVisualFactory.CreateModelSprite(current);
                    MonoBehaviour runner = Root.GetComponentInParent<MonoBehaviour>();
                    if (runner != null)
                    {
                        runner.StartCoroutine(BiologyVisualFactory.DownloadPreviewTextureRoutine(current.PreviewUrl, sprite => {
                            current.LoadedPreviewSprite = sprite;
                            if (mainVisual != null && models[currentIndex] == current)
                            {
                                mainVisual.sprite = sprite;
                            }
                        }));
                    }
                }
            }
            else
            {
                mainVisual.sprite = BiologyVisualFactory.CreateModelSprite(current);
            }
            aboutTitle.text = getText("detail.about_prefix") + current.Name.Get(language);
            aboutBody.text = current.Description.Get(language);
            sideLeft.text = previous.Name.Get(language) + "\n" + ((currentIndex - 1 + models.Count) % models.Count + 1) + " / " + models.Count;
            sideRight.text = next.Name.Get(language) + "\n" + ((currentIndex + 1) % models.Count + 1) + " / " + models.Count;
        }

        private void Previous()
        {
            currentIndex = (currentIndex - 1 + models.Count) % models.Count;
            Refresh();
        }

        private void Next()
        {
            currentIndex = (currentIndex + 1) % models.Count;
            Refresh();
        }

        private int IndexOf(BiologicalModel selected)
        {
            for (int i = 0; i < models.Count; i++)
            {
                if (models[i].Id == selected.Id)
                {
                    return i;
                }
            }

            return 0;
        }

    }
}
