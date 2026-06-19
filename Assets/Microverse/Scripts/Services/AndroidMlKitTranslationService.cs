using System;
using System.Collections.Generic;
using Microverse.Runtime;
using UnityEngine;

namespace Microverse.Services
{
    public class AndroidMlKitTranslationService : ITranslationService, IDisposable
    {
        private readonly Dictionary<string, string> cache = new Dictionary<string, string>();

#if UNITY_ANDROID && !UNITY_EDITOR
        private readonly AndroidJavaObject bridge;

        public AndroidMlKitTranslationService()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                bridge = new AndroidJavaObject("com.microverse.translation.MlKitTranslatorBridge", activity);
            }
        }

        public bool IsAutomaticTranslationAvailable
        {
            get { return true; }
        }
#else
        private readonly FallbackTranslationService fallback = new FallbackTranslationService();

        public bool IsAutomaticTranslationAvailable
        {
            get { return false; }
        }
#endif

        public void TranslateBatch(IReadOnlyList<TranslationRequest> requests, Action<IReadOnlyList<string>> onSuccess, Action<string> onError)
        {
            if (requests == null || requests.Count == 0)
            {
                onSuccess(new List<string>());
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            List<string> resolved = new List<string>(new string[requests.Count]);
            List<int> pendingIndexes = new List<int>();
            List<TranslationRequest> pendingRequests = new List<TranslationRequest>();

            for (int i = 0; i < requests.Count; i++)
            {
                TranslationRequest request = requests[i];
                if (string.IsNullOrWhiteSpace(request.Text) || request.SourceLanguage == request.TargetLanguage)
                {
                    resolved[i] = request.Text;
                    continue;
                }

                string key = CacheKey(request);
                if (cache.TryGetValue(key, out string cachedValue))
                {
                    resolved[i] = cachedValue;
                    continue;
                }

                pendingIndexes.Add(i);
                pendingRequests.Add(request);
            }

            if (pendingRequests.Count == 0)
            {
                onSuccess(resolved);
                return;
            }

            string[] texts = new string[pendingRequests.Count];
            string[] sources = new string[pendingRequests.Count];
            string[] targets = new string[pendingRequests.Count];

            for (int i = 0; i < pendingRequests.Count; i++)
            {
                texts[i] = pendingRequests[i].Text;
                sources[i] = pendingRequests[i].SourceLanguage;
                targets[i] = pendingRequests[i].TargetLanguage;
            }

            MlKitTranslationCallback callback = new MlKitTranslationCallback(
                translated =>
                {
                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        for (int i = 0; i < translated.Length; i++)
                        {
                            int originalIndex = pendingIndexes[i];
                            TranslationRequest request = pendingRequests[i];
                            resolved[originalIndex] = translated[i];
                            cache[CacheKey(request)] = translated[i];
                        }

                        onSuccess(resolved);
                    });
                },
                error =>
                {
                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        Debug.LogWarning("ML Kit translation failed: " + error);
                        onError(error);
                    });
                });

            bridge.Call("translateBatch", texts, sources, targets, callback);
#else
            fallback.TranslateBatch(requests, onSuccess, onError);
#endif
        }

        private static string CacheKey(TranslationRequest request)
        {
            return request.SourceLanguage + ">" + request.TargetLanguage + "|" + request.Text;
        }

        public void Dispose()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            bridge.Call("close");
#endif
        }
    }
}
