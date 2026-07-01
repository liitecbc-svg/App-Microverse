using System;
using Microverse.Data;
using Microverse.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Microverse.UI
{
    public class ModelCardView
    {
        private static readonly Color FavoriteGold = new Color(1.0f, 0.74f, 0.18f, 1f);
        private static Sprite emptyStarSprite;
        private static Sprite filledStarSprite;

        public GameObject Root { get; private set; }

        public ModelCardView(
            Transform parent,
            BiologicalModel model,
            MicroverseLanguage language,
            Action<BiologicalModel> onOpen,
            Func<string, string> getText,
            Action onFavoriteChanged = null,
            bool isAvailableForAr = true,
            bool showDownloadButton = false,
            Action<BiologicalModel> onDownload = null)
        {
            Root = UiFactory.Panel("ModelCard-" + model.Id, parent, new Color(0.02f, 0.06f, 0.14f, 0.96f), 24);
            Image frame = Root.GetComponent<Image>();
            frame.color = MicroverseTheme.Panel;

            Button button = Root.AddComponent<Button>();
            button.targetGraphic = frame;
            button.colors = UiFactory.ButtonColors(MicroverseTheme.Panel);
            button.onClick.AddListener(() =>
            {
                if (isAvailableForAr)
                {
                    onOpen?.Invoke(model);
                }
            });

            RectTransform rect = Root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(294f, 324f);

            GameObject visualWell = UiFactory.Panel("VisualWell", Root.transform, new Color(0.01f, 0.04f, 0.10f, 0.94f), 20);
            RectTransform wellRect = visualWell.GetComponent<RectTransform>();
            wellRect.anchorMin = new Vector2(0f, 0.36f);
            wellRect.anchorMax = new Vector2(1f, 1f);
            wellRect.offsetMin = new Vector2(8f, 6f);
            wellRect.offsetMax = new Vector2(-8f, -8f);

            Image visual = UiFactory.Image("Visual", visualWell.transform, BiologyVisualFactory.CreateModelSprite(model), Color.white);
            visual.preserveAspect = false;
            RectTransform visualRect = visual.rectTransform;
            UiFactory.Stretch(visualRect, 4f, 4f);

            if (!string.IsNullOrEmpty(model.PreviewUrl))
            {
                if (model.LoadedPreviewSprite != null)
                {
                    visual.sprite = model.LoadedPreviewSprite;
                }
                else if (BiologyVisualFactory.TryLoadPreviewSprite(model.PreviewUrl, out Sprite localPreview))
                {
                    model.LoadedPreviewSprite = localPreview;
                    visual.sprite = localPreview;
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

            bool initiallyFavorite = FavoriteModelsStore.IsFavorite(model.Id);
            Button favoriteButton = CreateFavoriteButton(Root.transform, initiallyFavorite);
            RectTransform favRect = favoriteButton.GetComponent<RectTransform>();
            favRect.anchorMin = new Vector2(1f, 1f);
            favRect.anchorMax = new Vector2(1f, 1f);
            favRect.pivot = new Vector2(1f, 1f);
            favRect.anchoredPosition = new Vector2(-16f, -16f);
            favRect.sizeDelta = new Vector2(46f, 46f);

            Image favoriteIcon = favoriteButton.transform.Find("StarIcon").GetComponent<Image>();
            favoriteButton.onClick.AddListener(() =>
            {
                FavoriteModelsStore.Toggle(model.Id);
                bool active = FavoriteModelsStore.IsFavorite(model.Id);
                favoriteIcon.sprite = GetStarSprite(active);
                onFavoriteChanged?.Invoke();
            });

            TextMeshProUGUI title = UiFactory.Text("Title", Root.transform, model.Name.Get(language), 23, FontStyles.Bold, MicroverseTheme.Text);
            title.enableAutoSizing = true;
            title.fontSizeMax = 23;
            title.fontSizeMin = 14;
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(1f, 0f);
            titleRect.offsetMin = showDownloadButton ? new Vector2(22f, 88f) : new Vector2(22f, 64f);
            titleRect.offsetMax = showDownloadButton ? new Vector2(-22f, 132f) : new Vector2(-22f, 112f);

            TextMeshProUGUI subtitle = UiFactory.Text("Subtitle", Root.transform, model.Subtitle.Get(language), 18, FontStyles.Normal, MicroverseTheme.MutedText);
            subtitle.enableAutoSizing = true;
            subtitle.fontSizeMax = 18;
            subtitle.fontSizeMin = 12;
            RectTransform subtitleRect = subtitle.rectTransform;
            subtitleRect.anchorMin = new Vector2(0f, 0f);
            subtitleRect.anchorMax = new Vector2(1f, 0f);
            subtitleRect.offsetMin = showDownloadButton ? new Vector2(22f, 60f) : new Vector2(22f, 34f);
            subtitleRect.offsetMax = showDownloadButton ? new Vector2(-22f, 88f) : new Vector2(-22f, 66f);

            if (showDownloadButton)
            {
                Button download = UiFactory.Button("DownloadModel", Root.transform, getText("model.download"), () => onDownload?.Invoke(model), new Color(0.0f, 0.24f, 0.48f, 0.95f), MicroverseTheme.Cyan, 17);
                RectTransform downloadRect = download.GetComponent<RectTransform>();
                downloadRect.anchorMin = new Vector2(0f, 0f);
                downloadRect.anchorMax = new Vector2(1f, 0f);
                downloadRect.offsetMin = new Vector2(22f, 14f);
                downloadRect.offsetMax = new Vector2(-22f, 54f);
                UiFactory.ConfigureButtonLabel(download.GetComponentInChildren<TextMeshProUGUI>(), 17, 11);
            }
        }

        private static Button CreateFavoriteButton(Transform parent, bool active)
        {
            GameObject root = UiFactory.Panel("FavoriteStar", parent, new Color(0.01f, 0.03f, 0.08f, 0.74f), 16);
            Button button = root.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();
            button.transition = Selectable.Transition.ColorTint;
            button.colors = UiFactory.ButtonColors(new Color(0.01f, 0.03f, 0.08f, 0.74f));

            GameObject iconGo = new GameObject("StarIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(root.transform, false);
            Image icon = iconGo.GetComponent<Image>();
            icon.sprite = GetStarSprite(active);
            icon.color = Color.white;
            icon.preserveAspect = true;
            UiFactory.Stretch(icon.rectTransform, 8f, 8f);

            return button;
        }

        private static Sprite GetStarSprite(bool filled)
        {
            if (filled)
            {
                if (filledStarSprite == null)
                {
                    filledStarSprite = CreateStarSprite(true);
                }

                return filledStarSprite;
            }

            if (emptyStarSprite == null)
            {
                emptyStarSprite = CreateStarSprite(false);
            }

            return emptyStarSprite;
        }

        private static Sprite CreateStarSprite(bool filled)
        {
            const int size = 96;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size * 0.5f, size * 0.52f);
            Vector2[] outer = BuildStarPoints(center, size * 0.42f, size * 0.18f);
            Vector2[] inner = BuildStarPoints(center, size * 0.30f, size * 0.12f);
            Color color = filled ? FavoriteGold : MicroverseTheme.Text;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                    bool insideOuter = ContainsPoint(outer, point);
                    bool insideInner = ContainsPoint(inner, point);
                    bool draw = filled ? insideOuter : insideOuter && !insideInner;
                    texture.SetPixel(x, y, draw ? color : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Vector2[] BuildStarPoints(Vector2 center, float outerRadius, float innerRadius)
        {
            Vector2[] points = new Vector2[10];
            for (int i = 0; i < points.Length; i++)
            {
                float radius = i % 2 == 0 ? outerRadius : innerRadius;
                float angle = -90f + i * 36f;
                float radians = angle * Mathf.Deg2Rad;
                points[i] = center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
            }

            return points;
        }

        private static bool ContainsPoint(Vector2[] polygon, Vector2 point)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                bool crossesY = (polygon[i].y > point.y) != (polygon[j].y > point.y);
                bool intersects = crossesY &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x;
                if (intersects)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

    }
}
