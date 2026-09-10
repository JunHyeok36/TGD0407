using UnityEngine;

namespace TDG0407._prototype
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class PixelPerfectCameraConfig : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("How many pixels should 1 Unity unit represent?")]
        public float pixelsPerUnit = 24.3f; // Default based on orthoSize 5 and height 243
        public float targetAspectRatio = 16f / 9f;
        
        [Tooltip("If true, automatically updates global shader variables for PixelArtPostProcess")]
        public bool updateShaderGlobals = true;

        private Camera cam;

        private void OnEnable()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (cam == null || !cam.orthographic) return;

            float orthoSize = cam.orthographicSize;
            
            // Calculate virtual resolution based on PPU and ortho size
            float virtualHeight = Mathf.Round(orthoSize * 2f * pixelsPerUnit);
            float virtualWidth = Mathf.Round(virtualHeight * targetAspectRatio);

            if (updateShaderGlobals)
            {
                Shader.SetGlobalFloat("_GlobalPixelResolutionX", virtualWidth);
                Shader.SetGlobalFloat("_GlobalPixelResolutionY", virtualHeight);
            }
        }
    }
}
