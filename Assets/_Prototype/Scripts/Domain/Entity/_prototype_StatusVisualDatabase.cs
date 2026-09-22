using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 각 StatusType에 대응하는 시각 메타데이터 (아이콘, 색상, 표시명, 설명)를 보관하는 ScriptableObject 레지스트리.
    /// Project에서 단 하나의 에셋이 존재하며, LifeHUD 및 PlayerUIView에서 참조합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "StatusVisualDatabase", menuName = "TDG0407/Prototype/Status Visual Database")]
    public class _prototype_StatusVisualDatabase : ScriptableObject
    {
        [Serializable]
        public class StatusEntry
        {
            public _prototype_StatusType type;

            [Tooltip("표시용 단축 이름 (예: 스턴, 출혈)")]
            public string displayName;

            [Tooltip("아이콘 심볼 문자 (스프라이트가 없을 때 폴백으로 사용)")]
            public string symbolChar = "?";

            [Tooltip("아이콘 스프라이트 (선택사항)")]
            public Sprite icon;

            [Tooltip("테마 컬러 (배지 테두리 및 배경색)")]
            public Color themeColor = Color.white;

            [TextArea(2, 4)]
            [Tooltip("마우스 호버 시 표시되는 상세 설명")]
            public string description;
        }

        [SerializeField] private List<StatusEntry> _entries = new();

        private Dictionary<_prototype_StatusType, StatusEntry> _lookup;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<_prototype_StatusType, StatusEntry>();
            if (_entries == null) return;
            foreach (var entry in _entries)
            {
                _lookup[entry.type] = entry;
            }
        }

        public StatusEntry GetEntry(_prototype_StatusType type)
        {
            if (_lookup == null) BuildLookup();
            _lookup.TryGetValue(type, out var entry);
            return entry;
        }

        // ─── 런타임 싱글턴 참조 ───
        private static _prototype_StatusVisualDatabase s_instance;

        public static _prototype_StatusVisualDatabase Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = Resources.Load<_prototype_StatusVisualDatabase>("StatusVisualDatabase");
                    if (s_instance == null)
                    {
                        // Resources 경로 없을 경우 에셋 데이터베이스 스캔 폴백 (에디터만)
#if UNITY_EDITOR
                        var guids = UnityEditor.AssetDatabase.FindAssets("t:_prototype_StatusVisualDatabase");
                        if (guids.Length > 0)
                        {
                            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                            s_instance = UnityEditor.AssetDatabase.LoadAssetAtPath<_prototype_StatusVisualDatabase>(path);
                        }
#endif
                    }
                }
                return s_instance;
            }
        }

        /// <summary>
        /// 외부에서 직접 인스턴스를 주입할 때 사용합니다 (BootStrapper 등).
        /// </summary>
        public static void SetInstance(_prototype_StatusVisualDatabase db)
        {
            s_instance = db;
        }

        // ─── 기본값 제공 (데이터베이스 에셋 없을 경우 폴백) ───
        private static readonly Dictionary<_prototype_StatusType, (string symbol, Color color, string name, string desc)> s_defaults
            = new()
            {
                { _prototype_StatusType.Stun,        ("⚡", new Color(1f,0.9f,0f),   "스턴",   "행동 불능 상태입니다.") },
                { _prototype_StatusType.Knockdown,   ("💫", new Color(0.8f,0.5f,1f), "녹다운", "스태미나 고갈로 쓰러져 있습니다.") },
                { _prototype_StatusType.Silence,     ("🔇", new Color(0.5f,0.5f,0.8f),"침묵",  "카드를 사용할 수 없습니다.") },
                { _prototype_StatusType.Fear,        ("😱", new Color(0.6f,0.2f,0.8f),"공포",  "두려움에 도망칩니다.") },
                { _prototype_StatusType.Curse,       ("☠",  new Color(0.4f,0f,0.6f), "저주",   "최대 HP가 감소합니다.") },
                { _prototype_StatusType.Bleeding,    ("🩸", new Color(0.9f,0.1f,0.1f),"출혈",  "매 턴 체력이 감소합니다.") },
                { _prototype_StatusType.Burning,     ("🔥", new Color(1f,0.4f,0f),   "화상",   "매 턴 화염 피해를 받습니다.") },
                { _prototype_StatusType.Freeze,      ("❄",  new Color(0.4f,0.7f,1f), "빙결",   "이동할 수 없습니다.") },
                { _prototype_StatusType.Poisoning,   ("☣",  new Color(0.3f,0.7f,0.2f),"중독", "매 턴 독 피해를 받습니다.") },
                { _prototype_StatusType.SuperArmor,  ("🛡",  new Color(0.9f,0.7f,0.1f),"슈아머","모든 CC를 무시합니다.") },
                { _prototype_StatusType.DeathsDoor,  ("💀", new Color(0.6f,0f,0f),   "빈사",   "치명적으로 위험한 상태입니다. 한 번만 더 피격되면 즉사합니다.") },
            };

        public static (string symbol, Color color, string name, string desc) GetDefaultEntry(_prototype_StatusType type)
        {
            if (s_defaults.TryGetValue(type, out var v)) return v;
            return ("?", Color.grey, type.ToString(), "");
        }
    }
}
