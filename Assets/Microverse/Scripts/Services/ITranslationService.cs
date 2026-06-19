using System;
using System.Collections.Generic;

namespace Microverse.Services
{
    public interface ITranslationService
    {
        bool IsAutomaticTranslationAvailable { get; }
        void TranslateBatch(IReadOnlyList<TranslationRequest> requests, Action<IReadOnlyList<string>> onSuccess, Action<string> onError);
    }
}
