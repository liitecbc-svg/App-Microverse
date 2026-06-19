using System;
using System.Collections.Generic;
using Microverse.Data;
using Microverse.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Microverse.UI
{
    public class MicroverseApp : MonoBehaviour
    {
        private const MicroverseLanguage SourceLanguage = MicroverseLanguage.English;

        private IModelCatalogService catalogService;
        private ITranslationService translationService;
        private UiTextCatalog uiTextCatalog;
        private IReadOnlyList<BiologicalModel> models;
        private readonly HashSet<MicroverseLanguage> translatedLanguages = new HashSet<MicroverseLanguage>();
        private readonly HashSet<MicroverseLanguage> pendingTranslationLanguages = new HashSet<MicroverseLanguage>();
        private MicroverseLanguage language = MicroverseLanguage.Spanish;
        private RectTransform screenRoot;
        private BottomNavigationBar navigationBar;
        private GameObject activeScreen;
        private BiologicalModel selectedModel;
        private string activeTab = "home";

        private void Awake()
        {
            translationService = new AndroidMlKitTranslationService();
            uiTextCatalog = new UiTextCatalog();
            
            BuildCanvas();
            ShowLoadingScreen();

            // Try loading from Supabase first
            catalogService = new SupabaseModelCatalogService();
            catalogService.LoadModels(
                loadedModels => {
                    models = loadedModels;
                    selectedModel = models.Count > 0 ? models[0] : null;
                    ClearScreen();
                    ShowHome();
                },
                error => {
                    Debug.LogWarning("Supabase loading failed, falling back to LocalModelCatalogService. Details: " + error);
                    
                    // Fallback to Local Catalog
                    catalogService = new LocalModelCatalogService();
                    catalogService.LoadModels(
                        localModels => {
                            models = localModels;
                            selectedModel = models.Count > 0 ? models[0] : null;
                            ClearScreen();
                            ShowHome();
                        },
                        localError => {
                            Debug.LogError("Critical: Local catalog also failed to load. " + localError);
                        }
                    );
                }
            );
        }

        private void ShowLoadingScreen()
        {
            ClearScreen();
            
            GameObject loadingGo = new GameObject("LoadingScreen", typeof(RectTransform));
            loadingGo.transform.SetParent(screenRoot, false);
            UiFactory.Stretch(loadingGo.GetComponent<RectTransform>());
            activeScreen = loadingGo;

            GameObject card = UiFactory.Panel("LoadingCard", loadingGo.transform, MicroverseTheme.Panel, 28);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.12f, 0.38f);
            cardRect.anchorMax = new Vector2(0.88f, 0.62f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            TextMeshProUGUI loadingText = UiFactory.Text("LoadingText", card.transform, "Cargando catálogo...\nLoading catalog...", 26, FontStyles.Bold, MicroverseTheme.Text, TextAlignmentOptions.Center);
            RectTransform textRect = loadingText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private void OnDestroy()
        {
            IDisposable disposable = translationService as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private void BuildCanvas()
        {
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = MicroverseTheme.Background;
            }

            GameObject canvasGo = new GameObject("MicroverseCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.55f;

            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
            UiFactory.Stretch(canvasRect);

            Image background = UiFactory.Image("Background", canvasGo.transform, BiologyVisualFactory.CreateBackground(), Color.white);
            UiFactory.Stretch(background.rectTransform);
            background.type = Image.Type.Simple;
            background.preserveAspect = false;

            GameObject safeFrame = UiFactory.Panel("AppFrame", canvasGo.transform, new Color(0.01f, 0.03f, 0.08f, 0.62f), 30);
            RectTransform frameRect = safeFrame.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = new Vector2(24f, 26f);
            frameRect.offsetMax = new Vector2(-24f, -26f);

            GameObject screenRootGo = new GameObject("ScreenRoot", typeof(RectTransform));
            screenRootGo.transform.SetParent(canvasGo.transform, false);
            screenRoot = screenRootGo.GetComponent<RectTransform>();
            UiFactory.Stretch(screenRoot);

            navigationBar = new BottomNavigationBar(canvasGo.transform, HandleNavigation, GetUiText);
            navigationBar.SetSelected("home");
        }

        private void HandleNavigation(string tab)
        {
            activeTab = tab;
            if (tab == "home")
            {
                ShowHome();
                return;
            }

            if (tab == "categories")
            {
                ShowCategories();
                return;
            }

            if (tab == "scan" && selectedModel != null)
            {
                ShowDetail(selectedModel);
                return;
            }

            ShowPlaceholder(tab);
        }

        private void ShowHome()
        {
            activeTab = "home";
            ClearScreen();
            HomeScreenView home = new HomeScreenView(screenRoot, models, catalogService.GetCategories(), language, ShowDetail, CycleLanguage, GetUiText);
            activeScreen = home.Root;
            navigationBar.RefreshLabels();
            navigationBar.SetSelected("home");
        }

        private void ShowCategories()
        {
            activeTab = "categories";
            ClearScreen();
            CategoriesScreenView categoriesView = new CategoriesScreenView(screenRoot, models, catalogService.GetCategories(), language, ShowDetail, GetUiText);
            activeScreen = categoriesView.Root;
            navigationBar.RefreshLabels();
            navigationBar.SetSelected("categories");
        }

        private void ShowDetail(BiologicalModel model)
        {
            selectedModel = model;
            activeTab = "scan";
            ClearScreen();
            DetailScreenView detail = new DetailScreenView(screenRoot, models, model, language, ShowHome, GetUiText);
            activeScreen = detail.Root;
            navigationBar.RefreshLabels();
            navigationBar.SetSelected("scan");
        }

        private void ShowPlaceholder(string tab)
        {
            ClearScreen();
            GameObject root = new GameObject("Placeholder-" + tab, typeof(RectTransform));
            root.transform.SetParent(screenRoot, false);
            UiFactory.Stretch(root.GetComponent<RectTransform>());
            activeScreen = root;

            string title = PlaceholderTitle(tab);
            string body = PlaceholderBody(tab);

            GameObject card = UiFactory.Panel("Content", root.transform, MicroverseTheme.Panel, 28);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.08f, 0.28f);
            cardRect.anchorMax = new Vector2(0.92f, 0.72f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            TextMeshProUGUI titleText = UiFactory.Text("Title", card.transform, title, 42, FontStyles.Bold, MicroverseTheme.Text, TextAlignmentOptions.Center);
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(44f, -148f);
            titleRect.offsetMax = new Vector2(-44f, -54f);

            TextMeshProUGUI bodyText = UiFactory.Text("Body", card.transform, body, 25, FontStyles.Normal, MicroverseTheme.MutedText, TextAlignmentOptions.Center);
            RectTransform bodyRect = bodyText.rectTransform;
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.offsetMin = new Vector2(56f, 86f);
            bodyRect.offsetMax = new Vector2(-56f, -170f);

            navigationBar.RefreshLabels();
        }

        private void CycleLanguage()
        {
            switch (language)
            {
                case MicroverseLanguage.Spanish:
                    language = MicroverseLanguage.English;
                    break;
                case MicroverseLanguage.English:
                    language = MicroverseLanguage.Portuguese;
                    break;
                default:
                    language = MicroverseLanguage.Spanish;
                    break;
            }

            if (activeTab == "scan" && selectedModel != null)
            {
                ShowDetail(selectedModel);
            }
            else if (activeTab == "home")
            {
                ShowHome();
            }
            else
            {
                ShowPlaceholder(activeTab);
            }

            TranslateLanguageIfNeeded(language);
        }

        private void TranslateLanguageIfNeeded(MicroverseLanguage targetLanguage)
        {
            if (targetLanguage == SourceLanguage ||
                translatedLanguages.Contains(targetLanguage) ||
                pendingTranslationLanguages.Contains(targetLanguage))
            {
                return;
            }

            if (translationService == null || !translationService.IsAutomaticTranslationAvailable)
            {
                return;
            }

            pendingTranslationLanguages.Add(targetLanguage);
            List<TranslationRequest> requests = new List<TranslationRequest>();
            List<Action<string>> applyTranslatedText = new List<Action<string>>();
            string source = SourceLanguage.ToLanguageCode();
            string target = targetLanguage.ToLanguageCode();

            foreach (string key in uiTextCatalog.Keys)
            {
                string uiText = uiTextCatalog.GetSource(key);
                requests.Add(new TranslationRequest(uiText, source, target));
                applyTranslatedText.Add(translated => uiTextCatalog.Set(targetLanguage, key, translated));
            }

            foreach (BiologicalModel model in models)
            {
                AddTranslationRequest(model.Name, targetLanguage, source, target, requests, applyTranslatedText);
                AddTranslationRequest(model.Subtitle, targetLanguage, source, target, requests, applyTranslatedText);
                AddTranslationRequest(model.Category, targetLanguage, source, target, requests, applyTranslatedText);
                AddTranslationRequest(model.Description, targetLanguage, source, target, requests, applyTranslatedText);
            }

            translationService.TranslateBatch(
                requests,
                translations =>
                {
                    for (int i = 0; i < translations.Count; i++)
                    {
                        applyTranslatedText[i](translations[i]);
                    }

                    pendingTranslationLanguages.Remove(targetLanguage);
                    translatedLanguages.Add(targetLanguage);
                    if (language == targetLanguage)
                    {
                        RefreshCurrentScreen();
                    }
                },
                error =>
                {
                    pendingTranslationLanguages.Remove(targetLanguage);
                    Debug.LogWarning("Automatic translation unavailable. Keeping local text. " + error);
                });
        }

        private void AddTranslationRequest(
            LocalizedText text,
            MicroverseLanguage targetLanguage,
            string source,
            string target,
            List<TranslationRequest> requests,
            List<Action<string>> applyTranslatedText)
        {
            string sourceText = text.GetSource(SourceLanguage);
            requests.Add(new TranslationRequest(sourceText, source, target));
            applyTranslatedText.Add(translated => text.Set(targetLanguage, translated));
        }

        private void RefreshCurrentScreen()
        {
            navigationBar.RefreshLabels();

            if (activeTab == "scan" && selectedModel != null)
            {
                ShowDetail(selectedModel);
            }
            else if (activeTab == "home")
            {
                ShowHome();
            }
            else
            {
                ShowPlaceholder(activeTab);
            }
        }

        private string GetUiText(string key)
        {
            return uiTextCatalog.Get(key, language);
        }

        private void ClearScreen()
        {
            if (activeScreen != null)
            {
                Destroy(activeScreen);
            }
        }

        private string PlaceholderTitle(string tab)
        {
            if (tab == "categories")
            {
                return GetUiText("placeholder.categories.title");
            }

            if (tab == "learn")
            {
                return GetUiText("placeholder.learn.title");
            }

            if (tab == "profile")
            {
                return GetUiText("placeholder.profile.title");
            }

            return GetUiText("placeholder.ar.title");
        }

        private string PlaceholderBody(string tab)
        {
            if (tab == "categories")
            {
                return GetUiText("placeholder.categories.body");
            }

            if (tab == "learn")
            {
                return GetUiText("placeholder.learn.body");
            }

            if (tab == "profile")
            {
                return GetUiText("placeholder.profile.body");
            }

            return GetUiText("placeholder.ar.body");
        }
    }
}
