using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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
        private GameObject mainCanvasGo;
        private BottomNavigationBar navigationBar;
        private GameObject activeScreen;
        private BiologicalModel selectedModel;
        private string activeTab = "home";

        // AR state variables
        private GameObject arBackgroundCanvas;
        private GameObject arOverlayCanvas;
        private GameObject arModelInstance;
        private GameObject arLight;
        private Color originalCameraBgColor;
        private CameraClearFlags originalCameraClearFlags;

        private void Awake()
        {
            translationService = new AndroidMlKitTranslationService();
            uiTextCatalog = new UiTextCatalog();
            
            BuildCanvas();
            ShowLoadingScreen();

            catalogService = new CompositeModelCatalogService(new LocalModelCatalogService(), new SupabaseModelCatalogService());
            catalogService.LoadModels(
                loadedModels => {
                    models = loadedModels;
                    selectedModel = models.Count > 0 ? models[0] : null;
                    StartCoroutine(TransitionToStartScreenWithDelay(1.5f));
                },
                error => {
                    Debug.LogError("Critical: Model catalog failed to load. " + error);
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

            // 1. Background
            Image bg = UiFactory.Image("Background", loadingGo.transform, BiologyVisualFactory.CreateBackground(), Color.white);
            UiFactory.Stretch(bg.rectTransform);
            bg.type = Image.Type.Simple;

            // 2. Loading Panel
            GameObject card = UiFactory.Panel("LoadingCard", loadingGo.transform, new Color(0.02f, 0.06f, 0.14f, 0.90f), 28);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.1f, 0.25f);
            cardRect.anchorMax = new Vector2(0.9f, 0.75f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            // 3. App Logo
            Sprite logoSprite = Resources.Load<Sprite>("AppLogo/microverse-logo-temp");
            if (logoSprite != null)
            {
                Image logo = UiFactory.Image("AppLogo", card.transform, logoSprite, Color.white);
                RectTransform logoRect = logo.rectTransform;
                logoRect.anchorMin = new Vector2(0.5f, 0.6f);
                logoRect.anchorMax = new Vector2(0.5f, 0.6f);
                logoRect.pivot = new Vector2(0.5f, 0.5f);
                logoRect.anchoredPosition = new Vector2(0f, 40f);
                logoRect.sizeDelta = new Vector2(300f, 300f);
            }

            // 4. Loading Text
            TextMeshProUGUI loadingText = UiFactory.Text("LoadingText", card.transform, "MICROVERSE\nCargando catálogo...", 26, FontStyles.Bold, MicroverseTheme.Cyan, TextAlignmentOptions.Center);
            RectTransform textRect = loadingText.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0.15f);
            textRect.anchorMax = new Vector2(1f, 0.35f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(0f, 80f);
        }

        private void OnDestroy()
        {
            IDisposable disposable = translationService as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private IEnumerator TransitionToStartScreenWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ClearScreen();
            ShowStartScreen();
        }

        private void ShowStartScreen()
        {
            activeTab = "start";
            ClearScreen();
            
            if (navigationBar != null && navigationBar.Root != null)
            {
                navigationBar.Root.SetActive(false);
            }

            GameObject startRoot = new GameObject("StartScreen", typeof(RectTransform));
            startRoot.transform.SetParent(screenRoot, false);
            UiFactory.Stretch(startRoot.GetComponent<RectTransform>());
            activeScreen = startRoot;

            // 1. Add Background
            Image bg = UiFactory.Image("Background", startRoot.transform, BiologyVisualFactory.CreateBackground(), Color.white);
            UiFactory.Stretch(bg.rectTransform);
            bg.type = Image.Type.Simple;

            // 2. Add App Logo (microverse-logo-temp)
            Sprite logoSprite = Resources.Load<Sprite>("AppLogo/microverse-logo-temp");
            if (logoSprite != null)
            {
                Image logo = UiFactory.Image("AppLogo", startRoot.transform, logoSprite, Color.white);
                RectTransform logoRect = logo.rectTransform;
                logoRect.anchorMin = new Vector2(0.5f, 0.6f);
                logoRect.anchorMax = new Vector2(0.5f, 0.6f);
                logoRect.pivot = new Vector2(0.5f, 0.5f);
                logoRect.anchoredPosition = new Vector2(0f, 0f);
                logoRect.sizeDelta = new Vector2(400f, 400f);
            }

            // 3. Add App Name
            TextMeshProUGUI appNameText = UiFactory.Text("AppName", startRoot.transform, "MICROVERSE", 54, FontStyles.Bold, MicroverseTheme.Cyan, TextAlignmentOptions.Center);
            RectTransform appNameRect = appNameText.rectTransform;
            appNameRect.anchorMin = new Vector2(0f, 0.42f);
            appNameRect.anchorMax = new Vector2(1f, 0.42f);
            appNameRect.pivot = new Vector2(0.5f, 0.5f);
            appNameRect.anchoredPosition = Vector2.zero;
            appNameRect.sizeDelta = new Vector2(0f, 80f);

            // 4. Center-Bottom Buttons (Comenzar & Instrucciones)
            // Left button: Instrucciones
            string instrLabel = language == MicroverseLanguage.Spanish ? "Instrucciones" :
                                (language == MicroverseLanguage.Portuguese ? "Instruções" : "Instructions");
            Button instrButton = UiFactory.Button("InstructionsButton", startRoot.transform, instrLabel, ShowInstructionsOverlay, MicroverseTheme.PanelLight, MicroverseTheme.Text, 24);
            RectTransform instrRect = instrButton.GetComponent<RectTransform>();
            instrRect.anchorMin = new Vector2(0.5f, 0.18f);
            instrRect.anchorMax = new Vector2(0.5f, 0.18f);
            instrRect.pivot = new Vector2(0.5f, 0.5f);
            instrRect.anchoredPosition = new Vector2(-190f, 0f);
            instrRect.sizeDelta = new Vector2(340f, 75f);

            // Right button: Comenzar
            string startLabel = language == MicroverseLanguage.Spanish ? "Comenzar" :
                                (language == MicroverseLanguage.Portuguese ? "Começar" : "Start");
            Button startBtn = UiFactory.Button("StartButton", startRoot.transform, startLabel, ShowHome, new Color(0.0f, 0.42f, 0.68f, 0.95f), MicroverseTheme.Text, 24);
            RectTransform startBtnRect = startBtn.GetComponent<RectTransform>();
            startBtnRect.anchorMin = new Vector2(0.5f, 0.18f);
            startBtnRect.anchorMax = new Vector2(0.5f, 0.18f);
            startBtnRect.pivot = new Vector2(0.5f, 0.5f);
            startBtnRect.anchoredPosition = new Vector2(190f, 0f);
            startBtnRect.sizeDelta = new Vector2(340f, 75f);

            // 5. Menu Button (Top-Right) to open side panel
            Button menuButton = UiFactory.Button("MenuButton", startRoot.transform, "☰", ShowSideMenu, new Color(0.05f, 0.12f, 0.28f, 0.85f), MicroverseTheme.Cyan, 30);
            RectTransform menuRect = menuButton.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(1f, 1f);
            menuRect.anchorMax = new Vector2(1f, 1f);
            menuRect.pivot = new Vector2(1f, 1f);
            menuRect.anchoredPosition = new Vector2(-38f, -38f);
            menuRect.sizeDelta = new Vector2(75f, 75f);
        }

        private void ShowInstructionsOverlay()
        {
            GameObject overlay = UiFactory.Panel("InstructionsOverlay", activeScreen.transform, new Color(0.01f, 0.03f, 0.08f, 0.96f), 0);
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            UiFactory.Stretch(overlayRect);
            
            CanvasGroup cg = overlay.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;

            // Title
            string instrTitle = language == MicroverseLanguage.Spanish ? "Cómo funciona la app" :
                                (language == MicroverseLanguage.Portuguese ? "Como funciona o app" : "How the app works");
            TextMeshProUGUI titleText = UiFactory.Text("OverlayTitle", overlay.transform, instrTitle, 40, FontStyles.Bold, MicroverseTheme.Cyan, TextAlignmentOptions.Center);
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(56f, -140f);
            titleRect.offsetMax = new Vector2(-56f, -50f);

            string[] descList = language == MicroverseLanguage.Spanish ? new string[] {
                "1. Explora y selecciona microorganismos de nuestro catálogo científico.",
                "2. Descarga el modelo 3D para tener acceso sin conexión a internet.",
                "3. ¡Míralo en 3D interactivo o proyéctalo en tu entorno usando AR!"
            } : (language == MicroverseLanguage.Portuguese ? new string[] {
                "1. Explore e selecione microorganismos do nosso catálogo científico.",
                "2. Baixe o modelo 3D para ter acesso off-line.",
                "3. Veja em 3D interativo ou projete no seu ambiente usando AR!"
            } : new string[] {
                "1. Explore and select microorganisms from our scientific catalog.",
                "2. Download the 3D model for offline access.",
                "3. View it in interactive 3D or project it in your environment using AR!"
            });

            string[] imagePaths = new string[] {
                "Instructions/step1_explore",
                "Instructions/step2_download",
                "Instructions/step3_view3d"
            };

            for (int i = 0; i < 3; i++)
            {
                GameObject stepPanel = new GameObject($"Step_{i + 1}", typeof(RectTransform));
                stepPanel.transform.SetParent(overlay.transform, false);
                RectTransform stepRect = stepPanel.GetComponent<RectTransform>();
                
                float minAnchorY = 0.68f - (i * 0.25f);
                float maxAnchorY = 0.90f - (i * 0.25f);
                stepRect.anchorMin = new Vector2(0.08f, minAnchorY);
                stepRect.anchorMax = new Vector2(0.92f, maxAnchorY);
                stepRect.offsetMin = Vector2.zero;
                stepRect.offsetMax = Vector2.zero;

                // Load Step Image
                Sprite stepSprite = Resources.Load<Sprite>(imagePaths[i]);
                if (stepSprite != null)
                {
                    Image stepImg = UiFactory.Image("Image", stepPanel.transform, stepSprite, Color.white);
                    RectTransform imgRect = stepImg.rectTransform;
                    imgRect.anchorMin = new Vector2(0f, 0.5f);
                    imgRect.anchorMax = new Vector2(0f, 0.5f);
                    imgRect.pivot = new Vector2(0f, 0.5f);
                    imgRect.anchoredPosition = new Vector2(10f, 0f);
                    imgRect.sizeDelta = new Vector2(200f, 200f);
                }

                // Add Step Text (next to the image)
                TextMeshProUGUI stepText = UiFactory.Text("Text", stepPanel.transform, descList[i], 22, FontStyles.Normal, MicroverseTheme.Text, TextAlignmentOptions.Left);
                RectTransform textRect = stepText.rectTransform;
                textRect.anchorMin = new Vector2(0f, 0f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.offsetMin = new Vector2(230f, 10f);
                textRect.offsetMax = new Vector2(-10f, -10f);
                
                stepText.enableAutoSizing = true;
                stepText.fontSizeMax = 22;
                stepText.fontSizeMin = 14;
            }

            // Close Button
            string closeLabel = language == MicroverseLanguage.Spanish ? "Entendido" :
                                (language == MicroverseLanguage.Portuguese ? "Entendi" : "Got it");
            Button closeBtn = UiFactory.Button("GotItButton", overlay.transform, closeLabel, () => {
                UnityEngine.Object.Destroy(overlay);
            }, new Color(0.0f, 0.42f, 0.68f, 0.95f), MicroverseTheme.Text, 24);
            RectTransform closeRect = closeBtn.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.5f, 0.05f);
            closeRect.anchorMax = new Vector2(0.5f, 0.05f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = Vector2.zero;
            closeRect.sizeDelta = new Vector2(380f, 75f);
        }

        private void ShowSideMenu()
        {
            GameObject overlay = UiFactory.Panel("SideMenuOverlay", activeScreen.transform, new Color(0f, 0f, 0f, 0.5f), 0);
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            UiFactory.Stretch(overlayRect);
            
            CanvasGroup cg = overlay.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;

            Button closeArea = overlay.AddComponent<Button>();
            closeArea.onClick.AddListener(() => UnityEngine.Object.Destroy(overlay));

            GameObject sidePanel = UiFactory.Panel("SidePanel", overlay.transform, new Color(0.02f, 0.06f, 0.14f, 0.98f), 0);
            RectTransform sideRect = sidePanel.GetComponent<RectTransform>();
            sideRect.anchorMin = new Vector2(0.5f, 0f);
            sideRect.anchorMax = new Vector2(1f, 1f);
            sideRect.pivot = new Vector2(1f, 0.5f);
            sideRect.offsetMin = Vector2.zero;
            sideRect.offsetMax = Vector2.zero;

            CanvasGroup panelCg = sidePanel.AddComponent<CanvasGroup>();
            panelCg.blocksRaycasts = true;

            string menuTitle = language == MicroverseLanguage.Spanish ? "Menú" :
                               (language == MicroverseLanguage.Portuguese ? "Menu" : "Menu");
            TextMeshProUGUI title = UiFactory.Text("MenuTitle", sidePanel.transform, menuTitle, 32, FontStyles.Bold, MicroverseTheme.Cyan, TextAlignmentOptions.Center);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(20f, -120f);
            titleRect.offsetMax = new Vector2(-20f, -40f);

            string langLabel = language == MicroverseLanguage.Spanish ? "Idioma" :
                               (language == MicroverseLanguage.Portuguese ? "Idioma" : "Language");
            Button langBtn = UiFactory.Button("LanguageBtn", sidePanel.transform, langLabel, () => ShowLanguageSubPanel(overlay.transform, sidePanel.transform), MicroverseTheme.PanelLight, MicroverseTheme.Text, 22);
            RectTransform langRect = langBtn.GetComponent<RectTransform>();
            langRect.anchorMin = new Vector2(0f, 0.70f);
            langRect.anchorMax = new Vector2(1f, 0.70f);
            langRect.pivot = new Vector2(0.5f, 0.5f);
            langRect.anchoredPosition = Vector2.zero;
            langRect.sizeDelta = new Vector2(400f, 75f);

            string aboutLabel = language == MicroverseLanguage.Spanish ? "Acerca de" :
                                (language == MicroverseLanguage.Portuguese ? "Sobre" : "About");
            Button aboutBtn = UiFactory.Button("AboutBtn", sidePanel.transform, aboutLabel, () => ShowCreditsOverlay(overlay.transform), MicroverseTheme.PanelLight, MicroverseTheme.Text, 22);
            RectTransform aboutRect = aboutBtn.GetComponent<RectTransform>();
            aboutRect.anchorMin = new Vector2(0f, 0.55f);
            aboutRect.anchorMax = new Vector2(1f, 0.55f);
            aboutRect.pivot = new Vector2(0.5f, 0.5f);
            aboutRect.anchoredPosition = Vector2.zero;
            aboutRect.sizeDelta = new Vector2(400f, 75f);

            string closeText = language == MicroverseLanguage.Spanish ? "Cerrar" :
                               (language == MicroverseLanguage.Portuguese ? "Fechar" : "Close");
            Button closeBtn = UiFactory.Button("CloseMenuBtn", sidePanel.transform, closeText, () => UnityEngine.Object.Destroy(overlay), new Color(0.0f, 0.42f, 0.68f, 0.95f), MicroverseTheme.Text, 22);
            RectTransform closeRect = closeBtn.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0f, 0.15f);
            closeRect.anchorMax = new Vector2(1f, 0.15f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = Vector2.zero;
            closeRect.sizeDelta = new Vector2(400f, 75f);
        }

        private void ShowLanguageSubPanel(Transform overlayTransform, Transform sidePanelTransform)
        {
            Transform existingLang = overlayTransform.Find("LanguageSubPanel");
            if (existingLang != null)
            {
                UnityEngine.Object.Destroy(existingLang.gameObject);
            }
            Transform existingCred = overlayTransform.Find("CreditsSubPanel");
            if (existingCred != null)
            {
                UnityEngine.Object.Destroy(existingCred.gameObject);
            }

            GameObject subPanel = UiFactory.Panel("LanguageSubPanel", overlayTransform, new Color(0.03f, 0.10f, 0.22f, 0.99f), 0);
            RectTransform subRect = subPanel.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0f, 0f);
            subRect.anchorMax = new Vector2(0.5f, 1f);
            subRect.pivot = new Vector2(0f, 0.5f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;

            CanvasGroup subCg = subPanel.AddComponent<CanvasGroup>();
            subCg.blocksRaycasts = true;

            string langTitle = language == MicroverseLanguage.Spanish ? "Seleccionar Idioma" :
                               (language == MicroverseLanguage.Portuguese ? "Selecionar Idioma" : "Select Language");
            TextMeshProUGUI title = UiFactory.Text("SubTitle", subPanel.transform, langTitle, 28, FontStyles.Bold, MicroverseTheme.Cyan, TextAlignmentOptions.Center);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(10f, -120f);
            titleRect.offsetMax = new Vector2(-10f, -40f);

            Button esBtn = UiFactory.Button("EsBtn", subPanel.transform, "Español", () => SetAppLanguage(MicroverseLanguage.Spanish, overlayTransform), MicroverseTheme.PanelLight, language == MicroverseLanguage.Spanish ? MicroverseTheme.Cyan : MicroverseTheme.Text, 22);
            RectTransform esRect = esBtn.GetComponent<RectTransform>();
            esRect.anchorMin = new Vector2(0f, 0.70f);
            esRect.anchorMax = new Vector2(1f, 0.70f);
            esRect.pivot = new Vector2(0.5f, 0.5f);
            esRect.anchoredPosition = Vector2.zero;
            esRect.sizeDelta = new Vector2(400f, 75f);

            Button enBtn = UiFactory.Button("EnBtn", subPanel.transform, "English", () => SetAppLanguage(MicroverseLanguage.English, overlayTransform), MicroverseTheme.PanelLight, language == MicroverseLanguage.English ? MicroverseTheme.Cyan : MicroverseTheme.Text, 22);
            RectTransform enRect = enBtn.GetComponent<RectTransform>();
            enRect.anchorMin = new Vector2(0f, 0.55f);
            enRect.anchorMax = new Vector2(1f, 0.55f);
            enRect.pivot = new Vector2(0.5f, 0.5f);
            enRect.anchoredPosition = Vector2.zero;
            enRect.sizeDelta = new Vector2(400f, 75f);

            Button ptBtn = UiFactory.Button("PtBtn", subPanel.transform, "Português", () => SetAppLanguage(MicroverseLanguage.Portuguese, overlayTransform), MicroverseTheme.PanelLight, language == MicroverseLanguage.Portuguese ? MicroverseTheme.Cyan : MicroverseTheme.Text, 22);
            RectTransform ptRect = ptBtn.GetComponent<RectTransform>();
            ptRect.anchorMin = new Vector2(0f, 0.40f);
            ptRect.anchorMax = new Vector2(1f, 0.40f);
            ptRect.pivot = new Vector2(0.5f, 0.5f);
            ptRect.anchoredPosition = Vector2.zero;
            ptRect.sizeDelta = new Vector2(400f, 75f);
        }

        private void SetAppLanguage(MicroverseLanguage lang, Transform overlayTransform)
        {
            language = lang;
            if (overlayTransform != null)
            {
                UnityEngine.Object.Destroy(overlayTransform.gameObject);
            }
            ShowStartScreen();
        }

        private void ShowCreditsOverlay(Transform overlayTransform)
        {
            Transform existingLang = overlayTransform.Find("LanguageSubPanel");
            if (existingLang != null)
            {
                UnityEngine.Object.Destroy(existingLang.gameObject);
            }
            Transform existingCred = overlayTransform.Find("CreditsSubPanel");
            if (existingCred != null)
            {
                UnityEngine.Object.Destroy(existingCred.gameObject);
            }

            GameObject subPanel = UiFactory.Panel("CreditsSubPanel", overlayTransform, new Color(0.03f, 0.10f, 0.22f, 0.99f), 0);
            RectTransform subRect = subPanel.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0f, 0f);
            subRect.anchorMax = new Vector2(0.5f, 1f);
            subRect.pivot = new Vector2(0f, 0.5f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;

            CanvasGroup subCg = subPanel.AddComponent<CanvasGroup>();
            subCg.blocksRaycasts = true;

            string aboutTitle = language == MicroverseLanguage.Spanish ? "Acerca de" :
                                (language == MicroverseLanguage.Portuguese ? "Sobre" : "About");
            TextMeshProUGUI title = UiFactory.Text("CreditsTitle", subPanel.transform, aboutTitle, 32, FontStyles.Bold, MicroverseTheme.Cyan, TextAlignmentOptions.Center);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(10f, -120f);
            titleRect.offsetMax = new Vector2(-10f, -40f);

            string creditsStr = language == MicroverseLanguage.Spanish ?
                "MICROVERSE\n\nDesarrollo:\nCristhian Montenegro\nBrandon Muñoz\n\nColaboración Académica:\nDra. Cassia Yano\n\nInstituciones:\nULS | LIITEC" :
                (language == MicroverseLanguage.Portuguese ?
                "MICROVERSE\n\nDesenvolvimento:\nCristhian Montenegro\nBrandon Muñoz\n\nColaboração Acadêmica:\nDra. Cassia Yano\n\nInstituições:\nULS | LIITEC" :
                "MICROVERSE\n\nDevelopment:\nCristhian Montenegro\nBrandon Muñoz\n\nAcademic Collaboration:\nDr. Cassia Yano\n\nInstitutions:\nULS | LIITEC");

            TextMeshProUGUI bodyText = UiFactory.Text("CreditsBody", subPanel.transform, creditsStr, 22, FontStyles.Normal, MicroverseTheme.Text, TextAlignmentOptions.Center);
            RectTransform bodyRect = bodyText.rectTransform;
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.offsetMin = new Vector2(24f, 40f);
            bodyRect.offsetMax = new Vector2(-24f, -140f);
            bodyText.enableAutoSizing = true;
            bodyText.fontSizeMax = 22;
            bodyText.fontSizeMin = 14;
        }

        private void BuildCanvas()
        {
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = MicroverseTheme.Background;
            }

            mainCanvasGo = new GameObject("MicroverseCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            mainCanvasGo.transform.SetParent(transform, false);

            Canvas canvas = mainCanvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = mainCanvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.55f;

            RectTransform canvasRect = mainCanvasGo.GetComponent<RectTransform>();
            UiFactory.Stretch(canvasRect);

            Image background = UiFactory.Image("Background", mainCanvasGo.transform, BiologyVisualFactory.CreateBackground(), Color.white);
            UiFactory.Stretch(background.rectTransform);
            background.type = Image.Type.Simple;
            background.preserveAspect = false;

            GameObject safeFrame = UiFactory.Panel("AppFrame", mainCanvasGo.transform, new Color(0.01f, 0.03f, 0.08f, 0.62f), 30);
            RectTransform frameRect = safeFrame.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = new Vector2(24f, 26f);
            frameRect.offsetMax = new Vector2(-24f, -26f);

            GameObject screenRootGo = new GameObject("ScreenRoot", typeof(RectTransform));
            screenRootGo.transform.SetParent(mainCanvasGo.transform, false);
            screenRoot = screenRootGo.GetComponent<RectTransform>();
            UiFactory.Stretch(screenRoot);

            navigationBar = new BottomNavigationBar(mainCanvasGo.transform, HandleNavigation, GetUiText);
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

            if (tab == "scan")
            {
                return;
            }

            if (tab == "profile")
            {
                ShowCredits();
                return;
            }

            ShowPlaceholder(tab);
        }

        private void ShowHome()
        {
            activeTab = "home";
            ClearScreen();
            HomeScreenView home = new HomeScreenView(screenRoot, models, catalogService.GetCategories(), language, HandleModelSelected, CycleLanguage, GetUiText);
            activeScreen = home.Root;
            if (navigationBar != null && navigationBar.Root != null)
            {
                navigationBar.Root.SetActive(true);
                navigationBar.RefreshLabels();
                navigationBar.SetSelected("home");
            }
        }

        private void ShowCategories()
        {
            activeTab = "categories";
            ClearScreen();
            CategoriesScreenView categoriesView = new CategoriesScreenView(screenRoot, models, catalogService.GetCategories(), language, HandleModelSelected, GetUiText);
            activeScreen = categoriesView.Root;
            if (navigationBar != null && navigationBar.Root != null)
            {
                navigationBar.Root.SetActive(true);
                navigationBar.RefreshLabels();
                navigationBar.SetSelected("categories");
            }
        }

        private void ShowDetail(BiologicalModel model)
        {
            selectedModel = model;
            activeTab = "scan";
            ClearScreen();
            DetailScreenView detail = new DetailScreenView(screenRoot, models, model, language, ShowHome, EnterARMode, GetUiText);
            activeScreen = detail.Root;
            if (navigationBar != null && navigationBar.Root != null)
            {
                navigationBar.Root.SetActive(true);
                navigationBar.RefreshLabels();
                navigationBar.SetSelected("scan");
            }
        }

        private void HandleModelSelected(BiologicalModel model)
        {
            if (model == null)
            {
                return;
            }

            selectedModel = model;
            EnterARMode(model);
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

        private void ShowCredits()
        {
            activeTab = "profile";
            ClearScreen();
            GameObject root = new GameObject("CreditsScreen", typeof(RectTransform));
            root.transform.SetParent(screenRoot, false);
            UiFactory.Stretch(root.GetComponent<RectTransform>());
            activeScreen = root;

            TextMeshProUGUI title = UiFactory.Text("CreditsTitle", root.transform, GetUiText("placeholder.profile.title"), 44, FontStyles.Bold, MicroverseTheme.Text, TextAlignmentOptions.Center);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(56f, -185f);
            titleRect.offsetMax = new Vector2(-56f, -105f);

            AddCreditSection(root.transform, "DevelopersPanel", DevelopersLabel(), "Cristhian Montenegro\nBrandon Muñoz", new Vector2(0.08f, 0.61f), new Vector2(0.92f, 0.78f), MicroverseTheme.Cyan);
            AddCreditSection(root.transform, "AcademicPanel", AcademicCollaborationLabel(), "Dra. Cassia Yano", new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.57f), new Color(0.54f, 0.82f, 1f));
            AddCreditSection(root.transform, "InstitutionsPanel", InstitutionsLabel(), "ULS\nLIITEC", new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.38f), new Color(0.90f, 0.74f, 0.22f));

            if (navigationBar != null && navigationBar.Root != null)
            {
                navigationBar.Root.SetActive(true);
                navigationBar.RefreshLabels();
                navigationBar.SetSelected("profile");
            }
        }

        private void AddCreditSection(Transform parent, string name, string heading, string content, Vector2 anchorMin, Vector2 anchorMax, Color accent)
        {
            GameObject panel = UiFactory.Panel(name, parent, new Color(0.02f, 0.06f, 0.14f, 0.96f), 24);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = anchorMin;
            panelRect.anchorMax = anchorMax;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            GameObject accentBar = UiFactory.Panel("Accent", panel.transform, accent, 8);
            RectTransform accentRect = accentBar.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.offsetMin = new Vector2(0f, 0f);
            accentRect.offsetMax = new Vector2(10f, 0f);

            TextMeshProUGUI label = UiFactory.Text("Heading", panel.transform, heading, 23, FontStyles.Bold, accent, TextAlignmentOptions.Left);
            label.enableAutoSizing = true;
            label.fontSizeMax = 23;
            label.fontSizeMin = 14;
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(34f, -58f);
            labelRect.offsetMax = new Vector2(-34f, -16f);

            TextMeshProUGUI names = UiFactory.Text("Content", panel.transform, content, 29, FontStyles.Bold, MicroverseTheme.Text, TextAlignmentOptions.Left);
            names.enableAutoSizing = true;
            names.fontSizeMax = 29;
            names.fontSizeMin = 16;
            RectTransform namesRect = names.rectTransform;
            namesRect.anchorMin = new Vector2(0f, 0f);
            namesRect.anchorMax = new Vector2(1f, 1f);
            namesRect.offsetMin = new Vector2(34f, 18f);
            namesRect.offsetMax = new Vector2(-34f, -62f);
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
            else if (activeTab == "profile")
            {
                ShowCredits();
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
            else if (activeTab == "profile")
            {
                ShowCredits();
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

        private string DevelopersLabel()
        {
            if (language == MicroverseLanguage.English)
            {
                return "Developers";
            }

            if (language == MicroverseLanguage.Portuguese)
            {
                return "Desenvolvedores";
            }

            return "Desarrolladores";
        }

        private string AcademicCollaborationLabel()
        {
            if (language == MicroverseLanguage.English)
            {
                return "Academic collaboration";
            }

            if (language == MicroverseLanguage.Portuguese)
            {
                return "Colaboracao academica";
            }

            return "Colaboracion academica";
        }

        private string InstitutionsLabel()
        {
            if (language == MicroverseLanguage.English)
            {
                return "Collaborating institutions";
            }

            if (language == MicroverseLanguage.Portuguese)
            {
                return "Instituicoes colaboradoras";
            }

            return "Instituciones colaboradoras";
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

        private void EnterARMode(BiologicalModel model)
        {
            // 1. Hide the main app UI and navigation bar
            if (screenRoot != null) screenRoot.gameObject.SetActive(false);
            if (navigationBar != null && navigationBar.Root != null) navigationBar.Root.SetActive(false);
            if (mainCanvasGo != null) mainCanvasGo.SetActive(false);

            // 2. Configure Main Camera
            if (Camera.main != null)
            {
                originalCameraBgColor = Camera.main.backgroundColor;
                originalCameraClearFlags = Camera.main.clearFlags;
                
                Camera.main.clearFlags = CameraClearFlags.Color;
                Camera.main.backgroundColor = Color.black;
            }

            // 3. Create AR Background Canvas (for WebCamTexture)
            arBackgroundCanvas = new GameObject("ARBackgroundCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas bgCanvas = arBackgroundCanvas.GetComponent<Canvas>();
            bgCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            bgCanvas.worldCamera = Camera.main;
            bgCanvas.planeDistance = 15f;
            bgCanvas.sortingOrder = -1;

            CanvasScaler bgScaler = arBackgroundCanvas.GetComponent<CanvasScaler>();
            bgScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            bgScaler.referenceResolution = new Vector2(1080f, 1920f);
            bgScaler.matchWidthOrHeight = 0.55f;

            GameObject rawImageGo = new GameObject("CameraFeed", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            rawImageGo.transform.SetParent(arBackgroundCanvas.transform, false);
            RawImage rawImage = rawImageGo.GetComponent<RawImage>();
            UiFactory.Stretch(rawImage.rectTransform);
            
            rawImageGo.AddComponent<WebCamCameraBackground>();

            // 4. Create 3D Cell Model
            arModelInstance = LoadRealOrProcedural3DModel(model);
            if (Camera.main != null)
            {
                arModelInstance.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 4f;
                arModelInstance.transform.LookAt(Camera.main.transform);
                arModelInstance.transform.Rotate(0f, 180f, 0f);
            }
            else
            {
                arModelInstance.transform.position = new Vector3(0f, 0f, 4f);
            }

            arModelInstance.AddComponent<ModelManipulator>();

            // 5. Create Directional Light for 3D Shading
            arLight = new GameObject("ARDirectionalLight", typeof(Light));
            Light light = arLight.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            light.color = new Color(0.95f, 0.98f, 1f);
            arLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            // 6. Create AROverlayCanvas
            arOverlayCanvas = new GameObject("AROverlayCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas overlayCanvas = arOverlayCanvas.GetComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 20;

            CanvasScaler overlayScaler = arOverlayCanvas.GetComponent<CanvasScaler>();
            overlayScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            overlayScaler.referenceResolution = new Vector2(1080f, 1920f);
            overlayScaler.matchWidthOrHeight = 0.55f;

            GameObject safeFrame = UiFactory.Panel("ARFrame", arOverlayCanvas.transform, new Color(0.01f, 0.03f, 0.08f, 0.4f), 30);
            RectTransform frameRect = safeFrame.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = new Vector2(24f, 26f);
            frameRect.offsetMax = new Vector2(-24f, -26f);

            string backLabel = "<";
            Button backButton = UiFactory.Button("ARBackButton", safeFrame.transform, backLabel, ExitARMode, new Color(0.05f, 0.12f, 0.28f, 0.85f), MicroverseTheme.Text, 25);
            RectTransform backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 1f);
            backRect.anchorMax = new Vector2(0f, 1f);
            backRect.pivot = new Vector2(0f, 1f);
            backRect.anchoredPosition = new Vector2(38f, -38f);
            backRect.sizeDelta = new Vector2(75f, 75f);

            string instruction = language == MicroverseLanguage.Spanish ? 
                "Usa 1 dedo para rotar la célula\nUsa 2 dedos para cambiar el tamaño" : 
                (language == MicroverseLanguage.Portuguese ?
                "Use 1 dedo para rotar a célula\nUse 2 dedos para redimensionar" :
                "Use 1 finger to rotate the cell\nUse 2 fingers to resize");
            
            instruction = language == MicroverseLanguage.Spanish ?
                "Usa 1 dedo para rotar el modelo\nUsa 2 dedos para cambiar el tamano" :
                (language == MicroverseLanguage.Portuguese ?
                "Use 1 dedo para rotar o modelo\nUse 2 dedos para redimensionar" :
                "Use 1 finger to rotate the model\nUse 2 fingers to resize");

            GameObject helpPanel = UiFactory.Panel("ARHelpPanel", safeFrame.transform, new Color(0.02f, 0.06f, 0.14f, 0.92f), 20);
            RectTransform helpPanelRect = helpPanel.GetComponent<RectTransform>();
            helpPanelRect.anchorMin = new Vector2(1f, 1f);
            helpPanelRect.anchorMax = new Vector2(1f, 1f);
            helpPanelRect.pivot = new Vector2(1f, 1f);
            helpPanelRect.anchoredPosition = new Vector2(-38f, -126f);
            helpPanelRect.sizeDelta = new Vector2(500f, 130f);

            TextMeshProUGUI instructionText = UiFactory.Text("ARInstructions", helpPanel.transform, instruction, 22, FontStyles.Normal, MicroverseTheme.Text, TextAlignmentOptions.Center);
            RectTransform instructionRect = instructionText.rectTransform;
            instructionText.enableAutoSizing = true;
            instructionText.fontSizeMax = 22;
            instructionText.fontSizeMin = 14;
            UiFactory.Stretch(instructionRect, 24f, 16f);
            helpPanel.SetActive(false);

            Button helpButton = UiFactory.Button("ARHelpButton", safeFrame.transform, "!", () => helpPanel.SetActive(!helpPanel.activeSelf), new Color(0.05f, 0.12f, 0.28f, 0.85f), MicroverseTheme.Cyan, 34);
            RectTransform helpRect = helpButton.GetComponent<RectTransform>();
            helpRect.anchorMin = new Vector2(1f, 1f);
            helpRect.anchorMax = new Vector2(1f, 1f);
            helpRect.pivot = new Vector2(1f, 1f);
            helpRect.anchoredPosition = new Vector2(-38f, -38f);
            helpRect.sizeDelta = new Vector2(75f, 75f);

            GameObject descriptionPanel = UiFactory.Panel("ARDescriptionPanel", safeFrame.transform, new Color(0.02f, 0.06f, 0.14f, 0.86f), 22);
            RectTransform descriptionPanelRect = descriptionPanel.GetComponent<RectTransform>();
            descriptionPanelRect.anchorMin = new Vector2(0f, 0f);
            descriptionPanelRect.anchorMax = new Vector2(1f, 0f);
            descriptionPanelRect.pivot = new Vector2(0.5f, 0f);
            descriptionPanelRect.offsetMin = new Vector2(56f, 42f);
            descriptionPanelRect.offsetMax = new Vector2(-56f, 242f);

            // Add ScrollRect component to enable scrolling
            ScrollRect scroll = descriptionPanel.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            // Create Viewport with RectMask2D for clipping text
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(descriptionPanel.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            UiFactory.Stretch(viewportRect, 28f, 18f);

            // Create Scrollbar for visual scrolling feedback
            GameObject scrollbarGo = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
            scrollbarGo.transform.SetParent(descriptionPanel.transform, false);
            RectTransform scrollbarRect = scrollbarGo.GetComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.pivot = new Vector2(1f, 0.5f);
            scrollbarRect.anchoredPosition = new Vector2(-12f, 0f);
            scrollbarRect.sizeDelta = new Vector2(8f, -24f);

            Image scrollbarBg = scrollbarGo.GetComponent<Image>();
            scrollbarBg.sprite = RoundedSpriteFactory.RoundedRect(new Color(1f, 1f, 1f, 0.05f), 4);
            scrollbarBg.type = Image.Type.Sliced;

            GameObject slidingArea = new GameObject("SlidingArea", typeof(RectTransform));
            slidingArea.transform.SetParent(scrollbarGo.transform, false);
            UiFactory.Stretch(slidingArea.GetComponent<RectTransform>());

            GameObject handleGo = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handleGo.transform.SetParent(slidingArea.transform, false);
            UiFactory.Stretch(handleGo.GetComponent<RectTransform>());

            Image handleImage = handleGo.GetComponent<Image>();
            handleImage.sprite = RoundedSpriteFactory.RoundedRect(MicroverseTheme.Cyan, 4);
            handleImage.type = Image.Type.Sliced;

            Scrollbar scrollbar = scrollbarGo.GetComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.handleRect = handleGo.GetComponent<RectTransform>();

            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scroll.verticalScrollbarSpacing = 6f;

            // Create Description Text inside the Viewport
            TextMeshProUGUI descriptionText = UiFactory.Text("ARDescription", viewport.transform, model.Description.Get(language), 28, FontStyles.Normal, MicroverseTheme.Text, TextAlignmentOptions.Center);
            RectTransform textRect = descriptionText.rectTransform;
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(0f, 0f);

            // Add ContentSizeFitter to dynamically resize text rect based on content length
            ContentSizeFitter fitter = descriptionText.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = textRect;

            TextMeshProUGUI cellLabel = UiFactory.Text("ARCellLabel", safeFrame.transform, model.Name.Get(language), 36, FontStyles.Bold, MicroverseTheme.Cyan, TextAlignmentOptions.Center);
            RectTransform labelRect = cellLabel.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = new Vector2(0f, 254f);
            labelRect.sizeDelta = new Vector2(800f, 64f);
        }

        private void ExitARMode()
        {
            if (arBackgroundCanvas != null)
            {
                Destroy(arBackgroundCanvas);
                arBackgroundCanvas = null;
            }
            if (arOverlayCanvas != null)
            {
                Destroy(arOverlayCanvas);
                arOverlayCanvas = null;
            }
            if (arModelInstance != null)
            {
                Destroy(arModelInstance);
                arModelInstance = null;
            }
            if (arLight != null)
            {
                Destroy(arLight);
                arLight = null;
            }

            if (Camera.main != null)
            {
                Camera.main.backgroundColor = originalCameraBgColor;
                Camera.main.clearFlags = originalCameraClearFlags;
            }

            if (mainCanvasGo != null) mainCanvasGo.SetActive(true);
            if (screenRoot != null) screenRoot.gameObject.SetActive(true);
            if (navigationBar != null && navigationBar.Root != null) navigationBar.Root.SetActive(true);
        }

        private GameObject CreateProcedural3DCell(BiologicalModel model)
        {
            GameObject cellParent = new GameObject("3DCell_" + model.Id);

            GameObject membrane = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            membrane.name = "Membrane";
            membrane.transform.SetParent(cellParent.transform, false);
            
            if (model.IsElongated)
            {
                membrane.transform.localScale = new Vector3(1.7f, 0.9f, 0.9f);
            }
            else
            {
                membrane.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            }

            Color membraneColor = new Color(model.PrimaryColor.r, model.PrimaryColor.g, model.PrimaryColor.b, 0.35f);
            membrane.GetComponent<Renderer>().material = CreateTransparentMaterial(membraneColor);

            GameObject nucleus = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nucleus.name = "Nucleus";
            nucleus.transform.SetParent(cellParent.transform, false);
            nucleus.transform.localPosition = Vector3.zero;
            nucleus.transform.localScale = new Vector3(0.38f, 0.38f, 0.38f);

            Color nucleusColor = Color.Lerp(model.SecondaryColor, new Color(0.48f, 0.05f, 0.52f), 0.55f);
            nucleus.GetComponent<Renderer>().material = CreateSolidMaterial(nucleusColor);

            GameObject nucleolus = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nucleolus.name = "Nucleolus";
            nucleolus.transform.SetParent(nucleus.transform, false);
            nucleolus.transform.localPosition = Vector3.zero;
            nucleolus.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            
            Color nucleolusColor = Color.Lerp(nucleusColor, Color.white, 0.3f);
            nucleolus.GetComponent<Renderer>().material = CreateSolidMaterial(nucleolusColor);

            UnityEngine.Random.InitState(model.VisualSeed);
            int organelleCount = model.IsElongated ? 9 : 14;
            for (int i = 0; i < organelleCount; i++)
            {
                GameObject organelle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                organelle.name = "Organelle_" + i;
                organelle.transform.SetParent(cellParent.transform, false);

                Vector3 localPos = UnityEngine.Random.insideUnitSphere * 0.72f;
                if (model.IsElongated)
                {
                    localPos.x *= 1.5f;
                    localPos.y *= 0.8f;
                    localPos.z *= 0.8f;
                }

                if (localPos.magnitude < 0.28f)
                {
                    localPos = localPos.normalized * 0.35f;
                }

                organelle.transform.localPosition = localPos;
                
                float organelleScale = UnityEngine.Random.Range(0.12f, 0.22f);
                organelle.transform.localScale = new Vector3(organelleScale, organelleScale, organelleScale);

                Color organelleColor = Color.Lerp(model.SecondaryColor, Color.white, UnityEngine.Random.Range(0.05f, 0.45f));
                organelle.GetComponent<Renderer>().material = CreateSolidMaterial(organelleColor);
            }

            return cellParent;
        }

        private GameObject LoadRealOrProcedural3DModel(BiologicalModel model)
        {
            GameObject holder = new GameObject("3DCell_" + model.Id);

            string localPath;
            ModelDownloadStore.TryGetLocalModelPath(model, out localPath);

            if (TryLoadBundledModelInto(holder, model, localPath))
            {
                return holder;
            }

            string gltfSource = FirstRuntimeGltfSource(localPath, model.ModelFileUrl);
            if (!string.IsNullOrWhiteSpace(gltfSource))
            {
                GameObject fallback = CreateProcedural3DCell(model);
                fallback.name = "LoadingFallbackModel";
                fallback.transform.SetParent(holder.transform, false);
                TryReplaceWithGltfModelAsync(holder, gltfSource, model);
                return holder;
            }

            Destroy(holder);
            return CreateProcedural3DCell(model);
        }

        private bool TryLoadBundledModelInto(GameObject holder, BiologicalModel model, string localPath)
        {
            List<string> candidates = BuildModelReferenceCandidates(model, localPath);
            for (int i = 0; i < candidates.Count; i++)
            {
                string resourcePath = ResourcePathFromReference(candidates[i]);
                if (string.IsNullOrWhiteSpace(resourcePath))
                {
                    continue;
                }

                GameObject prefab = Resources.Load<GameObject>(resourcePath);
                if (prefab != null)
                {
                    InstantiateAndNormalizeModel(holder, prefab, "BundledModel");
                    Debug.Log("[AR Model] Loaded bundled model from Resources: " + resourcePath);
                    return true;
                }
            }

#if UNITY_EDITOR
            for (int i = 0; i < candidates.Count; i++)
            {
                string assetName = FileNameWithoutExtensionFromReference(candidates[i]);
                if (string.IsNullOrWhiteSpace(assetName))
                {
                    continue;
                }

                string[] guids = UnityEditor.AssetDatabase.FindAssets(assetName + " t:GameObject", new[] { "Assets/Microverse/Models" });
                for (int j = 0; j < guids.Length; j++)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[j]);
                    GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        InstantiateAndNormalizeModel(holder, prefab, "EditorAssetModel");
                        Debug.Log("[AR Model] Loaded FBX model from Assets: " + path);
                        return true;
                    }
                }
            }
#endif

            return false;
        }

        private List<string> BuildModelReferenceCandidates(BiologicalModel model, string localPath)
        {
            List<string> candidates = new List<string>();
            AddCandidate(candidates, localPath);
            AddCandidate(candidates, model != null ? model.ModelFileUrl : string.Empty);

            string fileName = FileNameFromReference(model != null ? model.ModelFileUrl : string.Empty);
            AddCandidate(candidates, fileName);

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                string withoutExtension = Path.GetFileNameWithoutExtension(fileName);
                AddCandidate(candidates, "Models/" + withoutExtension);
            }

            if (model != null)
            {
                string name = model.Name.Get(MicroverseLanguage.Spanish);
                if (!string.IsNullOrWhiteSpace(name) && name.ToLowerInvariant().Contains("cromossomo"))
                {
                    AddCandidate(candidates, "Models/Cromossomo");
                }

                string url = model.ModelFileUrl != null ? model.ModelFileUrl.ToLowerInvariant() : string.Empty;
                if (url.Contains("cromossomo"))
                {
                    AddCandidate(candidates, "Models/Cromossomo");
                }
            }

            return candidates;
        }

        private void AddCandidate(List<string> candidates, string candidate)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && !candidates.Contains(candidate))
            {
                candidates.Add(candidate);
            }
        }

        private string ResourcePathFromReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return string.Empty;
            }

            if (reference.StartsWith("resource:", StringComparison.OrdinalIgnoreCase))
            {
                return StripExtension(reference.Substring("resource:".Length).Trim());
            }

            string clean = reference.Split('?')[0].Replace('\\', '/');
            string fileName = Path.GetFileName(clean);
            string withoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (clean.StartsWith("Models/", StringComparison.OrdinalIgnoreCase))
            {
                return StripExtension(clean);
            }

            if (clean.Contains("/Models/"))
            {
                int index = clean.LastIndexOf("/Models/", StringComparison.OrdinalIgnoreCase);
                return StripExtension(clean.Substring(index + 1));
            }

            return string.IsNullOrWhiteSpace(withoutExtension) ? string.Empty : "Models/" + withoutExtension;
        }

        private string StripExtension(string path)
        {
            string extension = Path.GetExtension(path);
            return string.IsNullOrWhiteSpace(extension) ? path : path.Substring(0, path.Length - extension.Length);
        }

        private string FileNameFromReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return string.Empty;
            }

            string clean = reference.Split('?')[0].Replace('\\', '/');
            return Path.GetFileName(clean);
        }

        private string FileNameWithoutExtensionFromReference(string reference)
        {
            string fileName = FileNameFromReference(reference);
            return string.IsNullOrWhiteSpace(fileName) ? StripExtension(reference) : Path.GetFileNameWithoutExtension(fileName);
        }

        private void InstantiateAndNormalizeModel(GameObject holder, GameObject prefab, string instanceName)
        {
            ClearChildren(holder.transform);
            GameObject inst = Instantiate(prefab);
            inst.name = instanceName;
            inst.transform.SetParent(holder.transform, false);
            NormalizeHolderContents(holder);
        }

        private void NormalizeHolderContents(GameObject holder)
        {
            Renderer[] renderers = holder.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float maxDimension = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (maxDimension <= 0.001f)
            {
                return;
            }

            float scaleFactor = 2.2f / maxDimension;
            Vector3 offset = -bounds.center * scaleFactor;
            for (int i = 0; i < holder.transform.childCount; i++)
            {
                Transform child = holder.transform.GetChild(i);
                child.localScale *= scaleFactor;
                child.localPosition = child.localPosition * scaleFactor + offset;
            }
        }

        private void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private string FirstRuntimeGltfSource(string localPath, string remoteUrl)
        {
            if (IsRuntimeGltfSource(localPath))
            {
                return localPath;
            }

            if (IsRuntimeGltfSource(remoteUrl))
            {
                return remoteUrl;
            }

            return string.Empty;
        }

        private bool IsRuntimeGltfSource(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string clean = value.Split('?')[0];
            string extension = Path.GetExtension(clean).ToLowerInvariant();
            return extension == ".glb" || extension == ".gltf";
        }

        private async void TryReplaceWithGltfModelAsync(GameObject holder, string source, BiologicalModel model)
        {
            bool loaded = await TryLoadGltfWithInstalledImporter(source, holder.transform);
            if (!loaded || holder == null)
            {
                Debug.LogWarning("[AR Model] Could not load runtime glTF model for " + model.Name.Get(language) + ". Keeping procedural fallback.");
                return;
            }

            ClearProceduralFallback(holder.transform);
            NormalizeHolderContents(holder);
            Debug.Log("[AR Model] Loaded runtime glTF model: " + source);
        }

        private async Task<bool> TryLoadGltfWithInstalledImporter(string source, Transform parent)
        {
            Type importerType = FindTypeInLoadedAssemblies("GLTFast.GltfImport");
            if (importerType == null)
            {
                Debug.LogWarning("[AR Model] glTFast is not available yet. Unity Package Manager must resolve com.unity.cloud.gltfast.");
                return false;
            }

            object importer = Activator.CreateInstance(importerType);
            try
            {
                MethodInfo loadMethod = FindMethodStartingWith(importerType, "Load", typeof(string));
                MethodInfo instantiateMethod = FindMethodStartingWith(importerType, "InstantiateMainSceneAsync", typeof(Transform));
                if (loadMethod == null || instantiateMethod == null)
                {
                    return false;
                }

                object loadTask = loadMethod.Invoke(importer, BuildInvokeArguments(loadMethod, UrlForGltfImporter(source)));
                await (Task)loadTask;
                if (!TaskResult(loadTask))
                {
                    return false;
                }

                int existingChildCount = parent.childCount;
                object instantiateTask = instantiateMethod.Invoke(importer, BuildInvokeArguments(instantiateMethod, parent));
                await (Task)instantiateTask;

                bool instantiated = TaskResult(instantiateTask);
                return instantiated && parent.childCount > existingChildCount;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[AR Model] glTF runtime load failed: " + ex.Message);
                return false;
            }
            finally
            {
                IDisposable disposable = importer as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        private MethodInfo FindMethodStartingWith(Type type, string name, Type firstParameterType)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name != name)
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 0 && parameters[0].ParameterType == firstParameterType && typeof(Task).IsAssignableFrom(method.ReturnType))
                {
                    return method;
                }
            }

            return null;
        }

        private object[] BuildInvokeArguments(MethodInfo method, object firstArgument)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] args = new object[parameters.Length];
            if (args.Length == 0)
            {
                return args;
            }

            args[0] = firstArgument;
            for (int i = 1; i < args.Length; i++)
            {
                if (parameters[i].HasDefaultValue)
                {
                    args[i] = parameters[i].DefaultValue;
                    if (args[i] == null || args[i] == DBNull.Value || args[i] == Type.Missing)
                    {
                        args[i] = parameters[i].ParameterType.IsValueType ? Activator.CreateInstance(parameters[i].ParameterType) : null;
                    }
                }
                else if (parameters[i].ParameterType.IsValueType)
                {
                    args[i] = Activator.CreateInstance(parameters[i].ParameterType);
                }
                else
                {
                    args[i] = null;
                }
            }

            return args;
        }

        private Type FindTypeInLoadedAssemblies(string fullName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private bool TaskResult(object task)
        {
            PropertyInfo result = task.GetType().GetProperty("Result");
            if (result == null || result.PropertyType != typeof(bool))
            {
                return true;
            }

            return (bool)result.GetValue(task);
        }

        private string UrlForGltfImporter(string source)
        {
            if (!string.IsNullOrWhiteSpace(source) && File.Exists(source))
            {
                return new Uri(source).AbsoluteUri;
            }

            return source;
        }

        private void ClearProceduralFallback(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.name == "LoadingFallbackModel")
                {
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
                }
            }
        }

        private Material CreateTransparentMaterial(Color baseColor)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.color = baseColor;
            mat.SetFloat("_Glossiness", 0.75f);
            mat.SetFloat("_Metallic", 0.1f);
            return mat;
        }

        private Material CreateSolidMaterial(Color baseColor)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = baseColor;
            mat.SetFloat("_Glossiness", 0.6f);
            mat.SetFloat("_Metallic", 0.1f);
            return mat;
        }
    }
}
