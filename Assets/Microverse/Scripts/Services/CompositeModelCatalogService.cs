/**
 * CompositeModelCatalogService.cs
 *
 * Combina el catalogo local, el catalogo remoto y los modelos descargados para presentar una lista unificada de contenido.
 *
 * Main responsibilities:
 * - Cargar primero modelos incluidos para mantener la app util sin conexion.
 * - Mezclar datos remotos sin reemplazar modelos incluidos protegidos.
 * - Reincorporar modelos descargados y sus categorias al iniciar.
 *
 * Related elements:
 * - LocalModelCatalogService
 * - SupabaseModelCatalogService
 * - ModelDownloadStore
 */
using System;
using System.Collections.Generic;
using Microverse.Data;
using UnityEngine;

namespace Microverse.Services
{
    public class CompositeModelCatalogService : IModelCatalogService
    {
        private readonly IModelCatalogService localCatalog;
        private readonly IModelCatalogService remoteCatalog;
        private readonly List<BiologicalModel> cachedModels = new List<BiologicalModel>();
        private readonly List<string> cachedCategories = new List<string>();
        private bool isLoaded;

        public CompositeModelCatalogService(IModelCatalogService localCatalog, IModelCatalogService remoteCatalog)
        {
            this.localCatalog = localCatalog;
            this.remoteCatalog = remoteCatalog;
        }

        public IReadOnlyList<BiologicalModel> GetModels()
        {
            return cachedModels;
        }

        public IReadOnlyList<string> GetCategories()
        {
            return cachedCategories;
        }

        public void LoadModels(Action<IReadOnlyList<BiologicalModel>> onComplete, Action<string> onError)
        {
            if (isLoaded)
            {
                onComplete?.Invoke(cachedModels);
                return;
            }

            localCatalog.LoadModels(
                localModels =>
                {
                    SetCatalog(localModels, localCatalog.GetCategories());
                    MergeDownloadedModels();
                    remoteCatalog.LoadModels(
                        remoteModels =>
                        {
                            MergeRemoteModels(remoteModels, remoteCatalog.GetCategories());
                            MergeDownloadedModels();
                            isLoaded = true;
                            onComplete?.Invoke(cachedModels);
                        },
                        remoteError =>
                        {
                            Debug.LogWarning("Remote model catalog unavailable. Keeping bundled and downloaded models. Details: " + remoteError);
                            isLoaded = true;
                            onComplete?.Invoke(cachedModels);
                        });
                },
                localError =>
                {
                    onError?.Invoke(localError);
                });
        }

        private void SetCatalog(IReadOnlyList<BiologicalModel> models, IReadOnlyList<string> categories)
        {
            cachedModels.Clear();
            cachedCategories.Clear();
            AddModels(models);
            AddCategories(categories);
        }

        private void MergeRemoteModels(IReadOnlyList<BiologicalModel> models, IReadOnlyList<string> categories)
        {
            CacheDownloadedRemoteMetadata(models);
            AddOrReplaceModels(models);
            AddCategories(categories);
        }

        private void MergeDownloadedModels()
        {
            IReadOnlyList<BiologicalModel> downloadedModels = ModelDownloadStore.GetDownloadedModels();
            AddModels(downloadedModels);

            for (int i = 0; i < downloadedModels.Count; i++)
            {
                BiologicalModel model = downloadedModels[i];
                if (model == null || model.Category == null)
                {
                    continue;
                }

                string category = model.Category.Get(MicroverseLanguage.Spanish);
                if (!string.IsNullOrWhiteSpace(category) && !cachedCategories.Contains(category))
                {
                    cachedCategories.Add(category);
                }
            }
        }

        private void AddModels(IReadOnlyList<BiologicalModel> models)
        {
            if (models == null)
            {
                return;
            }

            HashSet<string> knownIds = new HashSet<string>();
            for (int i = 0; i < cachedModels.Count; i++)
            {
                knownIds.Add(cachedModels[i].Id);
            }

            for (int i = 0; i < models.Count; i++)
            {
                BiologicalModel model = models[i];
                if (model == null || string.IsNullOrWhiteSpace(model.Id) || knownIds.Contains(model.Id))
                {
                    continue;
                }

                cachedModels.Add(model);
                knownIds.Add(model.Id);
            }
        }

        private void AddOrReplaceModels(IReadOnlyList<BiologicalModel> models)
        {
            if (models == null)
            {
                return;
            }

            for (int i = 0; i < models.Count; i++)
            {
                BiologicalModel model = models[i];
                if (model == null || string.IsNullOrWhiteSpace(model.Id))
                {
                    continue;
                }

                int existingIndex = IndexOfModel(model.Id);
                if (existingIndex >= 0)
                {
                    if (cachedModels[existingIndex] != null && cachedModels[existingIndex].IsBundledModel && !model.IsBundledModel)
                    {
                        continue;
                    }

                    cachedModels[existingIndex] = model;
                    continue;
                }

                cachedModels.Add(model);
            }
        }

        private int IndexOfModel(string modelId)
        {
            for (int i = 0; i < cachedModels.Count; i++)
            {
                if (cachedModels[i] != null && cachedModels[i].Id == modelId)
                {
                    return i;
                }
            }

            return -1;
        }

        private void CacheDownloadedRemoteMetadata(IReadOnlyList<BiologicalModel> models)
        {
            if (models == null)
            {
                return;
            }

            for (int i = 0; i < models.Count; i++)
            {
                ModelDownloadStore.CacheMetadataIfDownloaded(models[i]);
            }
        }

        private void AddCategories(IReadOnlyList<string> categories)
        {
            if (categories == null)
            {
                return;
            }

            for (int i = 0; i < categories.Count; i++)
            {
                string category = categories[i];
                if (!string.IsNullOrWhiteSpace(category) && !cachedCategories.Contains(category))
                {
                    cachedCategories.Add(category);
                }
            }
        }
    }
}
