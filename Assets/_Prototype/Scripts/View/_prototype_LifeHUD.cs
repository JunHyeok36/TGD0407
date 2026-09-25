using UnityEngine;
using UnityEngine.UI;
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

        // ── Screen-Space Canvas ──
        private static Canvas s_lifeHudCanvas;
        public static Canvas ScreenCanvas => GetOrCreateCanvas();

        private static Canvas GetOrCreateCanvas()
        {
            if (s_lifeHudCanvas != null) return s_lifeHudCanvas;

            var existing = GameObject.Find("LifeHUD_ScreenCanvas");
            if (existing != null)
            {
                s_lifeHudCanvas = existing.GetComponent<Canvas>();
                if (s_lifeHudCanvas != null) return s_lifeHudCanvas;
            }

            var canvasGo = new GameObject("LifeHUD_ScreenCanvas");
            DontDestroyOnLoad(canvasGo);
            s_lifeHudCanvas = canvasGo.AddComponent<Canvas>();
            s_lifeHudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            s_lifeHudCanvas.sortingOrder = 50;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            return s_lifeHudCanvas;
        }

        // ── HUD 크기 및 레이아웃 상수 (160x30 프레임, 156px 내부 캐비티 1:1 매핑) ──
        private const float k_HudWidth = 160f;
        private const float k_HudHeight = 30f;
        private const float k_OuterWidth = 164f;
        private const float k_OuterHeight = 34f;
        private const float k_BarWidth = 156f;
        private const float k_HpBarHeight = 20f;
        private const float k_HpBarY = 3.0f;
        private const float k_SpBarHeight = 5f;
        private const float k_SpBarY = -10.5f;
        private const float k_DividerY = -7.5f;

        // ── HUD UI 계층 구조 ──
        private RectTransform _hudRect;
        private Image _shadowImg;
        private Image _outerBorderImg;
        private Image _frameBgBack;
        private Image _frameBg;
        private Image _hpBg;
        private Image _hpGhostFill;
        private Image _hpFill;
        private Image _shieldFill;
        private Image _spBg;
        private Image _spFill;
        private RectTransform _hpTicksContainer;
        private RectTransform _spTicksContainer;

        private System.IDisposable _shieldSub;
        private DG.Tweening.Tween _ghostTween;
        private float _lastHpRatio = -1f;

        // ── 상태효과 아이콘 트레이 ──
        private struct StatusBadgeItem
        {
            public GameObject rootGo;
            public Image bgImg;
            public Image iconImg;
            public TextMeshProUGUI fallbackLabel;
            public GameObject tickBadgeGo;
            public TextMeshProUGUI tickText;
        }

        private RectTransform _statusTrayRoot;
        private readonly List<StatusBadgeItem> _statusBadges = new();

        // ── 적 의도 배지 ──
        private RectTransform _intentBadge;
        private Image _intentBg;
        private Image _intentIcon;
        private GameObject _intentValueBadge;
        private TextMeshProUGUI _intentValueText;

        // ── HP 및 SP 눈금 ──
        private readonly List<GameObject> _tickMarks = new();
        private int _lastTickMaxHp = -1;
        private int _lastTickMaxSp = -1;

        [Header("Prefab")]
        [SerializeField] private GameObject _hudPrefab;
        private static GameObject s_defaultHudPrefab;

        public static void SetDefaultPrefab(GameObject prefab)
        {
            s_defaultHudPrefab = prefab;
        }

        private bool _isInitialized = false;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
            if (_hudRect != null) _hudRect.gameObject.SetActive(true);
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _lifeView = GetComponent<_prototype_LifeView>();
            InitializeHUD();
            SetupEvents();
        }

        private static T FindChild<T>(Transform root, string path) where T : Component
        {
            var t = root.Find(path);
            if (t != null) return t.GetComponent<T>();
            t = root.Find("BarMask/" + path);
            if (t != null) return t.GetComponent<T>();
            return null;
        }

        private void InitializeHUD()
        {
            Canvas canvas = GetOrCreateCanvas();

            GameObject prefab = _hudPrefab != null ? _hudPrefab : s_defaultHudPrefab;
            if (prefab == null)
            {
                var playerUi = _prototype_PlayerUIView.Instance != null 
                    ? _prototype_PlayerUIView.Instance 
                    : FindFirstObjectByType<_prototype_PlayerUIView>();
                if (playerUi != null && playerUi.LifeHudPrefab != null)
                {
                    prefab = playerUi.LifeHudPrefab;
                    s_defaultHudPrefab = prefab;
                }
            }

            bool isPlayer = (_lifeView != null && _lifeView.Data != null && _lifeView.Data.side == _prototype_Side.A);

            if (prefab != null)
            {
                var hudGo = Instantiate(prefab, canvas.transform);
                hudGo.name = "LifeHUD_" + gameObject.name;
                _hudRect = hudGo.GetComponent<RectTransform>();
                _hudRect.sizeDelta = new Vector2(k_HudWidth, k_HudHeight);
                _hudRect.pivot = new Vector2(0.5f, 0.5f);

                _shadowImg = FindChild<Image>(_hudRect, "DropShadow");
                _outerBorderImg = FindChild<Image>(_hudRect, "OuterBorder");
                _frameBgBack = FindChild<Image>(_hudRect, "Frame_Back");
                _hpBg = FindChild<Image>(_hudRect, "HP_BG");
                _hpGhostFill = FindChild<Image>(_hudRect, "HP_Ghost");
                _hpFill = FindChild<Image>(_hudRect, "HP_Fill");
                _shieldFill = FindChild<Image>(_hudRect, "Shield_Fill");
                _hpTicksContainer = FindChild<RectTransform>(_hudRect, "HP_Ticks");
                _spBg = FindChild<Image>(_hudRect, "SP_BG");
                _spFill = FindChild<Image>(_hudRect, "SP_Fill");
                _spTicksContainer = FindChild<RectTransform>(_hudRect, "SP_Ticks");
                _frameBg = FindChild<Image>(_hudRect, "Frame_BG");
                _statusTrayRoot = FindChild<RectTransform>(_hudRect, "StatusIconTray");

                var intentTransform = _hudRect.Find("IntentBadge");
                if (intentTransform != null)
                {
                    _intentBadge = intentTransform.GetComponent<RectTransform>();
                    _intentBg = intentTransform.Find("IntentBg")?.GetComponent<Image>();
                    _intentIcon = intentTransform.Find("IntentIcon")?.GetComponent<Image>();
                    var valBadge = intentTransform.Find("IntentValueBadge");
                    if (valBadge != null)
                    {
                        _intentValueBadge = valBadge.gameObject;
                        _intentValueText = valBadge.Find("Text")?.GetComponent<TextMeshProUGUI>();
                    }
                }

                Sprite sq = GetSquareSprite();
                if (_hpGhostFill != null && _hpGhostFill.sprite == null) _hpGhostFill.sprite = sq;
                if (_hpFill != null && _hpFill.sprite == null) _hpFill.sprite = sq;
                if (_shieldFill != null && _shieldFill.sprite == null) _shieldFill.sprite = sq;
                if (_spFill != null && _spFill.sprite == null) _spFill.sprite = sq;

                if (_frameBg != null)
                {
                    _frameBg.color = isPlayer ? new Color(0.0f, 0.95f, 1.0f, 1f) : new Color(1.0f, 0.28f, 0.38f, 1f);
                }
            }
            else
            {
                var hudGo = new GameObject("LifeHUD_" + gameObject.name);
                hudGo.transform.SetParent(canvas.transform, false);

                _hudRect = hudGo.AddComponent<RectTransform>();
                _hudRect.sizeDelta = new Vector2(k_HudWidth, k_HudHeight);
                _hudRect.pivot = new Vector2(0.5f, 0.5f);

                // 0. 드롭 섀도우 (164x34 외곽 챔퍼)
                _shadowImg = CreateImage(_hudRect, "DropShadow", new Vector2(k_OuterWidth, k_OuterHeight), new Vector2(0f, -3f), new Color(0f, 0f, 0f, 0.65f), GetChamferOuterBgSprite());

                // 1. 솔리드 블랙 외곽 아웃라인 (164x34 외곽 챔퍼)
                _outerBorderImg = CreateImage(_hudRect, "OuterBorder", new Vector2(k_OuterWidth, k_OuterHeight), Vector2.zero, new Color(0.02f, 0.03f, 0.04f, 1f), GetChamferOuterBgSprite());

                // 2. BarMask (160x30 챔퍼 마스크: 내부 바가 프레임 밖으로 절대 삐져나오지 않도록 완벽 클리핑)
                var maskGo = new GameObject("BarMask", typeof(RectTransform), typeof(Image), typeof(Mask));
                maskGo.transform.SetParent(_hudRect, false);
                var maskRect = maskGo.GetComponent<RectTransform>();
                maskRect.sizeDelta = new Vector2(k_HudWidth, k_HudHeight);
                maskRect.anchoredPosition = Vector2.zero;
                var maskImg = maskGo.GetComponent<Image>();
                maskImg.sprite = GetChamferBgSprite();
                maskImg.color = Color.white;
                maskImg.raycastTarget = false;
                var maskComp = maskGo.GetComponent<Mask>();
                maskComp.showMaskGraphic = false;

                // 3. Frame_Back (슬레이트 배경)
                _frameBgBack = CreateImage(maskRect, "Frame_Back", new Vector2(k_HudWidth, k_HudHeight), Vector2.zero, new Color(0.06f, 0.08f, 0.12f, 1f), GetChamferBgSprite());

                // 4. HP 트랙 (어두운 배경 156x20)
                _hpBg = CreateImage(maskRect, "HP_BG", new Vector2(k_BarWidth, k_HpBarHeight), new Vector2(0f, k_HpBarY), new Color(0.07f, 0.09f, 0.14f, 0.95f));

                // 5. HP 고스트 잔상 바
                _hpGhostFill = CreateImage(maskRect, "HP_Ghost", new Vector2(k_BarWidth, k_HpBarHeight), Vector2.zero, new Color(1f, 1f, 1f, 0.95f), GetSquareSprite());
                SetAnchorLeft(_hpGhostFill.rectTransform, k_HpBarY);

                // 6. HP 채움 바 (에메랄드 그린)
                _hpFill = CreateImage(maskRect, "HP_Fill", new Vector2(k_BarWidth, k_HpBarHeight), Vector2.zero, new Color(0.15f, 0.92f, 0.45f, 1f), GetSquareSprite());
                SetAnchorLeft(_hpFill.rectTransform, k_HpBarY);

                // 7. 보호막 바 (시안)
                _shieldFill = CreateImage(maskRect, "Shield_Fill", new Vector2(k_BarWidth, k_HpBarHeight), Vector2.zero, new Color(0.2f, 0.85f, 0.95f, 0.95f), GetSquareSprite());
                SetAnchorLeft(_shieldFill.rectTransform, k_HpBarY);
                _shieldFill.gameObject.SetActive(false);

                // 8. HP 눈금 컨테이너
                var hpTickGo = new GameObject("HP_Ticks");
                hpTickGo.transform.SetParent(maskRect, false);
                _hpTicksContainer = hpTickGo.AddComponent<RectTransform>();
                _hpTicksContainer.sizeDelta = new Vector2(k_BarWidth, k_HpBarHeight);
                SetAnchorLeft(_hpTicksContainer, k_HpBarY);

                // 9. 1px 분리선
                CreateImage(maskRect, "Divider", new Vector2(k_BarWidth, 1f), new Vector2(0f, k_DividerY), new Color(0.02f, 0.03f, 0.04f, 1f));

                // 10. SP 트랙 (어두운 배경 156x5)
                _spBg = CreateImage(maskRect, "SP_BG", new Vector2(k_BarWidth, k_SpBarHeight), new Vector2(0f, k_SpBarY), new Color(0.05f, 0.07f, 0.12f, 0.95f));

                // 11. SP 채움 바 (호박색 골드)
                _spFill = CreateImage(maskRect, "SP_Fill", new Vector2(k_BarWidth, k_SpBarHeight), Vector2.zero, new Color(1f, 0.75f, 0.15f, 1f), GetSquareSprite());
                SetAnchorLeft(_spFill.rectTransform, k_SpBarY);

                // 12. SP 눈금 컨테이너
                var spTickGo = new GameObject("SP_Ticks");
                spTickGo.transform.SetParent(maskRect, false);
                _spTicksContainer = spTickGo.AddComponent<RectTransform>();
                _spTicksContainer.sizeDelta = new Vector2(k_BarWidth, k_SpBarHeight);
                SetAnchorLeft(_spTicksContainer, k_SpBarY);

                // 13. 챔퍼 외곽 프레임 (플레이어: 네온 시안, 적: 네온 크림슨 레드)
                Color frameColor = isPlayer ? new Color(0.0f, 0.95f, 1.0f, 1f) : new Color(1.0f, 0.28f, 0.38f, 1f);
                _frameBg = CreateImage(_hudRect, "Frame_BG", new Vector2(k_HudWidth, k_HudHeight), Vector2.zero, frameColor, GetChamferFrameSprite());

                // 14. 상태효과 아이콘 트레이
                var trayGo = new GameObject("StatusIconTray");
                trayGo.transform.SetParent(_hudRect, false);
                _statusTrayRoot = trayGo.AddComponent<RectTransform>();
                _statusTrayRoot.anchoredPosition = new Vector2(0f, -24f);

                // 15. 적 의도 배지
                CreateIntentBadge();
            }

            // 14. 눈금 생성
            if (_lifeView?.Data != null)
                BuildTickMarks(_lifeView.Data.health.Max, _lifeView.Data.stamina.Max);

            UpdateHUD();
        }

        private static void SetAnchorLeft(RectTransform rect, float yPos)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(-k_BarWidth * 0.5f, yPos);
        }

        private static Image CreateImage(Transform parent, string name, Vector2 size, Vector2 pos, Color color, Sprite sprite = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.color = color;
            if (sprite != null) img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, Vector2 size, Vector2 pos, float fontSize, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = GetNeodgmFont();
            if (font != null) tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        // 10단위는 1px, 100단위는 2px, 1000단위는 3px, 10000단위는 4px 너비 매핑
        private static float GetTickWidthPx(int val)
        {
            if (val % 10000 == 0) return 4f;
            if (val % 1000 == 0)  return 3f;
            if (val % 100 == 0)   return 2f;
            return 1f; // 10단위는 1px
        }

        private void BuildTickMarks(int maxHp, int maxSp)
        {
            _lastTickMaxHp = maxHp;
            _lastTickMaxSp = maxSp;

            foreach (var go in _tickMarks)
                if (go != null) Destroy(go);
            _tickMarks.Clear();

            if (_hpTicksContainer == null || _spTicksContainer == null) return;

            for (int i = _hpTicksContainer.childCount - 1; i >= 0; i--)
                Destroy(_hpTicksContainer.GetChild(i).gameObject);
            for (int i = _spTicksContainer.childCount - 1; i >= 0; i--)
                Destroy(_spTicksContainer.GetChild(i).gameObject);

            Sprite squareSprite = GetSquareSprite();

            // 1. HP 바 눈금 (10단위: 1px 상단 55%, 100단위: 2px 풀높이, 1000단위: 3px 풀높이)
            if (maxHp > 0)
            {
                var (minorUnit, majorUnit) = CalculateSmartTickIntervals(maxHp);
                if (minorUnit > 0)
                {
                    float minorHeight = 11f; // 20px 높이 중 상단 55% (~11px)
                    for (int val = minorUnit; val < maxHp; val += minorUnit)
                    {
                        bool isMajor = (majorUnit > 0) && (val % majorUnit == 0);
                        float ratio = (float)val / maxHp;

                        float pixelX = Mathf.Round(ratio * k_BarWidth);
                        float widthPx = GetTickWidthPx(val);
                        float heightPx = isMajor ? k_HpBarHeight : minorHeight;

                        var tickGo = new GameObject(isMajor ? $"Tick_Major_{val}" : $"Tick_Minor_{val}");
                        tickGo.transform.SetParent(_hpTicksContainer, false);
                        var rect = tickGo.AddComponent<RectTransform>();
                        
                        rect.anchorMin = new Vector2(0f, 1f);
                        rect.anchorMax = new Vector2(0f, 1f);
                        rect.pivot     = new Vector2(0.5f, 1f); // 상단에서 아래로 매달림
                        rect.anchoredPosition = new Vector2(pixelX, 0f);
                        rect.sizeDelta        = new Vector2(widthPx, heightPx);

                        var img = tickGo.AddComponent<Image>();
                        img.sprite = squareSprite;
                        img.color  = isMajor ? new Color(0.02f, 0.03f, 0.04f, 1f) : new Color(0.04f, 0.06f, 0.09f, 0.95f);
                        img.raycastTarget = false;

                        _tickMarks.Add(tickGo);
                    }
                }
            }

            // 2. SP 바 눈금 (5px 풀높이, 10단위 1px, 100단위 2px)
            if (maxSp > 0)
            {
                int spUnit = maxSp <= 100 ? 10 : 100;
                for (int val = spUnit; val < maxSp; val += spUnit)
                {
                    float ratio = (float)val / maxSp;
                    float pixelX = Mathf.Round(ratio * k_BarWidth);
                    float widthPx = GetTickWidthPx(val);

                    var tickGo = new GameObject($"SP_Tick_{val}");
                    tickGo.transform.SetParent(_spTicksContainer, false);
                    var rect = tickGo.AddComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0f, 0.5f);
                    rect.anchorMax = new Vector2(0f, 0.5f);
                    rect.pivot     = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(pixelX, 0f);
                    rect.sizeDelta        = new Vector2(widthPx, k_SpBarHeight);

                    var img = tickGo.AddComponent<Image>();
                    img.sprite = squareSprite;
                    img.color  = new Color(0.04f, 0.06f, 0.09f, 0.95f);
                    img.raycastTarget = false;

                    _tickMarks.Add(tickGo);
                }
            }
        }

        private static (int minor, int major) CalculateSmartTickIntervals(int maxHp)
        {
            if (maxHp <= 250) return (10, 100);
            if (maxHp <= 2500) return (100, 1000);
            return (1000, 10000);
        }

        private void CreateIntentBadge()
        {
            var badgeGo = new GameObject("IntentBadge");
            badgeGo.transform.SetParent(_hudRect, false);
            _intentBadge = badgeGo.AddComponent<RectTransform>();
            _intentBadge.sizeDelta = new Vector2(28f, 28f);
            _intentBadge.anchoredPosition = new Vector2(0f, 28f);

            _intentBg = CreateImage(_intentBadge, "IntentBg", new Vector2(28f, 28f), Vector2.zero, Color.white, GetDiamondBadgeSprite());
            _intentIcon = CreateImage(_intentBadge, "IntentIcon", new Vector2(22f, 22f), Vector2.zero, Color.white);

            _intentValueBadge = new GameObject("IntentValueBadge");
            _intentValueBadge.transform.SetParent(_intentBadge, false);
            var valRect = _intentValueBadge.AddComponent<RectTransform>();
            valRect.sizeDelta = new Vector2(16f, 16f);
            valRect.anchoredPosition = new Vector2(10f, -10f);

            var valBg = CreateImage(_intentValueBadge.transform, "Bg", new Vector2(16f, 16f), Vector2.zero, Color.white, GetMiniBadgeSprite());
            _intentValueText = CreateText(_intentValueBadge.transform, "Text", new Vector2(16f, 16f), Vector2.zero, 11f, Color.white);

            _intentBadge.gameObject.SetActive(false);
        }

        private static Sprite _squareSprite;
        private static Sprite GetSquareSprite()
        {
            if (_squareSprite == null)
            {
                var tex = new Texture2D(4, 4);
                tex.filterMode = FilterMode.Point;
                var cols = new Color[16];
                for (int i = 0; i < cols.Length; i++) cols[i] = Color.white;
                tex.SetPixels(cols);
                tex.Apply();
                _squareSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return _squareSprite;
        }

        private static Sprite s_chamferOuterBgSprite;
        private static Sprite s_chamferFrameSprite;
        private static Sprite s_chamferBgSprite;
        private static Sprite s_diamondBadgeSprite;
        private static Sprite s_statusChipSprite;
        private static Sprite s_miniBadgeSprite;
        private static TMP_FontAsset s_neodgmFont;

        private static Sprite GetChamferOuterBgSprite()
        {
            if (s_chamferOuterBgSprite == null)
                s_chamferOuterBgSprite = Resources.Load<Sprite>("HUD/LifeHUD_ChamferOuterBg");
            return s_chamferOuterBgSprite != null ? s_chamferOuterBgSprite : GetSquareSprite();
        }

        private static Sprite GetChamferFrameSprite()
        {
            if (s_chamferFrameSprite == null)
                s_chamferFrameSprite = Resources.Load<Sprite>("HUD/LifeHUD_ChamferFrame");
            return s_chamferFrameSprite != null ? s_chamferFrameSprite : GetSquareSprite();
        }

        private static Sprite GetChamferBgSprite()
        {
            if (s_chamferBgSprite == null)
                s_chamferBgSprite = Resources.Load<Sprite>("HUD/LifeHUD_ChamferBg");
            return s_chamferBgSprite != null ? s_chamferBgSprite : GetSquareSprite();
        }

        private static Sprite GetDiamondBadgeSprite()
        {
            if (s_diamondBadgeSprite == null)
                s_diamondBadgeSprite = Resources.Load<Sprite>("HUD/LifeHUD_DiamondBadge");
            return s_diamondBadgeSprite != null ? s_diamondBadgeSprite : GetSquareSprite();
        }

        private static Sprite GetStatusChipSprite()
        {
            if (s_statusChipSprite == null)
                s_statusChipSprite = Resources.Load<Sprite>("HUD/LifeHUD_StatusChip");
            return s_statusChipSprite != null ? s_statusChipSprite : GetSquareSprite();
        }

        private static Sprite GetMiniBadgeSprite()
        {
            if (s_miniBadgeSprite == null)
                s_miniBadgeSprite = Resources.Load<Sprite>("HUD/LifeHUD_MiniBadge");
            return s_miniBadgeSprite != null ? s_miniBadgeSprite : GetSquareSprite();
        }

        private static TMP_FontAsset GetNeodgmFont()
        {
            if (s_neodgmFont == null)
            {
                s_neodgmFont = Resources.Load<TMP_FontAsset>("neodgm_SDF_TMPro");
#if UNITY_EDITOR
                if (s_neodgmFont == null)
                    s_neodgmFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/neodgm_SDF_TMPro.asset");
#endif
                if (s_neodgmFont == null)
                    s_neodgmFont = TMP_Settings.defaultFontAsset;
            }
            return s_neodgmFont;
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
            if (_hudRect != null)
            {
                _hudRect.gameObject.SetActive(false);
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
            if (_hudRect != null)
            {
                Destroy(_hudRect.gameObject);
            }
        }

        public void UpdateHUD()
        {
            if (_lifeView == null) _lifeView = GetComponent<_prototype_LifeView>();
            if (_lifeView == null || _lifeView.Data == null) return;

            if (!_isInitialized || _hudRect == null)
            {
                _isInitialized = false;
                Initialize();
            }

            var hp = _lifeView.Data.health;
            var sp = _lifeView.Data.stamina;
            int currentShield = _lifeView.Data.CurrentShield;

            // max HP 또는 max SP가 바뀌면 눈금 재생성
            if (hp.Max != _lastTickMaxHp || sp.Max != _lastTickMaxSp)
                BuildTickMarks(hp.Max, sp.Max);

            float hpRatio = hp.Max > 0 ? Mathf.Clamp01((float)hp.Current / hp.Max) : 0;
            float spRatio = sp.Max > 0 ? Mathf.Clamp01((float)sp.Current / sp.Max) : 0;

            // 1. HP 바 즉시 갱신
            if (_hpFill != null)
                _hpFill.rectTransform.sizeDelta = new Vector2(k_BarWidth * hpRatio, k_HpBarHeight);

            // 2. HP 고스트 잔상 (DOTween 감쇄)
            if (_hpGhostFill != null)
            {
                if (_lastHpRatio < 0)
                {
                    _hpGhostFill.rectTransform.sizeDelta = new Vector2(k_BarWidth * hpRatio, k_HpBarHeight);
                }
                else if (hpRatio < _lastHpRatio)
                {
                    if (_ghostTween != null && _ghostTween.IsActive()) _ghostTween.Kill();
                    float targetW = k_BarWidth * hpRatio;
                    _ghostTween = DOTween.To(() => _hpGhostFill.rectTransform.sizeDelta.x,
                        x => _hpGhostFill.rectTransform.sizeDelta = new Vector2(x, k_HpBarHeight),
                        targetW, 0.45f)
                        .SetDelay(0.2f)
                        .SetEase(Ease.OutQuad);
                }
                else
                {
                    if (_ghostTween != null && _ghostTween.IsActive()) _ghostTween.Kill();
                    _hpGhostFill.rectTransform.sizeDelta = new Vector2(k_BarWidth * hpRatio, k_HpBarHeight);
                }
            }
            _lastHpRatio = hpRatio;

            // 3. 보호막 바 갱신 (텍스트 미표시, HP바에만 표시)
            if (_shieldFill != null)
            {
                if (currentShield > 0 && hp.Max > 0)
                {
                    _shieldFill.gameObject.SetActive(true);
                    var shieldRect = _shieldFill.rectTransform;

                    bool isOverflow = (hp.Current + currentShield) > hp.Max;

                    if (!isOverflow)
                    {
                        // [HP + 보호막 <= Max HP]: 현재 HP 끝나는 지점에서부터 우측으로 표시
                        shieldRect.pivot = new Vector2(0f, 0.5f);
                        float currentHpWidth = hpRatio * k_BarWidth;
                        float shieldWidth = ((float)currentShield / hp.Max) * k_BarWidth;
                        shieldRect.anchoredPosition = new Vector2(-k_BarWidth * 0.5f + currentHpWidth, k_HpBarY);
                        shieldRect.sizeDelta = new Vector2(shieldWidth, k_HpBarHeight);
                    }
                    else
                    {
                        // [HP + 보호막 > Max HP]: HP바 우측 끝(100%)에 정렬하여 좌측으로 표시
                        shieldRect.pivot = new Vector2(1f, 0.5f);
                        float shieldWidth = Mathf.Min(k_BarWidth, ((float)currentShield / hp.Max) * k_BarWidth);
                        shieldRect.anchoredPosition = new Vector2(k_BarWidth * 0.5f, k_HpBarY);
                        shieldRect.sizeDelta = new Vector2(shieldWidth, k_HpBarHeight);
                    }
                }
                else
                {
                    _shieldFill.gameObject.SetActive(false);
                }
            }

            // 4. SP 바 갱신
            if (_spFill != null)
                _spFill.rectTransform.sizeDelta = new Vector2(k_BarWidth * spRatio, k_SpBarHeight);

            UpdateStatusIconTray();
            UpdateIntentBadge();
        }

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
            foreach (var item in _statusBadges)
            {
                if (item.rootGo != null) Destroy(item.rootGo);
            }
            _statusBadges.Clear();
        }

        private void RebuildStatusBadges(List<IGrouping<_prototype_StatusType, _prototype_StatusEffect>> groups)
        {
            float badgeSize = 22f;
            float spacing = 26f;
            float totalWidth = groups.Count * spacing;
            float startX = -totalWidth * 0.5f + spacing * 0.5f;

            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                float x = startX + i * spacing;

                var badgeGo = new GameObject($"StatusBadge_{group.Key}");
                badgeGo.transform.SetParent(_statusTrayRoot, false);
                var badgeRect = badgeGo.AddComponent<RectTransform>();
                badgeRect.sizeDelta = new Vector2(badgeSize, badgeSize);
                badgeRect.anchoredPosition = new Vector2(x, 0f);

                // 1. 슬레이트 사각 칩 배경
                var bgImg = CreateImage(badgeGo.transform, "Bg", new Vector2(badgeSize, badgeSize), Vector2.zero, Color.white, GetStatusChipSprite());

                // 2. 심볼 아이콘
                var iconImg = CreateImage(badgeGo.transform, "Icon", new Vector2(16f, 16f), Vector2.zero, Color.white);

                // 3. 폴백 레이블
                var label = CreateText(badgeGo.transform, "Label", new Vector2(20f, 20f), Vector2.zero, 13f, Color.white);

                // 4. 지속 틱/스택 수치 미니 배지 (우하단)
                var tickBadgeGo = new GameObject("TickBadge");
                tickBadgeGo.transform.SetParent(badgeGo.transform, false);
                var tbRect = tickBadgeGo.AddComponent<RectTransform>();
                tbRect.sizeDelta = new Vector2(12f, 12f);
                tbRect.anchoredPosition = new Vector2(7f, -7f);

                var tickBg = CreateImage(tickBadgeGo.transform, "Bg", new Vector2(12f, 12f), Vector2.zero, Color.white, GetMiniBadgeSprite());
                var tickText = CreateText(tickBadgeGo.transform, "Text", new Vector2(12f, 12f), Vector2.zero, 9f, Color.white);

                _statusBadges.Add(new StatusBadgeItem
                {
                    rootGo = badgeGo,
                    bgImg = bgImg,
                    iconImg = iconImg,
                    fallbackLabel = label,
                    tickBadgeGo = tickBadgeGo,
                    tickText = tickText
                });

                RefreshOneBadge(i, group);

                badgeGo.transform.localScale = Vector3.one;
                badgeGo.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 2);
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
            var item = _statusBadges[i];

            var type = group.Key;
            int count = group.Count();
            var tickBased = group.Where(s => s.duration.TickDurationType == TickDurationType.TickBased && s.duration.Value != null).ToList();
            int maxTicks = tickBased.Count > 0 ? tickBased.Max(s => s.duration.Value.Current) : 0;

            Sprite iconSprite = null;
            string symbol = "?";
            Color themeColor = Color.white;

            var db = _prototype_StatusVisualDatabase.Instance;
            if (db != null)
            {
                var entry = db.GetEntry(type);
                if (entry != null)
                {
                    iconSprite = entry.icon;
                    symbol = entry.symbolChar;
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

            if (item.bgImg != null)
            {
                item.bgImg.color = new Color(
                    Mathf.Lerp(0.09f, themeColor.r, 0.25f),
                    Mathf.Lerp(0.11f, themeColor.g, 0.25f),
                    Mathf.Lerp(0.16f, themeColor.b, 0.25f),
                    0.95f
                );
            }

            if (iconSprite != null)
            {
                if (item.iconImg != null)
                {
                    item.iconImg.gameObject.SetActive(true);
                    item.iconImg.sprite = iconSprite;
                    item.iconImg.color = Color.white;
                }
                if (item.fallbackLabel != null) item.fallbackLabel.gameObject.SetActive(false);
            }
            else
            {
                if (item.iconImg != null) item.iconImg.gameObject.SetActive(false);
                if (item.fallbackLabel != null)
                {
                    item.fallbackLabel.gameObject.SetActive(true);
                    item.fallbackLabel.text = $"<color=#{ColorUtility.ToHtmlStringRGB(themeColor)}>{symbol}</color>";
                }
            }

            if (item.tickBadgeGo != null)
            {
                if (maxTicks > 0)
                {
                    item.tickBadgeGo.SetActive(true);
                    if (item.tickText != null) item.tickText.text = maxTicks.ToString();
                }
                else if (count > 1)
                {
                    item.tickBadgeGo.SetActive(true);
                    if (item.tickText != null) item.tickText.text = count.ToString();
                }
                else
                {
                    item.tickBadgeGo.SetActive(false);
                }
            }

            if (type == _prototype_StatusType.DeathsDoor)
            {
                if (item.bgImg != null && !DOTween.IsTweening(item.bgImg))
                {
                    item.bgImg.DOColor(new Color(0.6f, 0f, 0f, 0.9f), 0.4f)
                        .SetLoops(-1, LoopType.Yoyo);
                }
            }
        }

        private static Sprite s_iconSword;
        private static Sprite s_iconShield;
        private static Sprite s_iconBuff;
        private static Sprite s_iconDebuff;
        private static Sprite s_iconSpecial;
        private static Sprite s_iconStun;

        private static Sprite GetIntentSprite(_prototype_EnemyIntentType type)
        {
            switch (type)
            {
                case _prototype_EnemyIntentType.AttackMelee:
                case _prototype_EnemyIntentType.AttackRanged:
                case _prototype_EnemyIntentType.AttackKnockback:
                    if (s_iconSword == null) s_iconSword = Resources.Load<Sprite>("HUD/Icon_Sword");
                    return s_iconSword;
                case _prototype_EnemyIntentType.AttackDebuff:
                    if (s_iconDebuff == null) s_iconDebuff = Resources.Load<Sprite>("HUD/Icon_Debuff");
                    return s_iconDebuff;
                case _prototype_EnemyIntentType.Move:
                    if (s_iconBuff == null) s_iconBuff = Resources.Load<Sprite>("HUD/Icon_Buff");
                    return s_iconBuff;
                case _prototype_EnemyIntentType.Stunned:
                    if (s_iconStun == null) s_iconStun = Resources.Load<Sprite>("HUD/Icon_Stun");
                    return s_iconStun;
                case _prototype_EnemyIntentType.Flee:
                default:
                    if (s_iconSpecial == null) s_iconSpecial = Resources.Load<Sprite>("HUD/Icon_Special");
                    return s_iconSpecial;
            }
        }

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

            Sprite iconSprite = GetIntentSprite(intent.intentType);
            if (_intentIcon != null)
            {
                _intentIcon.sprite = iconSprite;
                _intentIcon.gameObject.SetActive(iconSprite != null);
                _intentIcon.color = Color.white;
            }

            int val = intent.estimatedDamage;
            if (_intentValueBadge != null)
            {
                if (val > 0)
                {
                    _intentValueBadge.SetActive(true);
                    if (_intentValueText != null)
                    {
                        _intentValueText.text = val.ToString();
                        _intentValueText.color = Color.white;
                    }
                }
                else
                {
                    _intentValueBadge.SetActive(false);
                }
            }

            if (_intentBg != null)
            {
                _intentBg.color = Color.white;
            }
        }

        private bool ShouldShowHUD()
        {
            if (_lifeView?.Data == null || _lifeView.Data.IsDead) return false;

            var modeMgr = _prototype_PlayModeManager.Instance;
            bool isExploration = modeMgr != null && modeMgr.IsExploration;

            if (isExploration)
            {
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
            if (_hudRect != null)
            {
                bool shouldShow = ShouldShowHUD();
                if (_hudRect.gameObject.activeSelf != shouldShow)
                {
                    _hudRect.gameObject.SetActive(shouldShow);
                }

                if (!shouldShow) return;

                UpdateHUD();

                Camera mainCamera = _prototype_CameraController.Instance != null 
                    ? _prototype_CameraController.Instance.MainCamera 
                    : Camera.main;

                if (mainCamera != null)
                {
                    // 캐릭터 머리 위 월드 좌표 (오프셋 +1.10f)
                    Vector3 worldPos = transform.position + new Vector3(0f, 1.10f, 0f);
                    Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

                    // 카메라 뒤편이면 숨김
                    if (screenPos.z < 0)
                    {
                        if (_hudRect.gameObject.activeSelf) _hudRect.gameObject.SetActive(false);
                        return;
                    }

                    if (!_hudRect.gameObject.activeSelf) _hudRect.gameObject.SetActive(true);

                    Canvas canvas = GetOrCreateCanvas();
                    RectTransform canvasRect = canvas.transform as RectTransform;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 localPoint))
                    {
                        _hudRect.anchoredPosition = localPoint;
                    }
                }
            }
        }
    }
}
