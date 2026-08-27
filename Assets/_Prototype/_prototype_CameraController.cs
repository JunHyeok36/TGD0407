using UnityEngine;
using UnityEngine.InputSystem;

namespace TDG0407._prototype
{
    
    public class _prototype_CameraController : MonoBehaviour
    {
        public static _prototype_CameraController Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform cameraTarget;

        [Header("Settings")]
        [SerializeField] private bool isCameraLocked = true;
        [SerializeField] [ReadOnly] private float edgeScrollBoundary = 20f;
        [SerializeField] [ReadOnly] private float cameraMoveSpeed = 12f;
        [SerializeField] [ReadOnly] private float cameraRotateSpeed = 180f;

        private float targetYRotation = 0f;
        private bool isRotating = false;

        public Camera MainCamera { get { return mainCamera; } }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void LateUpdate()
        {
            if (Keyboard.current == null || Mouse.current == null) return;

            HandleCameraLookToggle();
            HandleCameraMovement();
            HandleCameraRotation();
        }

        private void HandleCameraLookToggle()
        {
            if (Keyboard.current[Key.Space].wasPressedThisFrame)
            {
                if(isCameraLocked = !isCameraLocked)
                    transform.position = cameraTarget.position;
            }
        }

        private Vector3 rawPosition;
        private bool isInitialized = false;

        private void InitializeIfNeeded()
        {
            if (!isInitialized)
            {
                rawPosition = transform.position;
                targetYRotation = Mathf.Round(transform.eulerAngles.y / 45f) * 45f;
                isInitialized = true;
            }
        }

        private void HandleCameraMovement()
        {
            if (cameraTarget == null) return;
            InitializeIfNeeded();

            if (isCameraLocked)
            {
                Vector3 targetPosition = cameraTarget.position;
                rawPosition = Vector3.MoveTowards(rawPosition, targetPosition, cameraMoveSpeed * Time.unscaledDeltaTime);
            }
            else
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                Vector3 cameraForward = mainCamera.transform.forward; cameraForward.y = 0f;
                Vector3 cameraRight = mainCamera.transform.right; cameraRight.y = 0f;

                cameraForward.Normalize();
                cameraRight.Normalize();

                Vector3 moveDirection = Vector3.zero;
                if (mousePosition.x < edgeScrollBoundary)
                    moveDirection -= cameraRight;
                else if (mousePosition.x > Screen.width - edgeScrollBoundary)
                    moveDirection += cameraRight;
                if (mousePosition.y < edgeScrollBoundary)
                    moveDirection -= cameraForward; 
                else if (mousePosition.y > Screen.height - edgeScrollBoundary)
                    moveDirection += cameraForward;

                if (moveDirection != Vector3.zero)
                    rawPosition += cameraMoveSpeed * Time.unscaledDeltaTime * moveDirection.normalized;
            }

            // Apply Pixel Snapping along the camera's local plane to prevent jitter
            if (mainCamera.orthographic)
            {
                // Note: Set this to match your PixelArtPostProcess _PixelResolutionY
                float virtualHeight = 243f; 
                float orthoSize = mainCamera.orthographicSize;
                float pixelSize = (orthoSize * 2f) / virtualHeight;

                // Project raw position onto camera right/up vectors and snap
                float rightOffset = Vector3.Dot(rawPosition, mainCamera.transform.right);
                float upOffset = Vector3.Dot(rawPosition, mainCamera.transform.up);
                float forwardOffset = Vector3.Dot(rawPosition, mainCamera.transform.forward);

                float snappedRight = Mathf.Round(rightOffset / pixelSize) * pixelSize;
                float snappedUp = Mathf.Round(upOffset / pixelSize) * pixelSize;

                // Reconstruct world position
                transform.position = mainCamera.transform.right * snappedRight + 
                                     mainCamera.transform.up * snappedUp + 
                                     mainCamera.transform.forward * forwardOffset;
            }
            else
            {
                transform.position = rawPosition;
            }
        }


        private void HandleCameraRotation()
        {
            // Discrete 45-degree rotation steps per press
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                targetYRotation += 45f;
            }
            else if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                targetYRotation -= 45f;
            }

            // Smoothly rotate towards the target rotation
            float currentAngle = transform.eulerAngles.y;
            float smoothedAngle = Mathf.LerpAngle(currentAngle, targetYRotation, Time.unscaledDeltaTime * 15f);
            
            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
        }
    
    }

}