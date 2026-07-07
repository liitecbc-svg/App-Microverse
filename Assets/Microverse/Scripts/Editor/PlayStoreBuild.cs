#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Microverse.Editor
{
    public static class PlayStoreBuild
    {
        private const string PackageName = "com.microverse.app";
        private const string ProductName = "Microverse";
        private const string MainScene = "Assets/Scenes/SampleScene.unity";
        private const string AppIcon = "Assets/Microverse/Resources/AppLogo/microverse-logo-main.png";

        [MenuItem("Microverse/Android/Configure Play Store Settings")]
        public static void ConfigurePlayStoreSettings()
        {
            ConfigureAndroidPlayerSettings();
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

            int iconCount = PlayerSettings.GetIconSizes(NamedBuildTarget.Android, IconKind.Application).Length;
            Texture2D[] icons = new Texture2D[iconCount];
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = icon;
            }

            PlayerSettings.SetIcons(NamedBuildTarget.Android, icons, IconKind.Application);
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
