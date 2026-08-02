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

        private void HandleCameraMovement()
        {
            if (cameraTarget == null) return;

            if (isCameraLocked)
            {
                Vector3 targetPosition = cameraTarget.position;
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, cameraMoveSpeed * Time.unscaledDeltaTime);
                transform.position = smoothedPosition;
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
                    transform.Translate(cameraMoveSpeed * Time.unscaledDeltaTime * moveDirection.normalized, Space.World);
            }
        }

        private void HandleCameraRotation()
        {
            if (Keyboard.current.qKey.isPressed)
            {
                isRotating = true;
                transform.Rotate(Vector3.up, cameraRotateSpeed * Time.deltaTime);
                
                targetYRotation = transform.eulerAngles.y;
            }
            else if (Keyboard.current.eKey.isPressed)
            {
                isRotating = true;
                transform.Rotate(Vector3.up, -cameraRotateSpeed * Time.deltaTime);
                
                targetYRotation = transform.eulerAngles.y;
            }
            else
            {
                if (isRotating)
                {
                    float currentY = transform.eulerAngles.y;
                    targetYRotation = Mathf.Round(currentY / 45f) * 45f;
                    
                    isRotating = false;
                }

                float currentAngle = transform.eulerAngles.y;
                float smoothedAngle = Mathf.LerpAngle(currentAngle, targetYRotation, Time.unscaledDeltaTime * 10f);
                
                transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
            }
        }
    
    }

}