using System;
using Microverse.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Microverse.UI
{
    public class ModelCardView
    {
        public GameObject Root { get; private set; }

        public ModelCardView(Transform parent, BiologicalModel model, MicroverseLanguage language, Action<BiologicalModel> onOpen, Func<string, string> getText)
        {
            Root = UiFactory.Panel("ModelCard-" + model.Id, parent, new Color(0.02f, 0.06f, 0.14f, 0.94f), 22);
            Image frame = Root.GetComponent<Image>();
            frame.color = MicroverseTheme.Panel;

            Button button = Root.AddComponent<Button>();
            button.targetGraphic = frame;
            button.colors = UiFactory.ButtonColors(MicroverseTheme.Panel);
            button.onClick.AddListener(() => onOpen(model));

            RectTransform rect = Root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(294f, 324f);

            Image visual = UiFactory.Image("Visual", Root.transform, BiologyVisualFactory.CreateModelSprite(model), Color.white);
            RectTransform visualRect = visual.rectTransform;
            visualRect.anchorMin = new Vector2(0f, 0.34f);
            visualRect.anchorMax = new Vector2(1f, 1f);
            visualRect.offsetMin = new Vector2(14f, 0f);
            visualRect.offsetMax = new Vector2(-14f, -12f);

            if (!string.IsNullOrEmpty(model.PreviewUrl))
            {
                if (model.LoadedPreviewSprite != null)
                {
                    visual.sprite = model.LoadedPreviewSprite;
                }
                else
                {
                    MonoBehaviour runner = parent.GetComponentInParent<MonoBehaviour>();
                    if (runner != null)
                    {
                        runner.StartCoroutine(BiologyVisualFactory.DownloadPreviewTextureRoutine(model.PreviewUrl, sprite =>
                        {
                            model.LoadedPreviewSprite = sprite;
                            if (visual != null)
                            {
                                visual.sprite = sprite;
                            }
                        }));
                    }
                }
            }

            TextMeshProUGUI favorite = UiFactory.Text("Favorite", Root.transform, getText("model.favorite"), 18, FontStyles.Bold, MicroverseTheme.MutedText, TextAlignmentOptions.Center);
            RectTransform favRect = favorite.rectTransform;
            favRect.anchorMin = new Vector2(1f, 1f);
            favRect.anchorMax = new Vector2(1f, 1f);
            favRect.pivot = new Vector2(1f, 1f);
            favRect.anchoredPosition = new Vector2(-18f, -16f);
            favRect.sizeDelta = new Vector2(54f, 30f);

            TextMeshProUGUI title = UiFactory.Text("Title", Root.transform, model.Name.Get(language), 23, FontStyles.Bold, MicroverseTheme.Text);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(1f, 0f);
            titleRect.offsetMin = new Vector2(20f, 86f);
            titleRect.offsetMax = new Vector2(-18f, 132f);

            TextMeshProUGUI subtitle = UiFactory.Text("Subtitle", Root.transform, model.Subtitle.Get(language), 18, FontStyles.Normal, MicroverseTheme.MutedText);
            RectTransform subtitleRect = subtitle.rectTransform;
            subtitleRect.anchorMin = new Vector2(0f, 0f);
            subtitleRect.anchorMax = new Vector2(1f, 0f);
            subtitleRect.offsetMin = new Vector2(20f, 60f);
            subtitleRect.offsetMax = new Vector2(-18f, 90f);

            Button arButton = UiFactory.Button("ViewAr", Root.transform, getText("model.view_ar"), () => onOpen(model), new Color(0.03f, 0.10f, 0.22f, 0.95f), MicroverseTheme.Cyan, 20);
            RectTransform arRect = arButton.GetComponent<RectTransform>();
            arRect.anchorMin = new Vector2(0f, 0f);
            arRect.anchorMax = new Vector2(1f, 0f);
            arRect.offsetMin = new Vector2(20f, 14f);
            arRect.offsetMax = new Vector2(-20f, 58f);
        }
    }
}
