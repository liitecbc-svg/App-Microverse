using Microverse.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Microverse.Runtime
{
    public static class MicroverseBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartMicroverse()
        {
            UnityMainThreadDispatcher.Ensure();

            if (Object.FindObjectOfType<MicroverseApp>() != null)
            {
                return;
            }

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Object.DontDestroyOnLoad(eventSystem);
            }

            GameObject app = new GameObject("MicroverseApp");
            app.AddComponent<MicroverseApp>();
        }
    }
}
