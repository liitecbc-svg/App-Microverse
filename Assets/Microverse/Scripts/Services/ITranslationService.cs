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
