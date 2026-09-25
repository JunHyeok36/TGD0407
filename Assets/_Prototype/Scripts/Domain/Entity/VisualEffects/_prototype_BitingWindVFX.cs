using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace TDG0407._prototype
{
    /// <summary>
    /// 렌의 고유 패시브 '칼바람(Biting Wind)' 발동 시 소환되는 N×N 폭풍 시각 효과 컨트롤러.
    /// - Step 1: 지면 참격 소용돌이 폭발 (0.15s)
    /// - Step 2: 상승 나선 회오리(Vortex) 및 에어본 스파크 방출 (0.15s ~ 0.65s)
    /// - N×N 크기 자동 스케일링 지원
    /// </summary>
    public class _prototype_BitingWindVFX : MonoBehaviour
    {
        private static GameObject _cachedPrefab;

        [Header("Settings")]
        [SerializeField] private int _gridSize = 3;
        [SerializeField] private float _duration = 0.65f;
        [SerializeField] private Color _themeColor = new Color(0.2f, 0.85f, 1f, 1f);

        [Header("Components")]
        [SerializeField] private Transform _groundSlashTransform;
        [SerializeField] private ParticleSystem _vortexParticle;
        [SerializeField] private ParticleSystem _sparkParticle;
        [SerializeField] private LineRenderer _boundaryRing;

        public int GridSize => _gridSize;

        public static void SetDefaultPrefab(GameObject prefab)
        {
            _cachedPrefab = prefab;
        }

        /// <summary>
        /// 지정된 월드 위치에 N×N 칼바람 폭풍 VFX를 생성하고 재생합니다.
        /// </summary>
        public static _prototype_BitingWindVFX Spawn(Vector3 worldPos, int gridSize = 3)
        {
            GameObject instanceObj = null;

            if (_cachedPrefab != null)
            {
                instanceObj = Instantiate(_cachedPrefab, worldPos, Quaternion.identity);
            }
            else
            {
                // Resources 폴더 로드 시도
                var resPrefab = Resources.Load<GameObject>("BitingWindStormVFX");
                if (resPrefab != null)
                {
                    _cachedPrefab = resPrefab;
                    instanceObj = Instantiate(resPrefab, worldPos, Quaternion.identity);
                }
            }

            // 프리팹이 아직 등록되지 않은 경우 런타임 프로시저럴 생성 폴백
            if (instanceObj == null)
            {
                instanceObj = CreateProceduralStormObject(worldPos);
            }

            var vfx = instanceObj.GetComponent<_prototype_BitingWindVFX>();
            if (vfx == null)
            {
                vfx = instanceObj.AddComponent<_prototype_BitingWindVFX>();
            }

            vfx.Initialize(gridSize);
            vfx.Play();

            return vfx;
        }

        public void Initialize(int gridSize)
        {
            _gridSize = Mathf.Max(1, gridSize);

            // 3×3(반경 1)을 기본 배율 1.0으로 하고, N×N에 비례하여 Transform 스케일 조정
            float scaleMultiplier = _gridSize / 3.0f;
            transform.localScale = Vector3.one * scaleMultiplier;

            if (_vortexParticle != null)
            {
                var shape = _vortexParticle.shape;
                shape.radius = 1.2f * scaleMultiplier;
            }

            if (_boundaryRing != null)
            {
                SetupBoundaryRing(1.5f * scaleMultiplier);
            }
        }

        public void Play()
        {
            // 카메라 쉐이크 트리거 (타격감)
            _prototype_CameraController.Instance?.ShakeCamera(0.25f, 0.14f);

            // Step 1: 지면 참격 소용돌이 고속 회전 및 펄스
            if (_groundSlashTransform != null)
            {
                _groundSlashTransform.localScale = Vector3.zero;
                _groundSlashTransform.DOScale(Vector3.one * 1.15f, 0.12f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        _groundSlashTransform.DOScale(Vector3.one, 0.08f);
                    });

                // Y축 720도 폭풍 회전
                _groundSlashTransform.DORotate(new Vector3(0, 720, 0), _duration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutCubic);
            }

            // Step 2: 파티클 시스템 재생
            if (_vortexParticle != null)
            {
                _vortexParticle.Play();
            }
            if (_sparkParticle != null)
            {
                _sparkParticle.Play();
            }

            // 바닥 네온 링 페이드
            if (_boundaryRing != null)
            {
                _boundaryRing.enabled = true;
                DOVirtual.Float(1f, 0f, _duration, alpha =>
                {
                    if (_boundaryRing != null)
                    {
                        var startCol = _themeColor;
                        startCol.a = alpha * 0.9f;
                        var endCol = _themeColor;
                        endCol.a = alpha * 0.4f;
                        _boundaryRing.startColor = startCol;
                        _boundaryRing.endColor = endCol;
                    }
                });
            }

            // 지속 시간 후 자동 제거
            Destroy(gameObject, _duration + 0.1f);
        }

        private void SetupBoundaryRing(float radius)
        {
            int segments = 36;
            _boundaryRing.positionCount = segments + 1;
            _boundaryRing.useWorldSpace = false;
            _boundaryRing.startWidth = 0.08f;
            _boundaryRing.endWidth = 0.08f;

            float angle = 0f;
            for (int i = 0; i <= segments; i++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                _boundaryRing.SetPosition(i, new Vector3(x, 0.05f, z));
                angle += 360f / segments;
            }
        }

        /// <summary>
        /// 프리팹 에셋이 없을 때도 즉시 화려한 시각 효과가 발동되도록 하는 절차적 메쉬/파티클 생성기
        /// </summary>
        private static GameObject CreateProceduralStormObject(Vector3 worldPos)
        {
            var root = new GameObject("BitingWindStormVFX_Procedural");
            root.transform.position = worldPos;

            // 1. 지면 참격 메쉬 (Flat Quad / Circle)
            var slashObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            slashObj.name = "GroundSlashMesh";
            slashObj.transform.SetParent(root.transform);
            slashObj.transform.localPosition = new Vector3(0, 0.05f, 0);
            slashObj.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            slashObj.transform.localScale = new Vector3(3f, 3f, 1f);

            // Collider 제거
            var col = slashObj.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Material 구성
            var slashRenderer = slashObj.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            var mat = new Material(shader);
            mat.color = new Color(0.2f, 0.85f, 1f, 0.7f);
            slashRenderer.material = mat;

            // 2. 바닥 경계선 링
            var lineObj = new GameObject("BoundaryRing");
            lineObj.transform.SetParent(root.transform);
            lineObj.transform.localPosition = Vector3.zero;
            var lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(shader);

            // 3. 수직 회오리 파티클
            var particleObj = new GameObject("VortexParticle");
            particleObj.transform.SetParent(root.transform);
            particleObj.transform.localPosition = Vector3.zero;
            var ps = particleObj.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.45f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            main.startColor = new Color(0.4f, 0.95f, 1f, 0.9f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 80;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 1.2f;

            var velOverLifetime = ps.velocityOverLifetime;
            velOverLifetime.enabled = true;
            velOverLifetime.orbitalY = 8f; // 나선 회전
            velOverLifetime.y = 5f; // 상공 분출

            var psRenderer = particleObj.GetComponent<ParticleSystemRenderer>();
            psRenderer.material = new Material(shader);
            psRenderer.material.color = new Color(0.3f, 0.9f, 1f, 0.8f);

            var vfxComponent = root.AddComponent<_prototype_BitingWindVFX>();
            vfxComponent._groundSlashTransform = slashObj.transform;
            vfxComponent._boundaryRing = lineRenderer;
            vfxComponent._vortexParticle = ps;

            return root;
        }
    }
}
