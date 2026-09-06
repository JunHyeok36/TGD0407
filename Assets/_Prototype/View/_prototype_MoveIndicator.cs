using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    public class _prototype_MoveIndicator : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        private GameObject _endMarker;
        private MeshRenderer _endMarkerRenderer;
        
        [SerializeField] private float scrollSpeed = -1.0f;
        
        private void Awake()
        {
            // Rotate this object so local Z is UP (World Y)
            transform.localRotation = Quaternion.Euler(-90, 0, 0);

            // Setup LineRenderer
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.alignment = LineAlignment.TransformZ;
            _lineRenderer.textureMode = LineTextureMode.Stretch;
            _lineRenderer.startWidth = 0.4f;
            _lineRenderer.endWidth = 0.4f;
            _lineRenderer.numCapVertices = 0;
            _lineRenderer.numCornerVertices = 0;
            
            // Material for Line
            Material lineMat = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
            Texture2D dashTex = Resources.Load<Texture2D>("Textures/DashPattern");
            lineMat.mainTexture = dashTex;
            // LineRenderer uses vertex colors. Alpha Blended shader multiplies by _TintColor * 2. 
            // So setting _TintColor to 0.5 makes it 1.0 (exact match).
            lineMat.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
            _lineRenderer.material = lineMat;
            
            Color cyanColor = new Color(0.2f, 0.8f, 1f, 0.8f);
            
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(cyanColor, 0.0f), new GradientColorKey(cyanColor, 1.0f) },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.0f, 0.0f), 
                    new GradientAlphaKey(cyanColor.a, 0.2f), 
                    new GradientAlphaKey(cyanColor.a, 1.0f) 
                }
            );
            _lineRenderer.colorGradient = gradient;
            
            // Setup End Marker
            _endMarker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _endMarker.name = "EndMarker";
            _endMarker.transform.SetParent(transform);
            _endMarker.transform.localRotation = Quaternion.Euler(90, 0, 0); // Flat on ground
            _endMarker.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            Destroy(_endMarker.GetComponent<Collider>());
            
            _endMarkerRenderer = _endMarker.GetComponent<MeshRenderer>();
            Material markerMat = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
            Texture2D circleTex = Resources.Load<Texture2D>("Textures/TargetCircle");
            markerMat.mainTexture = circleTex;
            // Legacy Shaders/Particles/Alpha Blended supports _TintColor and multiplies by 2.
            markerMat.SetColor("_TintColor", new Color(cyanColor.r * 0.5f, cyanColor.g * 0.5f, cyanColor.b * 0.5f, cyanColor.a * 0.5f));
            _endMarkerRenderer.material = markerMat;
            
            Hide();
        }

        private void Update()
        {
            if (_lineRenderer.enabled && _lineRenderer.material != null)
            {
                float offset = Time.time * scrollSpeed;
                _lineRenderer.material.mainTextureOffset = new Vector2(offset, 0);
            }
        }

        public void ShowPath(List<_prototype_Point> points)
        {
            if (points == null || points.Count == 0)
            {
                Hide();
                return;
            }

            _lineRenderer.enabled = true;
            _endMarker.SetActive(true);
            
            _lineRenderer.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                var pointView = _prototype_GridManager.Instance.GetPointView(points[i]);
                Vector3 pos = pointView != null ? pointView.transform.position : new Vector3(points[i].x, 0, points[i].y);
                pos.y += 0.05f; // Slightly above ground
                _lineRenderer.SetPosition(i, pos);
                
                if (i == points.Count - 1)
                {
                    _endMarker.transform.position = pos + new Vector3(0, 0.01f, 0);
                    _endMarker.transform.rotation = Quaternion.Euler(90, 0, 0);
                }
            }

            if (_lineRenderer.material != null && points.Count > 1)
            {
                // Each grid cell is 1 unit, so total length is roughly points.Count - 1
                _lineRenderer.material.mainTextureScale = new Vector2(points.Count - 1, 1);
            }
        }

        public void Hide()
        {
            _lineRenderer.enabled = false;
            _endMarker.SetActive(false);
        }
    }
}
