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
