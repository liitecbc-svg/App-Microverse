/**
 * UnityMainThreadDispatcher.cs
 *
 * Ejecuta acciones en el hilo principal de Unity para callbacks que llegan desde servicios nativos o procesos asincronos.
 *
 * Main responsibilities:
 * - Mantener una cola segura de acciones pendientes.
 * - Crear una instancia persistente cuando se necesite.
 * - Procesar callbacks durante Update para tocar objetos de Unity de forma segura.
 *
 * Related elements:
 * - AndroidMlKitTranslationService
 * - MlKitTranslationCallback
 * - MicroverseBootstrap
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microverse.Runtime
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> Queue = new Queue<Action>();
        private static UnityMainThreadDispatcher instance;

        public static void Ensure()
        {
            if (instance != null)
            {
                return;
            }

            GameObject go = new GameObject("UnityMainThreadDispatcher");
            instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }

        public static void Enqueue(Action action)
        {
            if (action == null)
            {
                return;
            }

            lock (Queue)
            {
                Queue.Enqueue(action);
            }
        }

        private void Update()
        {
            while (true)
            {
                Action action;
                lock (Queue)
                {
                    if (Queue.Count == 0)
                    {
                        return;
                    }

                    action = Queue.Dequeue();
                }

                action();
            }
        }
    }
}
