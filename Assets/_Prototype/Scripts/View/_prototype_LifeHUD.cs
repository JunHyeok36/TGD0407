using UnityEngine;
using TMPro;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using DG.Tweening;

namespace TDG0407._prototype
{
    [RequireComponent(typeof(_prototype_LifeView))]
    public class _prototype_LifeHUD : MonoBehaviour
    {
        private _prototype_LifeView _lifeView;

        private Transform _hudRoot;
        private SpriteRenderer _hpFill;
        private SpriteRenderer _spFill;
        private TextMeshPro _hpText;

        // ── 상태효과 아이콘 트레이 ──
        private Transform _statusTrayRoot;
        private readonly List<(GameObject go, TextMeshPro label, SpriteRenderer icon, SpriteRenderer bg)> _statusBadges = new();

        // ── 적 의도 배지 ──
        private TextMeshPro _intentText;
        private SpriteRenderer _intentBg;
        private Transform _intentBadge;

        // ── 레거시 Status_Text (하위호환 — 없어도 무방) ──
        private TextMeshPro _statusText;

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
            if (prefab == null)
            {
                Debug.LogError($"LifeHUD prefab is not assigned for {gameObject.name}! Please assign it on the component or BootStrapper.");
                return;
            }

            GameObject hudInstance = Instantiate(prefab);
            hudInstance.name = "LifeHUD_" + gameObject.name;
            _hudRoot = hudInstance.transform;

            _hpFill = _hudRoot.Find("HP_Fill")?.GetComponent<SpriteRenderer>();
            _spFill = _hudRoot.Find("SP_Fill")?.GetComponent<SpriteRenderer>();
            _hpText = _hudRoot.Find("HUD_Text")?.GetComponent<TextMeshPro>();

            // 레거시 Status_Text — 찾으면 숨김 처리 (이제 아이콘 트레이가 대체)
            var statusTextObj = _hudRoot.Find("Status_Text");
            if (statusTextObj != null)
            {
                _statusText = statusTextObj.GetComponent<TextMeshPro>();
                if (_statusText != null) _statusText.text = "";
                statusTextObj.gameObject.SetActive(false);
            }

            if (_hpText != null)
            {
                var overlayShader = Shader.Find("TextMeshPro/Distance Field Overlay");
                if (overlayShader != null)
                    _hpText.fontMaterial.shader = overlayShader;
            }

            // ── 상태효과 아이콘 트레이 컨테이너 생성 ──
            _statusTrayRoot = CreateStatusTrayContainer();

            // ── 적 의도 배지 생성 (적 엔티티에만 적용 = aiLogic 있는 엔티티) ──
            // 플레이어의 aiLogic은 null이므로 자연스럽게 필터링됩니다.
            // 실제 배지 활성화는 UpdateIntentBadge()에서 aiLogic null 체크로 결정됩니다.
            CreateIntentBadge();

            UpdateHUD();
        }

        private Transform CreateStatusTrayContainer()
        {
            var tray = new GameObject("StatusIconTray");
            tray.transform.SetParent(_hudRoot, false);
            // HP 바 아래쪽에 위치 (HP_Fill이 -0.1 정도라 -0.35 사용)
            tray.transform.localPosition = new Vector3(0f, -0.35f, 0f);
            return tray.transform;
        }

        private void CreateIntentBadge()
        {
            _intentBadge = new GameObject("IntentBadge").transform;
            _intentBadge.SetParent(_hudRoot, false);
            // HUD 상단 (캐릭터 위쪽)
            _intentBadge.localPosition = new Vector3(0f, 0.45f, 0f);

            // 배지 배경
            var bgGo = new GameObject("IntentBg");
            bgGo.transform.SetParent(_intentBadge, false);
            _intentBg = bgGo.AddComponent<SpriteRenderer>();
            _intentBg.sprite = GetSquareSprite();
            _intentBg.color = new Color(0f, 0f, 0f, 0.7f);
            _intentBg.sortingOrder = 20;
            bgGo.transform.localScale = new Vector3(0.6f, 0.28f, 1f);

            // 배지 텍스트
            var textGo = new GameObject("IntentText");
            textGo.transform.SetParent(_intentBadge, false);
            _intentText = textGo.AddComponent<TextMeshPro>();
            _intentText.fontSize = 2.5f;
            _intentText.alignment = TextAlignmentOptions.Center;
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
                UpdateHUD();
            }
        }

        private void OnDestroy()
        {
            if (_lifeView != null && _lifeView.Data != null)
            {
                _lifeView.Data.health.OnValueChanged -= UpdateHUD;
                _lifeView.Data.stamina.OnValueChanged -= UpdateHUD;
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

            float hpRatio = hp.Max > 0 ? Mathf.Clamp01((float)hp.Current / hp.Max) : 0;
            float spRatio = sp.Max > 0 ? Mathf.Clamp01((float)sp.Current / sp.Max) : 0;

            if (_hpFill != null)
                _hpFill.transform.localScale = new Vector3(0.8f * hpRatio, 0.15f, 1f);
            if (_spFill != null)
                _spFill.transform.localScale = new Vector3(0.8f * spRatio, 0.1f, 1f);

            if (_hpText != null)
            {
                if (_lifeView.Data.HasStatusEffect(_prototype_StatusType.DeathsDoor))
                {
                    _hpText.text = "<color=#FF2222>💀 DEATH'S DOOR</color>";
                }
                else
                {
                    int displayHp = Mathf.Max(0, hp.Current);
                    int displaySp = Mathf.Max(0, sp.Current);
                    _hpText.text = $"<color=red>{displayHp}</color> <size=80%>| <color=yellow>{displaySp}</color></size>";
                }
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
                bgSr.sortingOrder = 18;
                bgGo.transform.localScale = new Vector3(badgeSize, badgeSize, 1f);

                // 레이블 TextMeshPro
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(badgeGo.transform, false);
                var label = labelGo.AddComponent<TextMeshPro>();
                label.fontSize = 1.8f;
                label.alignment = TextAlignmentOptions.Center;
                label.sortingOrder = 19;
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
            int maxTicks = group.Max(s => s.durationTicks);

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
            if (intent == null || intent.intentType == _prototype_EnemyIntentType.None)
            {
                _intentBadge.gameObject.SetActive(false);
                return;
            }

            _intentBadge.gameObject.SetActive(true);

            string symbol = intent.GetSymbol();
            string name = intent.GetDisplayName();
            Color color = intent.GetColor();

            if (_intentText != null)
            {
                string dmgText = intent.estimatedDamage > 0 ? $" {intent.estimatedDamage}" : "";
                _intentText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{symbol}{dmgText}</color>\n<size=1.8>{name}</size>";
            }
            if (_intentBg != null)
            {
                _intentBg.color = new Color(color.r * 0.2f, color.g * 0.2f, color.b * 0.2f, 0.75f);
            }
        }

        private void LateUpdate()
        {
            if (_hudRoot != null)
            {
                UpdateHUD();

                _hudRoot.position = transform.position + new Vector3(.0f, -0.25f, .0f);

                Camera mainCamera = _prototype_CameraController.Instance.MainCamera;
                if (mainCamera != null)
                {
                    _hudRoot.rotation = mainCamera.transform.rotation;
                }
            }
        }
    }
}
