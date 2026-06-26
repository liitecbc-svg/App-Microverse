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
            DetailScreenView detail = new DetailScreenView(screenRoot, models, model, language, ShowHome, EnterARMode, GetUiText);
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

            GameObject safeFrame = UiFactory.Panel("ARFrame", arOverlayCanvas.transform, new Color(0.01f, 0.03f, 0.08f, 0.4f), 30);
            RectTransform frameRect = safeFrame.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = new Vector2(24f, 26f);
            frameRect.offsetMax = new Vector2(-24f, -26f);

            string backLabel = "<  " + GetUiText("detail.back.types");
            Button backButton = UiFactory.Button("ARBackButton", safeFrame.transform, backLabel, ExitARMode, new Color(0.05f, 0.12f, 0.28f, 0.85f), MicroverseTheme.Text, 25);
            RectTransform backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 1f);
            backRect.anchorMax = new Vector2(0f, 1f);
            backRect.pivot = new Vector2(0f, 1f);
            backRect.anchoredPosition = new Vector2(38f, -38f);
            backRect.sizeDelta = new Vector2(340f, 75f);

            string instruction = language == MicroverseLanguage.Spanish ? 
                "Usa 1 dedo para rotar la célula\nUsa 2 dedos para cambiar el tamaño" : 
                (language == MicroverseLanguage.Portuguese ?
                "Use 1 dedo para rotar a célula\nUse 2 dedos para redimensionar" :
                "Use 1 finger to rotate the cell\nUse 2 fingers to resize");
            
            TextMeshProUGUI instructionText = UiFactory.Text("ARInstructions", safeFrame.transform, instruction, 24, FontStyles.Normal, MicroverseTheme.Text, TextAlignmentOptions.Center);
            RectTransform instructionRect = instructionText.rectTransform;
            instructionRect.anchorMin = new Vector2(0f, 0f);
            instructionRect.anchorMax = new Vector2(1f, 0f);
            instructionRect.pivot = new Vector2(0.5f, 0f);
            instructionRect.anchoredPosition = new Vector2(0f, 50f);
            instructionRect.sizeDelta = new Vector2(900f, 100f);

            TextMeshProUGUI cellLabel = UiFactory.Text("ARCellLabel", safeFrame.transform, model.Name.Get(language), 36, FontStyles.Bold, MicroverseTheme.Cyan, TextAlignmentOptions.Center);
            RectTransform labelRect = cellLabel.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = new Vector2(0f, -150f);
            labelRect.sizeDelta = new Vector2(800f, 80f);
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
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(model.ModelFileUrl))
            {
                string filename = System.IO.Path.GetFileName(model.ModelFileUrl);
                string filenameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filename);

                string[] guids = UnityEditor.AssetDatabase.FindAssets(filenameWithoutExtension);
                if (guids != null && guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        Debug.Log($"[AR Model] Successfully loaded real FBX model from Assets: '{path}'");
                        
                        // Create parent holder
                        GameObject holder = new GameObject("3DCell_" + model.Id);
                        
                        // Instantiate the actual model as child
                        GameObject inst = Instantiate(prefab);
                        inst.name = "FBXModel";
                        inst.transform.SetParent(holder.transform, false);
                        
                        // Calculate bounds of the child model in world space
                        Renderer[] renderers = inst.GetComponentsInChildren<Renderer>();
                        if (renderers.Length > 0)
                        {
                            Bounds bounds = renderers[0].bounds;
                            for (int i = 1; i < renderers.Length; i++)
                            {
                                bounds.Encapsulate(renderers[i].bounds);
                            }
                            
                            // Scale child so visual size is about 2.2f units
                            float maxDimension = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                            if (maxDimension > 0.001f)
                            {
                                float targetSize = 2.2f;
                                float scaleFactor = targetSize / maxDimension;
                                inst.transform.localScale = Vector3.one * scaleFactor;
                                inst.transform.localPosition = -bounds.center * scaleFactor;
                            }
                        }
                        
                        return holder;
                    }
                }
                Debug.LogWarning($"[AR Model] Real model FBX file '{filename}' was not found in Assets. Falling back to procedural model.");
            }
#endif
            return CreateProcedural3DCell(model);
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
