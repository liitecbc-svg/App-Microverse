using System;
using Microverse.Data;
using Microverse.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Microverse.UI
{
    public class ModelCardView
    {
        private static readonly Color FavoriteGold = new Color(1.0f, 0.74f, 0.18f, 1f);
        private static Sprite emptyStarSprite;
        private static Sprite filledStarSprite;
        private readonly MicroverseLanguage language;
        private readonly Func<string, string> getText;
        private Button downloadButton;
        private TextMeshProUGUI downloadProgressText;
        private RectTransform downloadProgressFillRect;

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
            Action<BiologicalModel> onDownload = null,
            bool isDownloading = false,
            float downloadProgress = 0f,
            float cardWidth = 300f,
            float cardHeight = 350f)
        {
            this.language = language;
            this.getText = getText;
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
            rect.sizeDelta = new Vector2(cardWidth, cardHeight);

            GameObject visualWell = UiFactory.Panel("VisualWell", Root.transform, new Color(0.01f, 0.04f, 0.10f, 0.94f), 20);
            RectTransform wellRect = visualWell.GetComponent<RectTransform>();
            wellRect.anchorMin = new Vector2(0f, 0.43f);
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
            title.maxVisibleLines = 2;
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(1f, 0f);
            titleRect.offsetMin = showDownloadButton ? new Vector2(22f, 98f) : new Vector2(22f, 78f);
            titleRect.offsetMax = showDownloadButton ? new Vector2(-22f, 148f) : new Vector2(-22f, 132f);

            TextMeshProUGUI subtitle = UiFactory.Text("Subtitle", Root.transform, model.Subtitle.Get(language), 18, FontStyles.Normal, MicroverseTheme.MutedText);
            subtitle.enableAutoSizing = true;
            subtitle.fontSizeMax = 18;
            subtitle.fontSizeMin = 12;
            subtitle.maxVisibleLines = 2;
            RectTransform subtitleRect = subtitle.rectTransform;
            subtitleRect.anchorMin = new Vector2(0f, 0f);
            subtitleRect.anchorMax = new Vector2(1f, 0f);
            subtitleRect.offsetMin = showDownloadButton ? new Vector2(22f, 58f) : new Vector2(22f, 36f);
            subtitleRect.offsetMax = showDownloadButton ? new Vector2(-22f, 96f) : new Vector2(-22f, 76f);

            if (showDownloadButton)
            {
                string downloadLabel = isDownloading ? DownloadProgressLabel(downloadProgress) : getText("model.download");
                Button download = UiFactory.Button("DownloadModel", Root.transform, downloadLabel, () =>
                {
                    if (!isDownloading)
                    {
                        onDownload?.Invoke(model);
                    }
                }, new Color(0.0f, 0.24f, 0.48f, 0.95f), MicroverseTheme.Cyan, 17);
                RectTransform downloadRect = download.GetComponent<RectTransform>();
                downloadRect.anchorMin = new Vector2(0f, 0f);
                downloadRect.anchorMax = new Vector2(1f, 0f);
                downloadRect.offsetMin = new Vector2(22f, 14f);
                downloadRect.offsetMax = new Vector2(-22f, 54f);
                download.interactable = !isDownloading;
                downloadButton = download;

                TextMeshProUGUI downloadText = download.GetComponentInChildren<TextMeshProUGUI>();
                if (isDownloading)
                {
                    BeginDownloadProgress(downloadProgress);
                }

                UiFactory.ConfigureButtonLabel(downloadText, 17, 11);
            }

            // Check if the model is downloaded (only downloaded models can be deleted)
            if (ModelDownloadStore.IsDownloaded(model.Id))
            {
                LongPressTrigger longPress = Root.AddComponent<LongPressTrigger>();
                longPress.duration = 0.8f;
                longPress.onLongPress.AddListener(() =>
                {
                    Canvas canvas = parent.GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        ShowDeleteConfirmationDialog(canvas.transform, model, () =>
                        {
                            ModelDownloadStore.DeleteDownloadedModel(model.Id);
                            onFavoriteChanged?.Invoke(); // Refresh the grid
                        });
                    }
                });
            }
        }

        public void BeginDownloadProgress(float progress)
        {
            if (downloadButton == null)
            {
                return;
            }

            downloadButton.interactable = false;
            if (downloadProgressText == null)
            {
                downloadProgressText = downloadButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (downloadProgressFillRect == null)
            {
                downloadProgressFillRect = AddDownloadProgressFill(downloadButton.transform);
            }

            if (downloadProgressText != null)
            {
                downloadProgressText.transform.SetAsLastSibling();
            }

            SetDownloadProgress(progress);
        }

        public void SetDownloadProgress(float progress)
        {
            if (downloadProgressFillRect != null)
            {
                float clamped = Mathf.Clamp01(progress);
                downloadProgressFillRect.anchorMax = new Vector2(clamped, 1f);
                downloadProgressFillRect.offsetMax = Vector2.zero;
            }

            if (downloadProgressText != null)
            {
                downloadProgressText.text = DownloadProgressLabel(progress);
            }
        }

        private static RectTransform AddDownloadProgressFill(Transform parent)
        {
            GameObject fillGo = new GameObject("DownloadProgressFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.SetAsFirstSibling();

            Image fill = fillGo.GetComponent<Image>();
            fill.color = new Color(0f, 0.86f, 1f, 0.34f);
            fill.raycastTarget = false;

            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            return fillRect;
        }

        private string DownloadProgressLabel(float progress)
        {
            int percentage = Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f);
            string format = getText != null ? getText("model.downloading") : "Downloading {0}%";
            return format.Replace("{0}", percentage.ToString());
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

        private void ShowDeleteConfirmationDialog(Transform canvasTransform, BiologicalModel model, Action onDeleteConfirmed)
        {
            // 1. Create Modal Background (blocks all inputs behind it)
            GameObject modalBg = UiFactory.Panel("DeleteConfirmationModal", canvasTransform, new Color(0.01f, 0.03f, 0.08f, 0.85f), 0);
            RectTransform bgRect = modalBg.GetComponent<RectTransform>();
            UiFactory.Stretch(bgRect);
            
            // Add a CanvasGroup and make it blocksRaycasts = true to ensure it blocks clicks
            CanvasGroup bgGroup = modalBg.AddComponent<CanvasGroup>();
            bgGroup.blocksRaycasts = true;

            // 2. Create Dialog Panel (in the center)
            GameObject dialogPanel = UiFactory.Panel("DialogPanel", modalBg.transform, new Color(0.03f, 0.10f, 0.22f, 0.98f), 24);
            RectTransform dialogRect = dialogPanel.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.anchoredPosition = Vector2.zero;
            dialogRect.sizeDelta = new Vector2(600f, 380f);

            // 3. Title Text
            string titleStr = language == MicroverseLanguage.Spanish ? "¿Eliminar modelo?" : 
                              (language == MicroverseLanguage.Portuguese ? "Excluir modelo?" : "Delete model?");
            TextMeshProUGUI titleText = UiFactory.Text("DialogTitle", dialogPanel.transform, titleStr, 28, FontStyles.Bold, MicroverseTheme.Cyan, TextAlignmentOptions.Center);
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(24f, -70f);
            titleRect.offsetMax = new Vector2(-24f, -24f);

            // 4. Description Text
            string descStr = language == MicroverseLanguage.Spanish ? $"¿Estás seguro de que deseas eliminar '{model.Name.Get(language)}' de tu dispositivo? Deberás descargarlo nuevamente para verlo sin conexión." :
                             (language == MicroverseLanguage.Portuguese ? $"Tem certeza de que deseja excluir '{model.Name.Get(language)}' do seu dispositivo? Você precisará baixá-lo novamente para vê-lo off-line." :
                             $"Are you sure you want to delete '{model.Name.Get(language)}' from your device? You will need to download it again to view it offline.");
            
            TextMeshProUGUI desc = UiFactory.Text("DialogDesc", dialogPanel.transform, descStr, 20, FontStyles.Normal, MicroverseTheme.Text, TextAlignmentOptions.Center);
            RectTransform descRect = desc.rectTransform;
            descRect.anchorMin = new Vector2(0f, 1f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.pivot = new Vector2(0.5f, 1f);
            descRect.offsetMin = new Vector2(34f, -240f);
            descRect.offsetMax = new Vector2(-34f, -90f);

            // 5. Cancel ("No") Button
            string noStr = language == MicroverseLanguage.Spanish ? "No" : "No";
            Button cancelButton = UiFactory.Button("CancelButton", dialogPanel.transform, noStr, () => {
                UnityEngine.Object.Destroy(modalBg);
            }, MicroverseTheme.PanelLight, MicroverseTheme.Text, 22);
            RectTransform cancelRect = cancelButton.GetComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0f, 0f);
            cancelRect.anchorMax = new Vector2(0.5f, 0f);
            cancelRect.pivot = new Vector2(0.5f, 0f);
            cancelRect.offsetMin = new Vector2(34f, 28f);
            cancelRect.offsetMax = new Vector2(-12f, 92f);

            // 6. Confirm ("Sí") Button
            string yesStr = language == MicroverseLanguage.Spanish ? "Sí" : 
                            (language == MicroverseLanguage.Portuguese ? "Sim" : "Yes");
            Button confirmButton = UiFactory.Button("ConfirmButton", dialogPanel.transform, yesStr, () => {
                onDeleteConfirmed?.Invoke();
                UnityEngine.Object.Destroy(modalBg);
            }, new Color(0.72f, 0.15f, 0.20f, 0.95f), MicroverseTheme.Text, 22);
            RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.5f, 0f);
            confirmRect.anchorMax = new Vector2(1f, 0f);
            confirmRect.pivot = new Vector2(0.5f, 0f);
            confirmRect.offsetMin = new Vector2(12f, 28f);
            confirmRect.offsetMax = new Vector2(-34f, 92f);
        }

    }
}
