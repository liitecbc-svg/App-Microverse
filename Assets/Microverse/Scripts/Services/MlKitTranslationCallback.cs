/**
 * MlKitTranslationCallback.cs
 *
 * Adapta los callbacks del puente Android de ML Kit hacia acciones C# usadas por la aplicacion.
 *
 * Main responsibilities:
 * - Recibir traducciones exitosas desde Java/Kotlin.
 * - Propagar errores de traduccion hacia Unity.
 * - Mantener desacoplado el servicio Android del callback nativo.
 *
 * Related elements:
 * - AndroidMlKitTranslationService
 * - ITranslationService
 * - UnityMainThreadDispatcher
 */
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
