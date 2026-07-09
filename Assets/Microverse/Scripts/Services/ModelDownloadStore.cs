/**
 * ModelDownloadStore.cs
 *
 * Gestiona la descarga, persistencia y recuperacion offline de modelos 3D obtenidos desde el catalogo remoto.
 *
 * Main responsibilities:
 * - Descargar archivos de modelos y previews remotas.
 * - Guardar rutas y metadatos descargados en PlayerPrefs.
 * - Permitir borrar modelos descargados y reconstruir el catalogo offline.
 *
 * Related elements:
 * - PreviewImageStore
 * - CompositeModelCatalogService
 * - HomeScreenView
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microverse.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Microverse.Services
{
    public static class ModelDownloadStore
    {
        private const string PlayerPrefsKey = "microverse.downloaded_model_paths";
        private const string CachedModelsKey = "microverse.downloaded_model_catalog";
        private static readonly Dictionary<string, string> DownloadedPaths = new Dictionary<string, string>();
        private static readonly Dictionary<string, BiologicalModel> DownloadedModels = new Dictionary<string, BiologicalModel>();
        private static bool loaded;

        [Serializable]
        private class DownloadedModelRecord
        {
            public string LocalPath;
            public BiologicalModel Model;
        }

        [Serializable]
        private class DownloadedModelRecordList
        {
            public List<DownloadedModelRecord> Items = new List<DownloadedModelRecord>();
        }

        public static bool IsAvailable(BiologicalModel model)
        {
            return model != null && (model.IsBundledModel || IsDownloaded(model.Id));
        }

        public static bool IsDownloaded(string modelId)
        {
            EnsureLoaded();
            return !string.IsNullOrWhiteSpace(modelId) && DownloadedPaths.ContainsKey(modelId) && File.Exists(DownloadedPaths[modelId]);
        }

        public static bool TryGetLocalModelPath(BiologicalModel model, out string localPath)
        {
            localPath = string.Empty;
            if (model == null)
            {
                return false;
            }

            if (model.IsBundledModel)
            {
                localPath = model.ModelFileUrl;
                return true;
            }

            EnsureLoaded();
            if (!string.IsNullOrWhiteSpace(model.Id) && DownloadedPaths.TryGetValue(model.Id, out string path) && File.Exists(path))
            {
                localPath = path;
                return true;
            }

            return false;
        }

        public static IReadOnlyList<BiologicalModel> GetDownloadedModels()
        {
            EnsureLoaded();
            List<BiologicalModel> models = new List<BiologicalModel>();
            foreach (KeyValuePair<string, BiologicalModel> entry in DownloadedModels)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) ||
                    !DownloadedPaths.TryGetValue(entry.Key, out string path) ||
                    !File.Exists(path))
                {
                    continue;
                }

                BiologicalModel copy = CloneForOfflineCatalog(entry.Value);
                if (copy != null)
                {
                    copy.IsBundledModel = false;
                    models.Add(copy);
                }
            }

            foreach (KeyValuePair<string, string> entry in DownloadedPaths)
            {
                if (DownloadedModels.ContainsKey(entry.Key) || !File.Exists(entry.Value))
                {
                    continue;
                }

                models.Add(CreateLegacyDownloadedModel(entry.Key, entry.Value));
            }

            return models;
        }

        public static void CacheMetadataIfDownloaded(BiologicalModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Id))
            {
                return;
            }

            EnsureLoaded();
            if (!DownloadedPaths.TryGetValue(model.Id, out string path) || !File.Exists(path))
            {
                return;
            }

            CacheDownloadedModel(model);
            Save();
        }

        public static IEnumerator DownloadModelRoutine(BiologicalModel model, Action<bool, string> onComplete, Action<float> onProgress = null)
        {
            onProgress?.Invoke(0f);

            if (model == null)
            {
                onComplete?.Invoke(false, "Model is missing.");
                yield break;
            }

            if (IsAvailable(model))
            {
                EnsureLoaded();
                onProgress?.Invoke(0.95f);
                yield return PreviewImageStore.DownloadPreviewRoutine(model);
                CacheDownloadedModel(model);
                Save();
                onProgress?.Invoke(1f);
                onComplete?.Invoke(true, string.Empty);
                yield break;
            }

            if (string.IsNullOrWhiteSpace(model.ModelFileUrl))
            {
                onComplete?.Invoke(false, "Model does not have a download URL.");
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(model.ModelFileUrl))
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    float downloadProgress = Mathf.Clamp01(request.downloadProgress);
                    onProgress?.Invoke(downloadProgress * 0.92f);
                    yield return null;
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onComplete?.Invoke(false, request.error);
                    yield break;
                }

                string directory = Path.Combine(Application.persistentDataPath, "MicroverseModels");
                Directory.CreateDirectory(directory);

                string fileName = FileNameFromUrl(model.ModelFileUrl);
                string safeName = string.IsNullOrWhiteSpace(fileName)
                    ? SafeFileName(model.Id) + ExtensionFromUrl(model.ModelFileUrl)
                    : SafeFileName(model.Id + "-" + fileName);
                string filePath = Path.Combine(directory, safeName);
                File.WriteAllBytes(filePath, request.downloadHandler.data);

                onProgress?.Invoke(0.96f);
                yield return PreviewImageStore.DownloadPreviewRoutine(model);

                EnsureLoaded();
                DownloadedPaths[model.Id] = filePath;
                CacheDownloadedModel(model);
                Save();
                onProgress?.Invoke(1f);
                onComplete?.Invoke(true, string.Empty);
            }
        }

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;
            DownloadedPaths.Clear();
            DownloadedModels.Clear();

            string raw = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(raw))
            {
                string[] rows = raw.Split('\n');
                for (int i = 0; i < rows.Length; i++)
                {
                    string row = rows[i];
                    int separator = row.IndexOf('|');
                    if (separator <= 0)
                    {
                        continue;
                    }

                    string modelId = row.Substring(0, separator);
                    string path = row.Substring(separator + 1);
                    if (!string.IsNullOrWhiteSpace(modelId) && !string.IsNullOrWhiteSpace(path))
                    {
                        DownloadedPaths[modelId] = path;
                    }
                }
            }

            string cachedRaw = PlayerPrefs.GetString(CachedModelsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(cachedRaw))
            {
                return;
            }

            try
            {
                DownloadedModelRecordList list = JsonUtility.FromJson<DownloadedModelRecordList>(cachedRaw);
                if (list == null || list.Items == null)
                {
                    return;
                }

                for (int i = 0; i < list.Items.Count; i++)
                {
                    DownloadedModelRecord record = list.Items[i];
                    if (record == null || record.Model == null || string.IsNullOrWhiteSpace(record.Model.Id))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(record.LocalPath))
                    {
                        DownloadedPaths[record.Model.Id] = record.LocalPath;
                    }

                    DownloadedModels[record.Model.Id] = CloneForOfflineCatalog(record.Model);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Downloaded model catalog cache could not be read. Keeping path cache only. " + ex.Message);
            }
        }

        private static void Save()
        {
            List<string> rows = new List<string>();
            foreach (KeyValuePair<string, string> entry in DownloadedPaths)
            {
                rows.Add(entry.Key + "|" + entry.Value);
            }

            PlayerPrefs.SetString(PlayerPrefsKey, string.Join("\n", rows.ToArray()));

            DownloadedModelRecordList list = new DownloadedModelRecordList();
            foreach (KeyValuePair<string, BiologicalModel> entry in DownloadedModels)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) ||
                    !DownloadedPaths.TryGetValue(entry.Key, out string path) ||
                    !File.Exists(path))
                {
                    continue;
                }

                list.Items.Add(new DownloadedModelRecord
                {
                    LocalPath = path,
                    Model = CloneForOfflineCatalog(entry.Value)
                });
            }

            PlayerPrefs.SetString(CachedModelsKey, JsonUtility.ToJson(list));
            PlayerPrefs.Save();
        }

        private static void CacheDownloadedModel(BiologicalModel model)
        {
            BiologicalModel copy = CloneForOfflineCatalog(model);
            if (copy == null || string.IsNullOrWhiteSpace(copy.Id))
            {
                return;
            }

            copy.IsBundledModel = false;
            DownloadedModels[copy.Id] = copy;
        }

        private static BiologicalModel CloneForOfflineCatalog(BiologicalModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new BiologicalModel(
                model.Id,
                CloneText(model.Name),
                CloneText(model.Subtitle),
                CloneText(model.Category),
                CloneText(model.Description),
                model.ScientificName,
                model.PrimaryColor,
                model.SecondaryColor,
                model.VisualSeed,
                model.IsElongated,
                model.ModelFileUrl,
                model.PreviewUrl,
                model.IsBundledModel);
        }

        private static LocalizedText CloneText(LocalizedText text)
        {
            if (text == null)
            {
                return new LocalizedText(string.Empty, string.Empty, string.Empty);
            }

            return new LocalizedText(text.Spanish, text.English, text.Portuguese);
        }

        private static BiologicalModel CreateLegacyDownloadedModel(string modelId, string localPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(localPath);
            string displayName = string.IsNullOrWhiteSpace(fileName) ? "Modelo descargado" : fileName;
            return new BiologicalModel(
                modelId,
                new LocalizedText(displayName, displayName, displayName),
                new LocalizedText("Disponible sin conexion", "Available offline", "Disponivel offline"),
                new LocalizedText("Descargados", "Downloaded", "Baixados"),
                new LocalizedText(
                    "Modelo guardado localmente antes de activar la cache offline completa.",
                    "Model saved locally before the full offline cache was enabled.",
                    "Modelo salvo localmente antes de ativar o cache offline completo."),
                displayName,
                new Color(0.35f, 0.78f, 0.96f),
                new Color(0.92f, 0.42f, 0.74f),
                Mathf.Abs(modelId.GetHashCode() % 100),
                false,
                localPath,
                string.Empty,
                false);
        }

        private static string SafeFileName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '-');
            }

            return value;
        }

        private static string ExtensionFromUrl(string url)
        {
            string clean = url.Split('?')[0];
            string extension = Path.GetExtension(clean);
            return string.IsNullOrWhiteSpace(extension) ? ".model" : extension;
        }

        private static string FileNameFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            string clean = url.Split('?')[0];
            string fileName = Path.GetFileName(clean);
            return string.IsNullOrWhiteSpace(fileName) ? string.Empty : fileName;
        }

        public static void DeleteDownloadedModel(string modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                return;
            }

            EnsureLoaded();
            if (DownloadedPaths.TryGetValue(modelId, out string path))
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Failed to delete local model file: " + ex.Message);
                }

                DownloadedPaths.Remove(modelId);
            }

            DownloadedModels.Remove(modelId);
            Save();
        }
    }
}
