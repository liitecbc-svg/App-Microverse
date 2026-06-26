using System;
using System.Collections;
using System.Collections.Generic;
using Microverse.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Microverse.Services
{
    public class SupabaseModelCatalogService : IModelCatalogService
    {
        private List<BiologicalModel> cachedModels = new List<BiologicalModel>();
        private List<string> cachedCategories = new List<string>();
        private bool isLoaded = false;

        // Config class matching resources JSON
        [Serializable]
        private class SupabaseConfig
        {
            public string supabaseUrl;
            public string supabaseKey;
        }

        // DB Mapping Classes for JsonUtility
        [Serializable]
        private class SupabaseCategory
        {
            public long id;
            public string nombre;
        }

        [Serializable]
        private class SupabaseCategoryList
        {
            public List<SupabaseCategory> items;
        }

        [Serializable]
        private class SupabaseModel
        {
            public int id;
            public string nombre;
            public string archivo_modelo_url;
            public string preview_url;
            public string formato;
            public long tamano_bytes;
            public string descripcion;
            public long categoria_id;
            
            // Optional styling/metadata columns
            public string subtitle;
            public string scientific_name;
            public string primary_color;
            public string secondary_color;
            public int visual_seed;
            public bool is_elongated;
        }

        [Serializable]
        private class SupabaseModelList
        {
            public List<SupabaseModel> items;
        }

        private class CoroutineRunner : MonoBehaviour { }

        public IReadOnlyList<BiologicalModel> GetModels()
        {
            return cachedModels;
        }

        public IReadOnlyList<string> GetCategories()
        {
            return cachedCategories;
        }

        private struct Credentials
        {
            public string url;
            public string key;
        }

        private Credentials LoadCredentials(out string logMessage)
        {
            Credentials creds = new Credentials();
            logMessage = "";

            // 1. Try to read from .env at project root
            string envPath = System.IO.Path.Combine(Application.dataPath, "../.env");
            if (System.IO.File.Exists(envPath))
            {
                try
                {
                    string[] lines = System.IO.File.ReadAllLines(envPath);
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

                        if (keyName == "SUPABASE_URL" || keyName == "VITE_SUPABASE_URL") creds.url = val;
                        else if (keyName == "SUPABASE_KEY" || keyName == "VITE_SUPABASE_ANON_KEY" || keyName == "VITE_SUPABASE_KEY") creds.key = val;
                    }

                    if (!string.IsNullOrEmpty(creds.url) && !string.IsNullOrEmpty(creds.key))
                    {
                        logMessage = "Loaded Supabase credentials from root .env file.";
                        return creds;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Error reading .env file, falling back to resources: " + ex.Message);
                }
            }

            // 2. Fallback to Resources/supabase_config.json
            TextAsset configAsset = Resources.Load<TextAsset>("supabase_config");
            if (configAsset != null)
            {
                try
                {
                    SupabaseConfig config = JsonUtility.FromJson<SupabaseConfig>(configAsset.text);
                    creds.url = config.supabaseUrl;
                    creds.key = config.supabaseKey;
                    logMessage = "Loaded Supabase credentials from Resources/supabase_config.json.";
                    return creds;
                }
                catch (Exception ex)
                {
                    logMessage = "Failed to parse supabase_config.json: " + ex.Message;
                }
            }
            else
            {
                logMessage = "supabase_config.json not found in Resources and .env file is missing/invalid.";
            }

            return creds;
        }

        public void LoadModels(Action<IReadOnlyList<BiologicalModel>> onComplete, Action<string> onError)
        {
            if (isLoaded)
            {
                onComplete?.Invoke(cachedModels);
                return;
            }

            string logMessage;
            Credentials creds = LoadCredentials(out logMessage);

            if (string.IsNullOrEmpty(creds.url) || string.IsNullOrEmpty(creds.key) ||
                creds.url.Contains("your-project") || creds.key.Contains("your-anon-key"))
            {
                onError?.Invoke("Supabase credentials not configured. " + logMessage);
                return;
            }

            Debug.Log(logMessage);

            // Spawn dynamic coroutine runner to fetch data from Supabase
            GameObject runnerGo = new GameObject("SupabaseLoaderRunner");
            CoroutineRunner runner = runnerGo.AddComponent<CoroutineRunner>();
            UnityEngine.Object.DontDestroyOnLoad(runnerGo);

            runner.StartCoroutine(FetchFromSupabaseRoutine(creds.url, creds.key, onComplete, onError, runnerGo));
        }

        private IEnumerator FetchFromSupabaseRoutine(
            string url, 
            string key, 
            Action<IReadOnlyList<BiologicalModel>> onComplete, 
            Action<string> onError, 
            GameObject runnerGo)
        {
            string cleanUrl = url.TrimEnd('/');
            string categoriesUrl = cleanUrl + "/rest/v1/categorias?select=id,nombre";
            string modelsUrl = cleanUrl + "/rest/v1/modelos_3d?select=*";

            // 1. Fetch Categories
            Debug.Log("[Supabase] Fetching categories from: " + categoriesUrl);
            UnityWebRequest catRequest = CreateSupabaseRequest(categoriesUrl, key);
            yield return catRequest.SendWebRequest();

            if (catRequest.result != UnityWebRequest.Result.Success)
            {
                string errMsg = "Failed to fetch categories: " + catRequest.error;
                Debug.LogError("[Supabase] " + errMsg);
                catRequest.Dispose();
                onError?.Invoke(errMsg);
                UnityEngine.Object.Destroy(runnerGo);
                yield break;
            }

            string catJson = catRequest.downloadHandler.text;
            Debug.Log("[Supabase] Categories response JSON: " + catJson);
            catRequest.Dispose();

            // 2. Fetch Models
            Debug.Log("[Supabase] Fetching 3D models from: " + modelsUrl);
            UnityWebRequest modelRequest = CreateSupabaseRequest(modelsUrl, key);
            yield return modelRequest.SendWebRequest();

            if (modelRequest.result != UnityWebRequest.Result.Success)
            {
                string errMsg = "Failed to fetch models: " + modelRequest.error;
                Debug.LogError("[Supabase] " + errMsg);
                modelRequest.Dispose();
                onError?.Invoke(errMsg);
                UnityEngine.Object.Destroy(runnerGo);
                yield break;
            }

            string modelJson = modelRequest.downloadHandler.text;
            Debug.Log("[Supabase] Models response JSON: " + modelJson);
            modelRequest.Dispose();

            // 3. Parse JSON & build model list
            try
            {
                // Wrap top level JSON arrays in objects for JsonUtility
                string wrappedCatJson = "{\"items\":" + catJson + "}";
                string wrappedModelJson = "{\"items\":" + modelJson + "}";

                var categoriesData = JsonUtility.FromJson<SupabaseCategoryList>(wrappedCatJson);
                var modelsData = JsonUtility.FromJson<SupabaseModelList>(wrappedModelJson);

                // Create category lookup map
                Dictionary<long, string> categoryMap = new Dictionary<long, string>();
                List<string> loadedCats = new List<string>();
                if (categoriesData != null && categoriesData.items != null)
                {
                    foreach (var cat in categoriesData.items)
                    {
                        categoryMap[cat.id] = cat.nombre;
                        if (!loadedCats.Contains(cat.nombre))
                        {
                            loadedCats.Add(cat.nombre);
                        }
                    }
                }

                List<BiologicalModel> loadedList = new List<BiologicalModel>();
                if (modelsData != null && modelsData.items != null)
                {
                    foreach (var model in modelsData.items)
                    {
                        // Map category
                        string categoryName = "Otros";
                        if (categoryMap.TryGetValue(model.categoria_id, out string mappedCatName))
                        {
                            categoryName = mappedCatName;
                        }

                        // Determine Subtitle
                        string subtitle = model.subtitle;
                        if (string.IsNullOrEmpty(subtitle))
                        {
                            subtitle = categoryName; // Fallback to category
                        }

                        // Determine Scientific Name
                        string scientificName = model.scientific_name;
                        if (string.IsNullOrEmpty(scientificName))
                        {
                            scientificName = model.nombre; // Fallback to name
                        }

                        // Determine visual seed
                        int seed = model.visual_seed;
                        if (seed == 0)
                        {
                            seed = Mathf.Abs(model.nombre.GetHashCode() % 100);
                        }

                        // Determine if elongated
                        bool isElongated = model.is_elongated;
                        if (!isElongated)
                        {
                            string lowerName = model.nombre.ToLowerInvariant();
                            string lowerSci = scientificName.ToLowerInvariant();
                            isElongated = lowerName.Contains("coli") || lowerName.Contains("paramecio") || lowerName.Contains("euglena") ||
                                          lowerName.Contains("bacil") || lowerSci.Contains("coli") || lowerSci.Contains("paramecium") ||
                                          lowerSci.Contains("euglena") || lowerSci.Contains("bacillus");
                        }

                        // Parse colors with harmonic procedural fallback
                        Color primaryColor = ParseColor(model.primary_color, Color.clear);
                        Color secondaryColor = ParseColor(model.secondary_color, Color.clear);

                        if (primaryColor == Color.clear || secondaryColor == Color.clear)
                        {
                            // Procedural colors based on category/name hash to keep UI premium and colorful
                            int hash = model.nombre.GetHashCode();
                            float hue1 = Mathf.Abs((hash % 360) / 360f);
                            float hue2 = Mathf.Abs(((hash + 140) % 360) / 360f); // Harmonic complementary hue

                            // Adjust hues based on category keywords
                            string catLower = categoryName.ToLowerInvariant();
                            if (catLower.Contains("célula") || catLower.Contains("celula"))
                            {
                                hue1 = 0.58f; // Cyan-blue range
                                hue2 = 0.78f; // Purple range
                            }
                            else if (catLower.Contains("bacteria"))
                            {
                                hue1 = 0.22f; // Lime green
                                hue2 = 0.35f; // Dark green
                            }
                            else if (catLower.Contains("virus"))
                            {
                                hue1 = 0.55f; // Blue-cyan
                                hue2 = 0.88f; // Pink-magenta
                            }
                            else if (catLower.Contains("protozoo") || catLower.Contains("protozo"))
                            {
                                hue1 = 0.45f; // Emerald green
                                hue2 = 0.13f; // Orange-yellow
                            }

                            if (primaryColor == Color.clear)
                            {
                                primaryColor = Color.HSVToRGB(hue1, 0.78f, 0.95f);
                            }
                            if (secondaryColor == Color.clear)
                            {
                                secondaryColor = Color.HSVToRGB(hue2, 0.78f, 0.95f);
                            }
                        }

                        // Instanciate BiologicalModel (client translation handles translating these from Spanish to other languages)
                        BiologicalModel bioModel = new BiologicalModel(
                            model.id.ToString(),
                            new LocalizedText(model.nombre, model.nombre, model.nombre),
                            new LocalizedText(subtitle, subtitle, subtitle),
                            new LocalizedText(categoryName, categoryName, categoryName),
                            new LocalizedText(model.descripcion ?? "", model.descripcion ?? "", model.descripcion ?? ""),
                            scientificName,
                            primaryColor,
                            secondaryColor,
                            seed,
                            isElongated,
                            model.archivo_modelo_url ?? "",
                            model.preview_url ?? ""
                        );

                        loadedList.Add(bioModel);
                    }
                }

                cachedModels = loadedList;
                cachedCategories = loadedCats;
                isLoaded = true;
                Debug.Log("[Supabase] Loaded and mapped " + cachedModels.Count + " models successfully.");
                onComplete?.Invoke(cachedModels);
            }
            catch (Exception ex)
            {
                string parseError = "Failed to parse Supabase JSON: " + ex.Message;
                Debug.LogError("[Supabase] " + parseError);
                onError?.Invoke(parseError);
            }

            UnityEngine.Object.Destroy(runnerGo);
        }

        private UnityWebRequest CreateSupabaseRequest(string url, string key)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("apikey", key);
            request.SetRequestHeader("Authorization", "Bearer " + key);
            request.SetRequestHeader("Accept", "application/json");
            return request;
        }

        private Color ParseColor(string hex, Color fallback)
        {
            if (string.IsNullOrEmpty(hex)) return fallback;
            
            // Format check
            string cleanHex = hex.Trim();
            if (!cleanHex.StartsWith("#"))
            {
                cleanHex = "#" + cleanHex;
            }

            if (ColorUtility.TryParseHtmlString(cleanHex, out Color color))
            {
                return color;
            }
            return fallback;
        }
    }
}
