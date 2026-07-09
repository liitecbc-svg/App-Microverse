/**
 * IModelCatalogService.cs
 *
 * Define el contrato comun para fuentes de catalogo de modelos biologicos.
 *
 * Main responsibilities:
 * - Exponer modelos cargados.
 * - Cargar modelos de forma asincrona mediante callbacks.
 * - Entregar categorias asociadas al catalogo.
 *
 * Related elements:
 * - LocalModelCatalogService
 * - SupabaseModelCatalogService
 * - CompositeModelCatalogService
 */
using System;
using System.Collections.Generic;
using Microverse.Data;

namespace Microverse.Services
{
    public interface IModelCatalogService
    {
        IReadOnlyList<BiologicalModel> GetModels();
        void LoadModels(Action<IReadOnlyList<BiologicalModel>> onComplete, Action<string> onError);
        IReadOnlyList<string> GetCategories();
    }
}
