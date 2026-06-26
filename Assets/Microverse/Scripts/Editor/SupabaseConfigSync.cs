#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Microverse.Editor
{
    [InitializeOnLoad]
    public class SupabaseConfigSync
    {
        static SupabaseConfigSync()
        {
            // Run synchronization when the Unity Editor loads or compiles scripts
            SyncEnvToConfig();
        }

        [MenuItem("Microverse/Sync .env to Config")]
        public static void SyncEnvToConfig()
        {
            string envPath = Path.Combine(Application.dataPath, "../.env");
            string resourcesDir = Path.Combine(Application.dataPath, "Resources");
            string configPath = Path.Combine(resourcesDir, "supabase_config.json");

            if (!File.Exists(envPath))
            {
                return;
            }

            try
            {
                string url = "";
                string key = "";

                string[] lines = File.ReadAllLines(envPath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    int index = line.IndexOf('=');
                    if (index <= 0) continue;
                    string keyName = line.Substring(0, index).Trim();
                    string val = line.Substring(index + 1).Trim();

                    // Remove quotes if present
                    if ((val.StartsWith("\"") && val.EndsWith("\"")) || (val.StartsWith("'") && val.EndsWith("'")))
                    {
                        val = val.Substring(1, val.Length - 2);
                    }

                    if (keyName == "SUPABASE_URL" || keyName == "VITE_SUPABASE_URL") url = val;
                    else if (keyName == "SUPABASE_KEY" || keyName == "VITE_SUPABASE_ANON_KEY" || keyName == "VITE_SUPABASE_KEY") key = val;
                }

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
                {
                    return;
                }

                // Ensure the Assets/Resources directory exists
                if (!Directory.Exists(resourcesDir))
                {
                    Directory.CreateDirectory(resourcesDir);
                }

                // Read existing config to avoid rewriting identical files
                bool shouldWrite = true;
                if (File.Exists(configPath))
                {
                    string existingText = File.ReadAllText(configPath);
                    if (existingText.Contains(url) && existingText.Contains(key))
                    {
                        shouldWrite = false;
                    }
                }

                if (shouldWrite)
                {
                    string json = $"{{\n  \"supabaseUrl\": \"{url}\",\n  \"supabaseKey\": \"{key}\"\n}}";
                    File.WriteAllText(configPath, json);
                    
                    // Import the asset so Unity registers it instantly
                    AssetDatabase.ImportAsset("Assets/Resources/supabase_config.json");
                    Debug.Log("Supabase config synchronized from .env successfully.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to automatically sync .env to supabase_config.json: " + ex.Message);
            }
        }
    }
}
#endif
