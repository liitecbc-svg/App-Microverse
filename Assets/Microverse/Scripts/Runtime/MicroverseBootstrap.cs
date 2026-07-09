/**
 * MicroverseBootstrap.cs
 *
 * Inicializa la aplicacion Microverse al cargar una escena y asegura que existan los componentes base de UI y eventos.
 *
 * Main responsibilities:
 * - Crear el despachador del hilo principal.
 * - Agregar un EventSystem si la escena no lo tiene.
 * - Instanciar MicroverseApp automaticamente cuando no existe en la escena.
 *
 * Related elements:
 * - MicroverseApp
 * - UnityMainThreadDispatcher
 * - EventSystem
 */
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
