using System;
using UnityEngine;

namespace Microverse.Services
{
    public class MlKitTranslationCallback : AndroidJavaProxy
    {
        private readonly Action<string[]> successHandler;
        private readonly Action<string> errorHandler;

        public MlKitTranslationCallback(Action<string[]> onSuccess, Action<string> onError)
            : base("com.microverse.translation.MlKitTranslationCallback")
        {
            successHandler = onSuccess;
            errorHandler = onError;
        }

        public void onSuccess(string[] translations)
        {
            successHandler(translations);
        }

        public void onError(string message)
        {
            errorHandler(message);
        }
    }
}
