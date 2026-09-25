using UnityEngine;
using TMPro;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using DG.Tweening;
using TDG0407.Domain;

namespace TDG0407._prototype
{
    [RequireComponent(typeof(_prototype_LifeView))]
    public class _prototype_LifeHUD : MonoBehaviour
    {
        private _prototype_LifeView _lifeView;

        private Transform _hudRoot;
        private SpriteRenderer _frameBg;
        private SpriteRenderer _hpBg;
        private SpriteRenderer _hpGhostFill;
        private SpriteRenderer _hpFill;
        private SpriteRenderer _shieldFill;
        private SpriteRenderer _spBg;
        private SpriteRenderer _spFill;
        private TextMeshPro _hpText;
        private System.IDisposable _shieldSub;
        private DG.Tweening.Tween _ghostTween;
        private float _lastHpRatio = -1f;

        // ── 상태효과 아이콘 트레이 ──
        private Transform _statusTrayRoot;
        private readonly List<(GameObject go, TextMeshPro label, SpriteRenderer icon, SpriteRenderer bg)> _statusBadges = new();

        // ── 적 의도 배지 ──
        private TextMeshPro _intentText;
        private SpriteRenderer _intentBg;
        private Transform _intentBadge;

        // ── 레거시 Status_Text (하위호환 — 없어도 무방) ──
        private TextMeshPro _statusText;

        // ── HP 눈금 ──
        private readonly List<GameObject> _tickMarks = new();
        private int _lastTickMaxHp = -1;

        [Header("Prefab")]
        [SerializeField] private GameObject _hudPrefab;
        private static GameObject s_defaultHudPrefab;

        public static void SetDefaultPrefab(GameObject prefab)
        {
            s_defaultHudPrefab = prefab;
        }

        private bool _isInitialized = false;

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _lifeView = GetComponent<_prototype_LifeView>();
            InitializeHUD();
            SetupEvents();
        }

        private void InitializeHUD()
        {
            GameObject prefab = _hudPrefab != null ? _hudPrefab : s_defaultHudPrefab;
            if (prefab == null && _prototype_PlayerUIView.Instance != null)
            {
                prefab = _prototype_PlayerUIView.Instance.LifeHudPrefab;
            }
            if (prefab == null)
            {
                Debug.LogError($"LifeHUD prefab is not assigned for {gameObject.name}! Please assign it on the component or BootStrapper.");
                return;
            }

            GameObject hudInstance = Instantiate(prefab);
            hudInstance.name = "LifeHUD_" + gameObject.name;
            _hudRoot = hudInstance.transform;

            _hpBg = _hudRoot.Find("HP_BG")?.GetComponent<SpriteRenderer>();
            _spBg = _hudRoot.Find("SP_BG")?.GetComponent<SpriteRenderer>();
            _hpFill = _hudRoot.Find("HP_Fill")?.GetComponent<SpriteRenderer>();
            _spFill = _hudRoot.Find("SP_Fill")?.GetComponent<SpriteRenderer>();
            _shieldFill = _hudRoot.Find("Shield_Fill")?.GetComponent<SpriteRenderer>();

            Material overlayMat = _hpFill != null ? _hpFill.sharedMaterial : null;

            // ── 1. 다크 외곽 프레임 박스 ──
            var frameTransform = _hudRoot.Find("Frame_BG");
            if (frameTransform != null)
            {
                _frameBg = frameTransform.GetComponent<SpriteRenderer>();
            }
            else
            {
                var frameObj = new GameObject("Frame_BG");
                frameObj.transform.SetParent(_hudRoot, false);
                frameObj.transform.localPosition = new Vector3(0f, -0.005f, 0.02f);
                frameObj.transform.localScale = new Vector3(k_BarWidth + 0.04f, 0.155f, 1f);
                _frameBg = frameObj.AddComponent<SpriteRenderer>();
                _frameBg.sprite = GetSquareSprite();
                _frameBg.color = new Color(0.04f, 0.04f, 0.06f, 0.95f);
                if (overlayMat != null) _frameBg.sharedMaterial = overlayMat;
                _frameBg.sortingLayerID = _hpFill != null ? _hpFill.sortingLayerID : 0;
                _frameBg.sortingOrder = 99;
            }

            // ── 2. HP 트랙 (배경) ──
            if (_hpBg != null)
            {
                _hpBg.transform.localPosition = new Vector3(k_BarLeft, 0.02f, 0.01f);
                _hpBg.transform.localScale = new Vector3(k_BarWidth, 0.075f, 1f);
                _hpBg.color = new Color(0.24f, 0.05f, 0.06f, 0.85f);
                _hpBg.sortingOrder = 100;
            }

            // ── 3. HP 고스트 잔상 바 ──
            var ghostTransform = _hudRoot.Find("HP_Ghost");
            if (ghostTransform != null)
            {
                _hpGhostFill = ghostTransform.GetComponent<SpriteRenderer>();
            }
            else
            {
                var ghostObj = new GameObject("HP_Ghost");
                ghostObj.transform.SetParent(_hudRoot, false);
                ghostObj.transform.localPosition = new Vector3(k_BarLeft, 0.02f, 0.005f);
                ghostObj.transform.localScale = new Vector3(k_BarWidth, 0.075f, 1f);
                _hpGhostFill = ghostObj.AddComponent<SpriteRenderer>();
                if (_hpFill != null)
                {
                    _hpGhostFill.sprite = _hpFill.sprite;
                    _hpGhostFill.sharedMaterial = overlayMat;
                    _hpGhostFill.sortingLayerID = _hpFill.sortingLayerID;
                }
            }
            if (_hpGhostFill != null)
            {
                _hpGhostFill.color = new Color(1f, 1f, 1f, 0.95f);
                _hpGhostFill.sortingOrder = 101;
            }

            // ── 4. HP 채움 바 ──
            if (_hpFill != null)
            {
                _hpFill.transform.localPosition = new Vector3(k_BarLeft, 0.02f, 0f);
                _hpFill.transform.localScale = new Vector3(k_BarWidth, 0.075f, 1f);
                _hpFill.color = new Color(0.95f, 0.2f, 0.22f, 1f);
                _hpFill.sortingOrder = 102;
            }

            // ── 5. 보호막 바 ──
            if (_shieldFill == null && _hpFill != null)
            {
                var shieldObj = new GameObject("Shield_Fill");
                shieldObj.transform.SetParent(_hudRoot, false);
                _shieldFill = shieldObj.AddComponent<SpriteRenderer>();
                _shieldFill.sprite = _hpFill.sprite;
                _shieldFill.sharedMaterial = overlayMat;
                shieldObj.SetActive(false);
            }
            if (_shieldFill != null)
            {
                _shieldFill.transform.localPosition = new Vector3(k_BarLeft, 0.02f, -0.005f);
                _shieldFill.color = new Color(0.25f, 0.82f, 1f, 0.95f);
                _shieldFill.sortingLayerID = _hpFill != null ? _hpFill.sortingLayerID : 0;
                _shieldFill.sortingOrder = 103;
            }

            // ── 6. SP 트랙 (배경) ──
            if (_spBg != null)
            {
                _spBg.transform.localPosition = new Vector3(k_BarLeft, -0.035f, 0.01f);
                _spBg.transform.localScale = new Vector3(k_BarWidth, 0.03f, 1f);
                _spBg.color = new Color(0.04f, 0.12f, 0.22f, 0.85f);
                _spBg.sortingOrder = 100;
            }

            // ── 7. SP 채움 바 ──
            if (_spFill != null)
            {
                _spFill.transform.localPosition = new Vector3(k_BarLeft, -0.035f, 0f);
                _spFill.transform.localScale = new Vector3(k_BarWidth, 0.03f, 1f);
                _spFill.color = new Color(0.12f, 0.68f, 0.98f, 1f);
                _spFill.sortingOrder = 102;
            }

            // 레거시 Status_Text 및 HUD_Text 숨김
            var statusTextObj = _hudRoot.Find("Status_Text");
            if (statusTextObj != null)
            {
                _statusText = statusTextObj.GetComponent<TextMeshPro>();
                if (_statusText != null) _statusText.text = "";
                statusTextObj.gameObject.SetActive(false);
            }

            var hpTextObj = _hudRoot.Find("HUD_Text");
            if (hpTextObj != null)
            {
                _hpText = hpTextObj.GetComponent<TextMeshPro>();
                if (_hpText != null) _hpText.text = "";
                hpTextObj.gameObject.SetActive(false);
            }

            // ── 상태효과 아이콘 트레이 컨테이너 바인딩 ──
            var statusTrayTransform = _hudRoot.Find("StatusIconTray");
            _statusTrayRoot = statusTrayTransform != null ? statusTrayTransform : CreateStatusTrayContainer();

            // ── 적 의도 배지 바인딩 ──
            var intentTransform = _hudRoot.Find("IntentBadge");
            if (intentTransform != null)
            {
                _intentBadge = intentTransform;
                _intentBg = _intentBadge.Find("IntentBg")?.GetComponent<SpriteRenderer>();
                _intentText = _intentBadge.Find("IntentText")?.GetComponent<TextMeshPro>();
                _intentBadge.gameObject.SetActive(false);
            }
            else
            {
                CreateIntentBadge();
            }

            // ── HP 눈금 초기 생성 ──
            if (_lifeView?.Data != null)
                BuildTickMarks(_lifeView.Data.health.Max);

            UpdateHUD();
        }

        private Transform CreateStatusTrayContainer()
        {
            var tray = new GameObject("StatusIconTray");
            tray.transform.SetParent(_hudRoot, false);
            // SP 바 아래쪽에 위치
            tray.transform.localPosition = new Vector3(0f, -0.16f, 0f);
            return tray.transform;
        }

        // ── HP 눈금 생성 ──────────────────────────────────────────
        // HP 바 전체 너비: 0.8f 유닛 (max HP 기준)
        // HP_Fill pivot: 왼쪽(0 pivot) 기준, 부모 origin에서 왼쪽으로 -0.4f 시작
        // 눈금 위치 = barLeft + (tickValue / maxHp) * barWidth
        private const float k_BarWidth = 0.8f;        // HP 바 전체 너비 (유닛)
        private const float k_BarLeft = -0.4f;        // 바 왼쪽 끝 local X (HP_Fill 부모 기준)

        private void BuildTickMarks(int maxHp)
        {
            _lastTickMaxHp = maxHp;

            // 기존 눈금 오브젝트 제거
            foreach (var go in _tickMarks)
                if (go != null) Destroy(go);
            _tickMarks.Clear();

            if (_hpFill == null || maxHp <= 0) return;

            int unit = CalculateSmartTickInterval(maxHp);
            if (unit <= 0) return;

            Transform parent = _hpFill.transform.parent;
            var container = new GameObject("HP_TickContainer");
            container.transform.SetParent(parent, false);
            _tickMarks.Add(container);

            Sprite squareSprite = GetSquareSprite();
            Material overlayMat = _hpFill.sharedMaterial;

            float fillY = 0.02f;
            float fillZ = -0.008f;

            for (int val = unit; val < maxHp; val += unit)
            {
                float ratio = (float)val / maxHp;
                float xPos  = k_BarLeft + ratio * k_BarWidth;

                var tickGo = new GameObject($"Tick_{unit}_{val}");
                tickGo.transform.SetParent(container.transform, false);
                tickGo.transform.localPosition = new Vector3(xPos, fillY, fillZ);
                tickGo.transform.localScale    = new Vector3(0.008f, 0.075f, 1f);

                var sr = tickGo.AddComponent<SpriteRenderer>();
                sr.sprite         = squareSprite;
                sr.color          = new Color(0.05f, 0.05f, 0.08f, 0.75f);
                sr.sharedMaterial = overlayMat;
                sr.sortingLayerID = _hpFill.sortingLayerID;
                sr.sortingOrder   = 104;
            }
        }

        private static int CalculateSmartTickInterval(int maxHp)
        {
            if (maxHp <= 75) return 25;
            if (maxHp <= 150) return 25;
            if (maxHp <= 350) return 50;
            if (maxHp <= 800) return 100;
            if (maxHp <= 2000) return 250;
            if (maxHp <= 5000) return 500;
            return 1000;
        }

        private void CreateIntentBadge()
        {
            _intentBadge = new GameObject("IntentBadge").transform;
            _intentBadge.SetParent(_hudRoot, false);
            // HUD 상단 (HP 바 바로 위)
            _intentBadge.localPosition = new Vector3(0f, 0.18f, 0f);

            // 배지 배경
            var bgGo = new GameObject("IntentBg");
            bgGo.transform.SetParent(_intentBadge, false);
            _intentBg = bgGo.AddComponent<SpriteRenderer>();
            _intentBg.sprite = GetSquareSprite();
            _intentBg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);
            _intentBg.sortingLayerID = _hpFill != null ? _hpFill.sortingLayerID : 0;
            _intentBg.sortingOrder = 110;
            bgGo.transform.localScale = new Vector3(0.6f, 0.26f, 1f);

            // 배지 텍스트 (단일 라인으로 깔끔하게 표시)
            var textGo = new GameObject("IntentText");
            textGo.transform.SetParent(_intentBadge, false);
            _intentText = textGo.AddComponent<TextMeshPro>();

            // 폰트 에셋 상속 (neodgm_SDF_TMPro 등 _hpText와 동일 폰트 적용)
            if (_hpText != null && _hpText.font != null)
            {
                _intentText.font = _hpText.font;
                _intentText.fontSharedMaterial = _hpText.fontSharedMaterial;
            }

            _intentText.fontSize = 2.1f;
            _intentText.alignment = TextAlignmentOptions.Center;
            _intentText.textWrappingMode = TextWrappingModes.NoWrap;
            _intentText.sortingOrder = 21;
            _intentText.text = "";

            var overlayShader = Shader.Find("TextMeshPro/Distance Field Overlay");
            if (overlayShader != null)
                _intentText.fontMaterial.shader = overlayShader;

            _intentBadge.gameObject.SetActive(false);
        }

        private static Sprite _squareSprite;
        private static Sprite GetSquareSprite()
        {
            if (_squareSprite == null)
            {
                var tex = new Texture2D(4, 4);
                var cols = new Color[16];
                for (int i = 0; i < cols.Length; i++) cols[i] = Color.white;
                tex.SetPixels(cols);
                tex.Apply();
                _squareSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return _squareSprite;
        }

        public void SetupEvents()
        {
            if (_lifeView == null)
                _lifeView = GetComponent<_prototype_LifeView>();

            if (_lifeView != null && _lifeView.Data != null)
            {
                _lifeView.Data.health.OnValueChanged -= UpdateHUD;
                _lifeView.Data.stamina.OnValueChanged -= UpdateHUD;
                _lifeView.Data.health.OnValueChanged += UpdateHUD;
                _lifeView.Data.stamina.OnValueChanged += UpdateHUD;

                _shieldSub?.Dispose();
                _shieldSub = _prototype_EventBus.Listen<EntityShieldChangedEvent>(evt =>
                {
                    if (_lifeView != null && evt.Entity == _lifeView.Data)
                    {
                        UpdateHUD();
                    }
                });

                UpdateHUD();
            }
        }

        private void OnDisable()
        {
            if (_ghostTween != null && _ghostTween.IsActive())
            {
                _ghostTween.Kill();
            }
        }

        private void OnDestroy()
        {
            if (_lifeView != null && _lifeView.Data != null)
            {
                _lifeView.Data.health.OnValueChanged -= UpdateHUD;
                _lifeView.Data.stamina.OnValueChanged -= UpdateHUD;
            }
            _shieldSub?.Dispose();
            if (_ghostTween != null && _ghostTween.IsActive())
            {
                _ghostTween.Kill();
            }
            if (_hudRoot != null)
            {
                Destroy(_hudRoot.gameObject);
            }
        }

        public void UpdateHUD()
        {
            if (_lifeView == null || _lifeView.Data == null) return;

            var hp = _lifeView.Data.health;
            var sp = _lifeView.Data.stamina;
            int currentShield = _lifeView.Data.CurrentShield;

            // max HP가 바뀌면 눈금 재생성
            if (hp.Max != _lastTickMaxHp)
                BuildTickMarks(hp.Max);

            float hpRatio = hp.Max > 0 ? Mathf.Clamp01((float)hp.Current / hp.Max) : 0;
            float spRatio = sp.Max > 0 ? Mathf.Clamp01((float)sp.Current / sp.Max) : 0;

            // 1. HP 바 즉시 갱신
            if (_hpFill != null)
                _hpFill.transform.localScale = new Vector3(k_BarWidth * hpRatio, 0.075f, 1f);

            // 2. 대미지 고스트 잔상 처리
            if (_hpGhostFill != null)
            {
                if (_lastHpRatio < 0f)
                {
                    _hpGhostFill.transform.localScale = new Vector3(k_BarWidth * hpRatio, 0.075f, 1f);
                    _lastHpRatio = hpRatio;
                }
                else if (hpRatio < _lastHpRatio - 0.001f)
                {
                    // 대미지 피격 시: 0.35초 머문 후 0.45초 동안 부드럽게 감소
                    if (_ghostTween != null && _ghostTween.IsActive()) _ghostTween.Kill();
                    float targetWidth = k_BarWidth * hpRatio;
                    _ghostTween = DG.Tweening.DOVirtual.DelayedCall(0.35f, () =>
                    {
                        if (_hpGhostFill != null)
                        {
                            _ghostTween = _hpGhostFill.transform.DOScaleX(targetWidth, 0.45f)
                                .SetEase(DG.Tweening.Ease.OutQuad);
                        }
                    });
                    _lastHpRatio = hpRatio;
                }
                else if (hpRatio > _lastHpRatio + 0.001f)
                {
                    // 회복 시: 즉각 일치
                    if (_ghostTween != null && _ghostTween.IsActive()) _ghostTween.Kill();
                    _hpGhostFill.transform.localScale = new Vector3(k_BarWidth * hpRatio, 0.075f, 1f);
                    _lastHpRatio = hpRatio;
                }
                // hpRatio == _lastHpRatio 일 때는 트윈을 방해하지 않고 유지!
            }

            // 3. SP 바 갱신 (두께 0.03f)
            if (_spFill != null)
                _spFill.transform.localScale = new Vector3(k_BarWidth * spRatio, 0.03f, 1f);

            // 4. 보호막 바 처리 (HP바와 동일한 높이 0.075f 및 Y 0.02f)
            if (_shieldFill != null)
            {
                if (currentShield > 0 && hp.Max > 0)
                {
                    float shieldRatio = Mathf.Clamp01((float)currentShield / hp.Max);
                    float hpWidth     = k_BarWidth * hpRatio;
                    float shieldWidth = k_BarWidth * shieldRatio;

                    // 현재 HP + 보호막량이 최대 HP를 초과하면 우측 정렬, 아니면 HP 바 끝 지점부터 표시
                    float shieldStartX;
                    if (hp.Current + currentShield > hp.Max)
                    {
                        shieldStartX = (k_BarLeft + k_BarWidth) - shieldWidth;
                    }
                    else
                    {
                        shieldStartX = k_BarLeft + hpWidth;
                    }

                    _shieldFill.transform.localPosition = new Vector3(shieldStartX, 0.02f, -0.005f);
                    _shieldFill.transform.localScale    = new Vector3(shieldWidth, 0.075f, 1f);
                    _shieldFill.gameObject.SetActive(true);
                }
                else
                {
                    _shieldFill.gameObject.SetActive(false);
                }
            }

            // 텍스트 부분 모두 제거 (상세 스탯은 PlayerUIView 등에서 확인)
            if (_hpText != null && _hpText.gameObject.activeSelf)
            {
                _hpText.text = "";
                _hpText.gameObject.SetActive(false);
            }

            UpdateStatusIconTray();
            UpdateIntentBadge();
        }

        // ─────────────────────────────────────────────────────────
        // 상태효과 아이콘 트레이
        // ─────────────────────────────────────────────────────────
        private void UpdateStatusIconTray()
        {
            if (_statusTrayRoot == null) return;
            if (_lifeView.Data.statusEffects == null)
            {
                ClearStatusBadges();
                return;
            }

            var groups = _lifeView.Data.statusEffects
                .GroupBy(s => s.type)
                .ToList();

            // 배지 수가 달라지면 재생성
            if (groups.Count != _statusBadges.Count)
            {
                ClearStatusBadges();
                RebuildStatusBadges(groups);
            }
            else
            {
                RefreshStatusBadges(groups);
            }
        }

        private void ClearStatusBadges()
        {
            foreach (var badge in _statusBadges)
            {
                if (badge.go != null) Destroy(badge.go);
            }
            _statusBadges.Clear();
        }

        private void RebuildStatusBadges(List<IGrouping<_prototype_StatusType, _prototype_StatusEffect>> groups)
        {
            float badgeSize = 0.22f;
            float spacing = 0.24f;
            float totalWidth = groups.Count * spacing;
            float startX = -totalWidth * 0.5f + spacing * 0.5f;

            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                float x = startX + i * spacing;

                var badgeGo = new GameObject($"StatusBadge_{group.Key}");
                badgeGo.transform.SetParent(_statusTrayRoot, false);
                badgeGo.transform.localPosition = new Vector3(x, 0f, 0f);

                // 배경 SpriteRenderer
                var bgGo = new GameObject("Bg");
                bgGo.transform.SetParent(badgeGo.transform, false);
                var bgSr = bgGo.AddComponent<SpriteRenderer>();
                bgSr.sprite = GetSquareSprite();
                bgSr.sortingLayerID = _hpFill != null ? _hpFill.sortingLayerID : 0;
                bgSr.sortingOrder = 110;
                bgGo.transform.localScale = new Vector3(badgeSize, badgeSize, 1f);

                // 레이블 TextMeshPro
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(badgeGo.transform, false);
                var label = labelGo.AddComponent<TextMeshPro>();

                // 폰트 에셋 상속
                if (_hpText != null && _hpText.font != null)
                {
                    label.font = _hpText.font;
                    label.fontSharedMaterial = _hpText.fontSharedMaterial;
                }

                label.fontSize = 1.8f;
                label.alignment = TextAlignmentOptions.Center;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                label.sortingLayerID = _hpFill != null ? _hpFill.sortingLayerID : 0;
                label.sortingOrder = 111;
                var overlayShader = Shader.Find("TextMeshPro/Distance Field Overlay");
                if (overlayShader != null) label.fontMaterial.shader = overlayShader;

                _statusBadges.Add((badgeGo, label, null, bgSr));
                RefreshOneBadge(i, group);

                // 등장 팝 연출
                badgeGo.transform.localScale = Vector3.zero;
                badgeGo.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
            }
        }

        private void RefreshStatusBadges(List<IGrouping<_prototype_StatusType, _prototype_StatusEffect>> groups)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                RefreshOneBadge(i, groups[i]);
            }
        }

        private void RefreshOneBadge(int i, IGrouping<_prototype_StatusType, _prototype_StatusEffect> group)
        {
            if (i >= _statusBadges.Count) return;
            var (go, label, _, bgSr) = _statusBadges[i];

            var type = group.Key;
            int count = group.Count();
            // Forever 타입은 UI에 지속 틱을 표시하지 않음 (TickBased 효과의 남은 틱만 계산)
            var tickBased = group.Where(s => s.duration.TickDurationType == TickDurationType.TickBased && s.duration.Value != null).ToList();
            int maxTicks = tickBased.Count > 0 ? tickBased.Max(s => s.duration.Value.Current) : 0;


            // 시각 데이터 가져오기
            string symbol;
            Color themeColor;

            var db = _prototype_StatusVisualDatabase.Instance;
            if (db != null)
            {
                var entry = db.GetEntry(type);
                if (entry != null)
                {
                    symbol = entry.icon != null ? null : entry.symbolChar;
                    themeColor = entry.themeColor;
                }
                else
                {
                    var (s, c, _, _) = _prototype_StatusVisualDatabase.GetDefaultEntry(type);
                    symbol = s; themeColor = c;
                }
            }
            else
            {
                var (s, c, _, _) = _prototype_StatusVisualDatabase.GetDefaultEntry(type);
                symbol = s; themeColor = c;
            }

            // 배경색 설정
            if (bgSr != null)
            {
                bgSr.color = new Color(themeColor.r * 0.25f, themeColor.g * 0.25f, themeColor.b * 0.25f, 0.85f);
            }

            // 레이블 설정
            if (label != null)
            {
                string stackStr = count > 1 ? $"x{count}" : "";
                string durationStr = maxTicks > 0 ? $"\n<size=1.2>{maxTicks}t</size>" : "";
                string sym = symbol ?? "?";

                label.text = $"<color=#{ColorUtility.ToHtmlStringRGB(themeColor)}>{sym}</color>{stackStr}{durationStr}";
            }

            // Death's Door 깜빡임 연출
            if (type == _prototype_StatusType.DeathsDoor)
            {
                if (bgSr != null && !DOTween.IsTweening(bgSr))
                {
                    bgSr.DOColor(new Color(0.6f, 0f, 0f, 0.9f), 0.4f)
                        .SetLoops(-1, LoopType.Yoyo);
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        // 적 의도 배지
        // ─────────────────────────────────────────────────────────
        private void UpdateIntentBadge()
        {
            if (_intentBadge == null) return;

            var lifeData = _lifeView?.Data;
            if (lifeData?.aiLogic == null)
            {
                _intentBadge.gameObject.SetActive(false);
                return;
            }

            var intent = lifeData.aiLogic.CurrentIntent;
            if (intent == null || intent.intentType == _prototype_EnemyIntentType.None || intent.intentType == _prototype_EnemyIntentType.Rest)
            {
                _intentBadge.gameObject.SetActive(false);
                return;
            }

            _intentBadge.gameObject.SetActive(true);

            string displayText = intent.GetIntentDisplayText();
            Color color = intent.GetColor();

            if (_intentText != null)
            {
                _intentText.text = displayText;
                _intentText.color = color;
            }
            if (_intentBg != null)
            {
                // 글자 수에 맞춘 가변 너비 조절 (한 줄 텍스트가 패딩과 함께 깔끔하게 감싸짐)
                float width = Mathf.Max(0.52f, 0.18f + displayText.Length * 0.13f);
                _intentBg.transform.localScale = new Vector3(width, 0.26f, 1f);
                _intentBg.color = new Color(color.r * 0.15f, color.g * 0.15f, color.b * 0.15f, 0.88f);
            }
        }

        private bool ShouldShowHUD()
        {
            if (_lifeView?.Data == null || _lifeView.Data.IsDead) return false;

            var modeMgr = _prototype_PlayModeManager.Instance;
            bool isExploration = modeMgr != null && modeMgr.IsExploration;

            if (isExploration)
            {
                // 탐색 모드: 풀 HP이고 실드나 상태효과가 없으면 숨김
                bool isFullHp = _lifeView.Data.health.Current >= _lifeView.Data.health.Max;
                bool hasShield = _lifeView.Data.CurrentShield > 0;
                bool hasStatus = _lifeView.Data.statusEffects != null && _lifeView.Data.statusEffects.Count > 0;
                if (isFullHp && !hasShield && !hasStatus)
                {
                    return false;
                }
            }

            return true;
        }

        private void LateUpdate()
        {
            if (_hudRoot != null)
            {
                bool shouldShow = ShouldShowHUD();
                if (_hudRoot.gameObject.activeSelf != shouldShow)
                {
                    _hudRoot.gameObject.SetActive(shouldShow);
                }

                if (!shouldShow) return;

                UpdateHUD();

                // 머리 위 헤드업 배치 (오프셋 +0.85f)
                _hudRoot.position = transform.position + new Vector3(0f, 0.85f, 0f);

                Camera mainCamera = _prototype_CameraController.Instance != null 
                    ? _prototype_CameraController.Instance.MainCamera 
                    : Camera.main;
                if (mainCamera != null)
                {
                    _hudRoot.rotation = mainCamera.transform.rotation;
                }
            }
        }
    }
}
