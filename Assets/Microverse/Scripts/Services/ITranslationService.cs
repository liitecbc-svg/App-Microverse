/**
 * ITranslationService.cs
 *
 * Define el contrato para preparar modelos de traduccion y traducir contenido de la aplicacion.
 *
 * Main responsibilities:
 * - Indicar si existe traduccion automatica real.
 * - Preparar modelos offline antes de cambiar de idioma.
 * - Traducir lotes de textos manteniendo callbacks de exito y error.
 *
 * Related elements:
 * - AndroidMlKitTranslationService
 * - FallbackTranslationService
 * - TranslationRequest
 */
using System;
using System.Collections.Generic;

namespace Microverse.Services
{
    public interface ITranslationService
    {
        bool IsAutomaticTranslationAvailable { get; }
        void PrepareOfflineModels(string sourceLanguage, IReadOnlyList<string> targetLanguages, Action<float, string> onProgress, Action onSuccess, Action<string> onError, Action onCancelled);
        void CancelOfflineModelPreparation();
        void TranslateBatch(IReadOnlyList<TranslationRequest> requests, Action<IReadOnlyList<string>> onSuccess, Action<string> onError);
    }
}
