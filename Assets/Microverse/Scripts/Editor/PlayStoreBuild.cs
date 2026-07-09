/**
 * PlayStoreBuild.cs
 *
 * Automatiza la configuracion y construccion Android orientada a publicar Microverse en Play Store.
 *
 * Main responsibilities:
 * - Aplicar package name, version, SDK, IL2CPP, escenas e iconos Android.
 * - Leer versionado y firma desde variables de entorno.
 * - Generar un AAB de release desde el menu o linea de comandos.
 *
 * Related elements:
 * - PlayerSettings
 * - EditorBuildSettings
 * - Microverse/Android menu
 */
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Microverse.Editor
{
    public class PlayStoreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                ConfigureAndroidPlayerSettings();
                AssetDatabase.SaveAssets();
            }
        }

        private const string PackageName = "com.microverse.app";
        private const string ProductName = "Microverse";
        private const string MainScene = "Assets/Scenes/SampleScene.unity";
        private const string AppIcon = "Assets/Microverse/Resources/AppLogo/microverse-logo-main.png";
        private const string AppIconBackground = "Assets/Microverse/Resources/AppLogo/microverse-logo-background.png";
        private const string AppIconForeground = "Assets/Microverse/Resources/AppLogo/microverse-logo-foreground.png";

        [MenuItem("Microverse/Android/Configure Play Store Settings")]
        public static void ConfigurePlayStoreSettings()
        {
            ConfigureAndroidPlayerSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Microverse Android Play Store settings configured.");
        }

        [MenuItem("Microverse/Android/Bump Version Code")]
        public static void BumpVersionCode()
        {
            PlayerSettings.Android.bundleVersionCode += 1;
            Debug.Log("Android versionCode bumped to " + PlayerSettings.Android.bundleVersionCode + ".");
        }

        public static void BuildPlayStoreRelease()
        {
            ConfigureAndroidPlayerSettings();
            ApplyCommandLineVersion();
            ApplySigningFromEnvironment(requireSigning: true);
            AssetDatabase.SaveAssets();

            string outputDirectory = Path.Combine("Builds", "Android");
            Directory.CreateDirectory(outputDirectory);

            string outputPath = Path.Combine(
                outputDirectory,
                ProductName + "-v" + PlayerSettings.bundleVersion + "-" + PlayerSettings.Android.bundleVersionCode + ".aab");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { MainScene },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("Play Store build failed: " + report.summary.result);
            }

            Debug.Log("Play Store AAB generated at " + outputPath);
        }

        private static void ConfigureAndroidPlayerSettings()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

            PlayerSettings.companyName = "Microverse";
            PlayerSettings.productName = ProductName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageName);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)35;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = Math.Max(1, PlayerSettings.Android.bundleVersionCode);
            ApplyAndroidIcons();

            if (string.IsNullOrWhiteSpace(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion == "0.1")
            {
                PlayerSettings.bundleVersion = "1.0.0";
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScene, true)
            };
        }

        private static void ApplyAndroidIcons()
        {
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIcon);
            if (icon == null)
            {
                Debug.LogWarning("App icon not found at " + AppIcon + ".");
                return;
            }

            var platform = NamedBuildTarget.Android;

            // 1. Set legacy/standard icons (IconKind.Application)
            int iconCount = PlayerSettings.GetIconSizes(platform, IconKind.Application).Length;
            Texture2D[] icons = new Texture2D[iconCount];
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = icon;
            }
            PlayerSettings.SetIcons(platform, icons, IconKind.Application);

            // 2. Set round icons (AndroidPlatformIconKind.Round)
            var roundKind = UnityEditor.Android.AndroidPlatformIconKind.Round;
            PlatformIcon[] roundIcons = PlayerSettings.GetPlatformIcons(platform, roundKind);
            for (int i = 0; i < roundIcons.Length; i++)
            {
                roundIcons[i].SetTextures(new Texture2D[] { icon });
            }
            PlayerSettings.SetPlatformIcons(platform, roundKind, roundIcons);

            // 3. Set adaptive icons (AndroidPlatformIconKind.Adaptive)
            Texture2D bgIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconBackground);
            Texture2D fgIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconForeground);
            if (bgIcon != null && fgIcon != null)
            {
                var adaptiveKind = UnityEditor.Android.AndroidPlatformIconKind.Adaptive;
                PlatformIcon[] adaptiveIcons = PlayerSettings.GetPlatformIcons(platform, adaptiveKind);
                for (int i = 0; i < adaptiveIcons.Length; i++)
                {
                    adaptiveIcons[i].SetTextures(new Texture2D[] { bgIcon, fgIcon });
                }
                PlayerSettings.SetPlatformIcons(platform, adaptiveKind, adaptiveIcons);
                Debug.Log("Adaptive icons (foreground/background) successfully configured.");
            }
            else
            {
                Debug.LogWarning("Adaptive icon layers (background/foreground) not found at paths. Skipping adaptive icon configuration.");
            }
        }

        private static void ApplyCommandLineVersion()
        {
            string versionName = Environment.GetEnvironmentVariable("MICROVERSE_VERSION_NAME");
            if (!string.IsNullOrWhiteSpace(versionName))
            {
                PlayerSettings.bundleVersion = versionName.Trim();
            }

            string versionCode = Environment.GetEnvironmentVariable("MICROVERSE_VERSION_CODE");
            if (int.TryParse(versionCode, out int parsedVersionCode) && parsedVersionCode > 0)
            {
                PlayerSettings.Android.bundleVersionCode = parsedVersionCode;
            }
        }

        private static void ApplySigningFromEnvironment(bool requireSigning)
        {
            string keystorePath = Environment.GetEnvironmentVariable("MICROVERSE_KEYSTORE_PATH");
            string keystorePass = Environment.GetEnvironmentVariable("MICROVERSE_KEYSTORE_PASS");
            string keyAlias = Environment.GetEnvironmentVariable("MICROVERSE_KEY_ALIAS");
            string keyPass = Environment.GetEnvironmentVariable("MICROVERSE_KEY_PASS");

            bool hasSigningConfig =
                !string.IsNullOrWhiteSpace(keystorePath) &&
                !string.IsNullOrWhiteSpace(keystorePass) &&
                !string.IsNullOrWhiteSpace(keyAlias) &&
                !string.IsNullOrWhiteSpace(keyPass);

            if (!hasSigningConfig)
            {
                if (requireSigning)
                {
                    throw new InvalidOperationException("Missing signing environment variables. Set MICROVERSE_KEYSTORE_PATH, MICROVERSE_KEYSTORE_PASS, MICROVERSE_KEY_ALIAS, and MICROVERSE_KEY_PASS.");
                }

                return;
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = keyAlias;
            PlayerSettings.Android.keyaliasPass = keyPass;
        }
    }
}
#endif
