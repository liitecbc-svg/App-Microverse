using UnityEngine;

namespace Microverse.UI
{
    public class ModelManipulator : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private float touchRotationSpeed = 0.4f;
        [SerializeField] private float mouseRotationSpeed = 8.0f;

        [Header("Scale Settings")]
        [SerializeField] private float touchScaleSpeed = 0.005f;
        [SerializeField] private float mouseScaleSpeed = 1.5f;
        [SerializeField] private float minScale = 0.3f;
        [SerializeField] private float maxScale = 3.5f;

        private Vector3 initialScale;

        private void Start()
        {
            initialScale = transform.localScale;
        }

        private void Update()
        {
            HandleTouchInput();
            HandleMouseInput();
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    // Rotate the model in world coordinates relative to touch movement
                    float rotX = touch.deltaPosition.x * touchRotationSpeed;
                    float rotY = touch.deltaPosition.y * touchRotationSpeed;

                    transform.Rotate(Vector3.up, -rotX, Space.World);
                    transform.Rotate(Vector3.right, rotY, Space.World);
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                // Find the position in the previous frame
                Vector2 prevTouch0 = touch0.position - touch0.deltaPosition;
                Vector2 prevTouch1 = touch1.position - touch1.deltaPosition;

                // Calculate the distance between touches in each frame
                float prevDistance = Vector2.Distance(prevTouch0, prevTouch1);
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);

                // Difference in distance
                float delta = currentDistance - prevDistance;

                if (Mathf.Abs(delta) > 0.01f)
                {
                    Vector3 newScale = transform.localScale + initialScale * delta * touchScaleSpeed;
                    
                    // Constrain scale
                    float clampMin = initialScale.x * minScale;
                    float clampMax = initialScale.x * maxScale;
                    newScale.x = Mathf.Clamp(newScale.x, clampMin, clampMax);
                    newScale.y = Mathf.Clamp(newScale.y, clampMin, clampMax);
                    newScale.z = Mathf.Clamp(newScale.z, clampMin, clampMax);

                    transform.localScale = newScale;
                }
            }
        }

        private void HandleMouseInput()
        {
            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            // Left mouse button drag for rotation
            if (Input.GetMouseButton(0))
            {
                // Only rotate if not touching (to avoid double input on touch devices simulating mouse)
                if (Input.touchCount == 0)
                {
                    float rotX = Input.GetAxis("Mouse X") * mouseRotationSpeed;
                    float rotY = Input.GetAxis("Mouse Y") * mouseRotationSpeed;

                    transform.Rotate(Vector3.up, -rotX, Space.World);
                    transform.Rotate(Vector3.right, rotY, Space.World);
                }
            }

            // Mouse wheel scroll for scaling
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                Vector3 newScale = transform.localScale + initialScale * scroll * mouseScaleSpeed;

                // Constrain scale
                float clampMin = initialScale.x * minScale;
                float clampMax = initialScale.x * maxScale;
                newScale.x = Mathf.Clamp(newScale.x, clampMin, clampMax);
                newScale.y = Mathf.Clamp(newScale.y, clampMin, clampMax);
                newScale.z = Mathf.Clamp(newScale.z, clampMin, clampMax);

                transform.localScale = newScale;
            }
            #endif
        }
    }
}
