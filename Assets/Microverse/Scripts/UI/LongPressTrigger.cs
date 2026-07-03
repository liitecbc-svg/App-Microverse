using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Microverse.UI
{
    public class LongPressTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Tooltip("Hold duration in seconds to trigger the long press event.")]
        public float duration = 0.8f;
        
        public UnityEvent onLongPress = new UnityEvent();

        private bool isPointerDown = false;
        private float pointerDownTime = 0f;

        private void Update()
        {
            if (isPointerDown)
            {
                if (Time.time - pointerDownTime >= duration)
                {
                    isPointerDown = false;
                    onLongPress?.Invoke();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;
            pointerDownTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerDown = false;
        }
    }
}
