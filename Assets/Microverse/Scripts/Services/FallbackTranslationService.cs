/**
 * FallbackTranslationService.cs
 *
 * Implementa una traduccion de respaldo que conserva el texto original cuando la traduccion automatica no esta disponible.
 *
 * Main responsibilities:
 * - Cumplir el contrato de ITranslationService en editor o plataformas no soportadas.
 * - Responder exitosamente a la preparacion offline sin descargar modelos.
 * - Devolver los textos originales para no bloquear la UI.
 *
 * Related elements:
 * - ITranslationService
 * - AndroidMlKitTranslationService
 * - TranslationRequest
 */
using System;
using System.Collections.Generic;

namespace Microverse.Services
{
    public class FallbackTranslationService : ITranslationService
    {
        public bool IsAutomaticTranslationAvailable
        {
            get { return false; }
        }

        public void PrepareOfflineModels(string sourceLanguage, IReadOnlyList<string> targetLanguages, Action<float, string> onProgress, Action onSuccess, Action<string> onError, Action onCancelled)
        {
            onProgress?.Invoke(1f, string.Empty);
            onSuccess?.Invoke();
        }

        public void CancelOfflineModelPreparation()
        {
        }

        public void TranslateBatch(IReadOnlyList<TranslationRequest> requests, Action<IReadOnlyList<string>> onSuccess, Action<string> onError)
        {
            List<string> values = new List<string>(requests.Count);
            for (int i = 0; i < requests.Count; i++)
            {
                values.Add(requests[i].Text);
            }

            onSuccess(values);
        }
    }
}
