/**
 * FavoriteModelsStore.cs
 *
 * Guarda y consulta los modelos marcados como favoritos por el usuario usando PlayerPrefs.
 *
 * Main responsibilities:
 * - Cargar IDs favoritos de forma diferida.
 * - Alternar el estado favorito de un modelo.
 * - Persistir la seleccion para futuras sesiones.
 *
 * Related elements:
 * - ModelCardView
 * - HomeScreenView
 * - PlayerPrefs
 */
using System.Collections.Generic;
using UnityEngine;

namespace Microverse.Services
{
    public static class FavoriteModelsStore
    {
        private const string PlayerPrefsKey = "microverse.favorite_model_ids";
        private static readonly HashSet<string> FavoriteIds = new HashSet<string>();
        private static bool loaded;

        public static bool IsFavorite(string modelId)
        {
            EnsureLoaded();
            return !string.IsNullOrWhiteSpace(modelId) && FavoriteIds.Contains(modelId);
        }

        public static void Toggle(string modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                return;
            }

            EnsureLoaded();
            if (FavoriteIds.Contains(modelId))
            {
                FavoriteIds.Remove(modelId);
            }
            else
            {
                FavoriteIds.Add(modelId);
            }

            Save();
        }

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;
            FavoriteIds.Clear();

            string raw = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            string[] ids = raw.Split('\n');
            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i].Trim();
                if (!string.IsNullOrWhiteSpace(id))
                {
                    FavoriteIds.Add(id);
                }
            }
        }

        private static void Save()
        {
            List<string> ids = new List<string>(FavoriteIds);
            PlayerPrefs.SetString(PlayerPrefsKey, string.Join("\n", ids.ToArray()));
            PlayerPrefs.Save();
        }
    }
}
