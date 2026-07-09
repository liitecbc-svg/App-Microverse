/**
 * PreviewImageStore.cs
 *
 * Administra la cache local de imagenes preview para que los modelos descargados mantengan su vista previa sin conexion.
 *
 * Main responsibilities:
 * - Guardar previews remotas como archivos PNG persistentes.
 * - Cargar sprites desde rutas locales o referencias cacheadas.
 * - Evitar descargas repetidas cuando ya existe una preview local.
 *
 * Related elements:
 * - BiologyVisualFactory
 * - ModelDownloadStore
 * - Application.persistentDataPath
 */
using System.Collections;
using System.IO;
using Microverse.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Microverse.Services
{
    public static class PreviewImageStore
    {
        private const string DirectoryName = "MicroversePreviews";
        private const string FilePrefix = "file:";

        public static bool TryLoadSprite(string previewReference, out Sprite sprite)
        {
            sprite = null;
            string localPath = LocalPathForReference(previewReference);
            if (string.IsNullOrWhiteSpace(localPath) || !File.Exists(localPath))
            {
                return false;
            }

            byte[] bytes = File.ReadAllBytes(localPath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes))
            {
                Object.Destroy(texture);
                return false;
            }

            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return true;
        }

        public static bool HasCachedPreview(string previewReference)
        {
            string localPath = LocalPathForReference(previewReference);
            return !string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath);
        }

        public static IEnumerator DownloadPreviewRoutine(BiologicalModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.PreviewUrl) || !IsRemoteUrl(model.PreviewUrl) || HasCachedPreview(model.PreviewUrl))
            {
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(model.PreviewUrl))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("[Supabase] Failed to download preview image (" + model.PreviewUrl + "): " + request.error);
                    yield break;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    SaveTexture(model.PreviewUrl, texture);
                }
            }
        }

        public static void SaveTexture(string previewReference, Texture2D texture)
        {
            if (string.IsNullOrWhiteSpace(previewReference) || texture == null || !IsRemoteUrl(previewReference))
            {
                return;
            }

            try
            {
                string localPath = LocalPathForReference(previewReference);
                if (string.IsNullOrWhiteSpace(localPath))
                {
                    return;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                File.WriteAllBytes(localPath, texture.EncodeToPNG());
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Preview image could not be saved for offline use. " + ex.Message);
            }
        }

        private static string LocalPathForReference(string previewReference)
        {
            if (string.IsNullOrWhiteSpace(previewReference))
            {
                return string.Empty;
            }

            if (previewReference.StartsWith(FilePrefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return previewReference.Substring(FilePrefix.Length);
            }

            if (File.Exists(previewReference))
            {
                return previewReference;
            }

            if (!IsRemoteUrl(previewReference))
            {
                return string.Empty;
            }

            string directory = Path.Combine(Application.persistentDataPath, DirectoryName);
            return Path.Combine(directory, StableHash(previewReference) + ".png");
        }

        private static bool IsRemoteUrl(string value)
        {
            return value.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase);
        }

        private static string StableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 16777619;
                }

                return hash.ToString("x8");
            }
        }
    }
}
