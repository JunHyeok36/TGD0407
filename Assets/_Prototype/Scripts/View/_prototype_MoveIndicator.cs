using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    public class _prototype_MoveIndicator : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        
        private GameObject _nextTickMarker;
        private MeshRenderer _nextTickMarkerRenderer;

        private GameObject _endMarker;
        private MeshRenderer _endMarkerRenderer;
        
        [SerializeField] private float scrollSpeed = -1.0f;

        [Header("Textures")]
        [SerializeField] private Texture2D _dashPatternTexture;
        [SerializeField] private Texture2D _targetMarkerTexture;
        
        public void Initialize(Texture2D dashTex = null, Texture2D markerTex = null)
        {
            if (dashTex != null) _dashPatternTexture = dashTex;
            if (markerTex != null) _targetMarkerTexture = markerTex;

            if (_targetMarkerTexture == null)
            {
#if UNITY_EDITOR
                _targetMarkerTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Prototype/Textures/TargetSquare.png");
#endif
            }

            // Rotate this object so local Z is UP (World Y)
            transform.localRotation = Quaternion.Euler(-90, 0, 0);
            transform.localPosition = Vector3.zero;

            // Clean up existing children if re-initializing
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                SafeDestroy(transform.GetChild(i).gameObject);
            }

            // 1. Single Continuous LineRenderer (no breaks, seamless ribbon)
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null) _lineRenderer = gameObject.AddComponent<LineRenderer>();
            
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.alignment = LineAlignment.TransformZ;
            _lineRenderer.textureMode = LineTextureMode.Stretch;
            _lineRenderer.startWidth = 0.45f;
            _lineRenderer.endWidth = 0.45f;
            _lineRenderer.numCapVertices = 0;
            _lineRenderer.numCornerVertices = 0;

            var xrayShader = Shader.Find("Custom/XRaySpriteUnlit");
            if (xrayShader == null) xrayShader = Shader.Find("Sprites/Default");

            Material lineMat = new Material(xrayShader);
            lineMat.mainTexture = _dashPatternTexture;
            lineMat.color = Color.white;
            _lineRenderer.material = lineMat;

            // 2. Next Tick Marker (Square Marker, scale 0.65, thick border)
            _nextTickMarker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _nextTickMarker.name = "NextTickMarker";
            _nextTickMarker.transform.SetParent(transform, false);
            _nextTickMarker.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
            SafeDestroy(_nextTickMarker.GetComponent<Collider>());

            _nextTickMarkerRenderer = _nextTickMarker.GetComponent<MeshRenderer>();
            Material nextMarkerMat = new Material(xrayShader);
            nextMarkerMat.mainTexture = _targetMarkerTexture;
            Color nextColor = new Color(0.2f, 0.95f, 1f, 0.95f);
            nextMarkerMat.color = nextColor;
            _nextTickMarkerRenderer.material = nextMarkerMat;

            // 3. End Marker (Square Marker, scale 0.90, thick border)
            _endMarker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _endMarker.name = "EndMarker";
            _endMarker.transform.SetParent(transform, false);
            _endMarker.transform.localScale = new Vector3(0.90f, 0.90f, 1f);
            SafeDestroy(_endMarker.GetComponent<Collider>());
            
            _endMarkerRenderer = _endMarker.GetComponent<MeshRenderer>();
            Material markerMat = new Material(xrayShader);
            markerMat.mainTexture = _targetMarkerTexture;
            Color cyanColor = new Color(0.2f, 0.8f, 1f, 0.85f);
            markerMat.color = cyanColor;
            _endMarkerRenderer.material = markerMat;
            
            Hide();
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        private void Update()
        {
            if (_lineRenderer != null && _lineRenderer.enabled && _lineRenderer.material != null)
            {
                float offset = Time.time * scrollSpeed;
                _lineRenderer.material.mainTextureOffset = new Vector2(offset, 0);
            }
        }

        public void ShowPath(List<_prototype_Point> points, int moveDistancePerTick = 1)
        {
            if (points == null || points.Count <= 1)
            {
                Hide();
                return;
            }

            // 1. Position all points on the single continuous line
            _lineRenderer.enabled = true;
            _lineRenderer.positionCount = points.Count;
            _lineRenderer.numCapVertices = 0;
            _lineRenderer.numCornerVertices = 0;
            for (int i = 0; i < points.Count; i++)
            {
                var pointView = _prototype_GridManager.Instance.GetPointView(points[i]);
                Vector3 pos = pointView != null ? pointView.transform.position : new Vector3(points[i].x, 0, points[i].y);
                pos.y += 0.015f;
                _lineRenderer.SetPosition(i, pos);
            }

            int totalSegments = points.Count - 1;
            if (_lineRenderer.material != null)
            {
                _lineRenderer.material.mainTextureScale = new Vector2(totalSegments * 2f, 1);
            }

            int nextTickIndex = Mathf.Min(Mathf.Max(1, moveDistancePerTick), totalSegments);

            // 2. Color Gradient: 1st tick is vibrant Cyan, beyond 1st tick is desaturated Gray
            Color cyan = new Color(0.2f, 0.85f, 1.0f);
            Color gray = new Color(0.65f, 0.65f, 0.65f);
            float cyanAlpha = 0.9f;
            float grayAlpha = 0.45f;

            Gradient gradient = new Gradient();
            if (nextTickIndex >= totalSegments)
            {
                // Full path reachable in next tick
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(cyan, 0.0f), new GradientColorKey(cyan, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(cyanAlpha, 0.0f), new GradientAlphaKey(cyanAlpha, 1.0f) }
                );
            }
            else
            {
                // Multi-segment path: sharp transition at nextTickIndex
                float t = (float)nextTickIndex / totalSegments;
                float t0 = Mathf.Clamp(t - 0.001f, 0.0f, 1.0f);
                float t1 = Mathf.Clamp(t + 0.001f, 0.0f, 1.0f);

                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(cyan, 0.0f),
                        new GradientColorKey(cyan, t0),
                        new GradientColorKey(gray, t1),
                        new GradientColorKey(gray, 1.0f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(cyanAlpha, 0.0f),
                        new GradientAlphaKey(cyanAlpha, t0),
                        new GradientAlphaKey(grayAlpha, t1),
                        new GradientAlphaKey(grayAlpha, 1.0f)
                    }
                );
            }
            _lineRenderer.colorGradient = gradient;

            // 3. Render Next Tick Marker (at nextTickIndex, square Quad)
            _nextTickMarker.SetActive(true);
            var nextPointView = _prototype_GridManager.Instance.GetPointView(points[nextTickIndex]);
            Vector3 nextPos = nextPointView != null ? nextPointView.transform.position : new Vector3(points[nextTickIndex].x, 0, points[nextTickIndex].y);
            _nextTickMarker.transform.position = nextPos + new Vector3(0, 0.022f, 0);
            _nextTickMarker.transform.rotation = Quaternion.Euler(90, 0, 0);

            // 4. Render End Marker (at last index, square Quad)
            _endMarker.SetActive(true);
            int lastIndex = points.Count - 1;
            var endPointView = _prototype_GridManager.Instance.GetPointView(points[lastIndex]);
            Vector3 endPos = endPointView != null ? endPointView.transform.position : new Vector3(points[lastIndex].x, 0, points[lastIndex].y);
            _endMarker.transform.position = endPos + new Vector3(0, 0.020f, 0);
            _endMarker.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        public void Hide()
        {
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            if (_nextTickMarker != null) _nextTickMarker.SetActive(false);
            if (_endMarker != null) _endMarker.SetActive(false);
        }
    }
}
