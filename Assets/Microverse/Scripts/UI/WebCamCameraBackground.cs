using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Microverse.UI
{
    [RequireComponent(typeof(RawImage))]
    public class WebCamCameraBackground : MonoBehaviour
    {
        private struct CameraAttempt
        {
            public string deviceName;
            public int width;
            public int height;
            public int fps;
            public bool useDefaultConstructor;
            
            public CameraAttempt(string deviceName, int width, int height, int fps)
            {
                this.deviceName = deviceName;
                this.width = width;
                this.height = height;
                this.fps = fps;
                this.useDefaultConstructor = false;
            }
            
            public CameraAttempt(string deviceName)
            {
                this.deviceName = deviceName;
                this.width = 0;
                this.height = 0;
                this.fps = 0;
                this.useDefaultConstructor = true;
            }
        }

        private RawImage rawImage;
        private WebCamTexture webcamTexture;
        private AspectRatioFitter fitter;
        private bool isInitialized = false;

        private float playStartTime = 0f;
        private bool hasDetectedFrame = false;
        private bool hasFailed = false;

        private List<CameraAttempt> attempts = new List<CameraAttempt>();
        private int currentAttemptIndex = 0;

        private Coroutine startCameraCoroutine;
        private bool isWaitingForPermission = false;

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && isWaitingForPermission)
            {
                isWaitingForPermission = false;
            }
        }

        private void Awake()
        {
            rawImage = GetComponent<RawImage>();
            
            // Add AspectRatioFitter dynamically if not present
            fitter = GetComponent<AspectRatioFitter>();
            if (fitter == null)
            {
                fitter = gameObject.AddComponent<AspectRatioFitter>();
            }
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }

        private void OnEnable()
        {
            StartCamera();
        }

        private void OnDisable()
        {
            if (startCameraCoroutine != null)
            {
                StopCoroutine(startCameraCoroutine);
                startCameraCoroutine = null;
            }
            StopCamera();
        }

        private void StartCamera()
        {
            if (startCameraCoroutine != null)
            {
                StopCoroutine(startCameraCoroutine);
            }
            startCameraCoroutine = StartCoroutine(StartCameraRoutine());
        }

        private System.Collections.IEnumerator StartCameraRoutine()
        {
            if (webcamTexture != null) yield break;

            // Request camera permission if on mobile
            #if UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                isWaitingForPermission = true;
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
                
                // Wait until the user responds to the permission dialog
                while (isWaitingForPermission && !UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
                {
                    yield return null;
                }
                isWaitingForPermission = false;
            }
            #elif UNITY_IOS
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }
            #endif

            // Reset state flags
            hasFailed = false;
            hasDetectedFrame = false;

            // Find and list all devices
            var webCamDevices = WebCamTexture.devices;
            attempts.Clear();

            if (webCamDevices == null || webCamDevices.Length == 0)
            {
                Debug.LogWarning("No camera device found or accessible.");
                ShowFallbackBackground();
                yield break;
            }

            // Group devices into primary candidate list (e.g. back facing or video0/even) and fallback list
            List<WebCamDevice> primaryDevices = new List<WebCamDevice>();
            List<WebCamDevice> secondaryDevices = new List<WebCamDevice>();

            foreach (var dev in webCamDevices)
            {
                // Check if name contains /dev/video followed by an odd number, which is typically metadata node on Linux
                bool isMetadataNode = false;
                if (dev.name.Contains("/dev/video"))
                {
                    int index = dev.name.IndexOf("/dev/video") + "/dev/video".Length;
                    if (index < dev.name.Length)
                    {
                        char numChar = dev.name[index];
                        if (char.IsDigit(numChar))
                        {
                            int num = numChar - '0';
                            if (num % 2 != 0) // odd video nodes (video1, video3, etc) are often metadata
                            {
                                isMetadataNode = true;
                            }
                        }
                    }
                }

                if (dev.isFrontFacing || isMetadataNode)
                {
                    secondaryDevices.Add(dev);
                }
                else
                {
                    primaryDevices.Add(dev);
                }
            }

            // Build attempts: primary first, then secondary
            AddAttemptsForDevices(primaryDevices);
            AddAttemptsForDevices(secondaryDevices);

            if (attempts.Count == 0)
            {
                Debug.LogWarning("No suitable camera devices configured for attempts.");
                ShowFallbackBackground();
                yield break;
            }

            currentAttemptIndex = 0;
            TryNextAttempt();
        }

        private void AddAttemptsForDevices(List<WebCamDevice> deviceList)
        {
            foreach (var dev in deviceList)
            {
                // Prefer camera quality that fills a portrait AR view cleanly, then fall back to safer profiles.
                attempts.Add(new CameraAttempt(dev.name, 1920, 1080, 30));
                attempts.Add(new CameraAttempt(dev.name, 1280, 720, 30));
                attempts.Add(new CameraAttempt(dev.name));
                attempts.Add(new CameraAttempt(dev.name, 640, 480, 30));
            }
        }

        private void TryNextAttempt()
        {
            StopCameraOnly();

            if (currentAttemptIndex >= attempts.Count)
            {
                Debug.LogWarning("All camera initialization attempts failed.");
                ShowFallbackBackground();
                return;
            }

            CameraAttempt attempt = attempts[currentAttemptIndex];
            Debug.Log($"Trying camera attempt {currentAttemptIndex + 1}/{attempts.Count}: Device='{attempt.deviceName}', Res={(attempt.useDefaultConstructor ? "Default" : $"{attempt.width}x{attempt.height}@{attempt.fps}fps")}");

            playStartTime = Time.time;
            hasDetectedFrame = false;
            hasFailed = false;

            try
            {
                if (attempt.useDefaultConstructor)
                {
                    webcamTexture = new WebCamTexture(attempt.deviceName);
                }
                else
                {
                    webcamTexture = new WebCamTexture(attempt.deviceName, attempt.width, attempt.height, attempt.fps);
                }

                rawImage.texture = webcamTexture;
                rawImage.uvRect = new Rect(0f, 0f, 1f, 1f);
                webcamTexture.Play();
                isInitialized = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed attempt {currentAttemptIndex + 1}: {ex.Message}");
                currentAttemptIndex++;
                TryNextAttempt();
            }
        }

        private void ShowFallbackBackground()
        {
            hasFailed = true;
            rawImage.texture = null;
            // Set a beautiful dark tech theme color
            rawImage.color = new Color(0.01f, 0.05f, 0.12f, 1f); 

            if (fitter != null)
            {
                fitter.aspectMode = AspectRatioFitter.AspectMode.None;
            }

            // Create instructions/mock-indicator in overlay if not already created
            if (transform.Find("ARFallbackText") == null)
            {
                GameObject textGo = new GameObject("ARFallbackText", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
                textGo.transform.SetParent(transform, false);
                TMPro.TextMeshProUGUI text = textGo.GetComponent<TMPro.TextMeshProUGUI>();
                text.text = "CÁMARA NO INICIALIZADA (MODO DE SIMULACIÓN 3D)\nCAMERA NOT INITIALIZED (3D SIMULATION MODE)";
                text.fontSize = 20;
                text.fontStyle = TMPro.FontStyles.Bold;
                text.color = new Color(0.0f, 0.62f, 0.96f, 0.5f);
                text.alignment = TMPro.TextAlignmentOptions.Center;

                RectTransform rect = text.rectTransform;
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, 0f);
                rect.sizeDelta = new Vector2(800f, 120f);
            }
        }

        private void StopCameraOnly()
        {
            if (webcamTexture != null)
            {
                try
                {
                    if (webcamTexture.isPlaying)
                    {
                        webcamTexture.Stop();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Error stopping WebCamTexture: " + ex.Message);
                }
                Destroy(webcamTexture);
                webcamTexture = null;
            }
            isInitialized = false;
        }

        private void StopCamera()
        {
            StopCameraOnly();
            attempts.Clear();
            currentAttemptIndex = 0;
            hasDetectedFrame = false;
            hasFailed = false;
        }

        private void Update()
        {
            if (hasFailed) return;
            if (!isInitialized || webcamTexture == null) return;

            // Wait until the camera has valid width and height and is actually playing
            if (!hasDetectedFrame)
            {
                if (webcamTexture.isPlaying && webcamTexture.width > 16 && webcamTexture.height > 16)
                {
                    hasDetectedFrame = true;
                    // Destroy fallback text if it exists
                    Transform fallbackText = transform.Find("ARFallbackText");
                    if (fallbackText != null)
                    {
                        Destroy(fallbackText.gameObject);
                    }
                    // Reset rawImage color to white so camera is fully visible
                    rawImage.color = Color.white;
                }
                else if (Time.time - playStartTime > 1.5f)
                {
                    Debug.LogWarning($"WebCamTexture timeout on attempt {currentAttemptIndex + 1}: no valid frames received (isPlaying={webcamTexture.isPlaying}, size={webcamTexture.width}x{webcamTexture.height}). Trying next configuration.");
                    currentAttemptIndex++;
                    TryNextAttempt();
                    return;
                }
                else
                {
                    return; // Wait for next frame
                }
            }

            // Handle camera rotation on mobile device
            int rotationAngle = webcamTexture.videoRotationAngle;
            bool isSideways = Mathf.Abs(rotationAngle) == 90 || Mathf.Abs(rotationAngle) == 270;

            // Get parent rect size to scale manually since AspectRatioFitter does not handle RectTransform rotation
            RectTransform parentRect = transform.parent as RectTransform;
            if (parentRect != null)
            {
                if (fitter != null)
                {
                    fitter.aspectMode = AspectRatioFitter.AspectMode.None;
                }

                Vector2 parentSize = parentRect.rect.size;
                float wParent = parentSize.x;
                float hParent = parentSize.y;

                if (wParent > 0 && hParent > 0)
                {
                    float wTexture = webcamTexture.width;
                    float hTexture = webcamTexture.height;

                    float ratio = isSideways
                        ? hTexture / wTexture
                        : wTexture / hTexture;

                    float wVisual, hVisual;
                    float parentRatio = wParent / hParent;

                    if (parentRatio > ratio)
                    {
                        // Match width
                        wVisual = wParent;
                        hVisual = wVisual / ratio;
                    }
                    else
                    {
                        // Match height
                        hVisual = hParent;
                        wVisual = hVisual * ratio;
                    }

                    // Set anchors and pivot to center so rotation scales from the center point
                    rawImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rawImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rawImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

                    if (isSideways)
                    {
                        rawImage.rectTransform.sizeDelta = new Vector2(hVisual, wVisual);
                    }
                    else
                    {
                        rawImage.rectTransform.sizeDelta = new Vector2(wVisual, hVisual);
                    }
                }
            }

            rawImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, -rotationAngle);

            // Handle vertical mirror (common with front cameras, but good to check)
            float scaleY = webcamTexture.videoVerticallyMirrored ? -1f : 1f;
            rawImage.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
        }
    }
}
