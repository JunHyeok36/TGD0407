using UnityEngine;
using TMPro;
using DG.Tweening;

namespace TDG0407._prototype
{
    public class _prototype_FloatingText : MonoBehaviour
    {

        [SerializeField] private TextMeshPro textMesh;

        public void Initialize(string text, Color color, float fontSizeMultiplier = 1f, float customLifetime = 1.0f, float customDistance = .5f)
        {
            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = new Color(color.r, color.g, color.b, 1f);
                if (fontSizeMultiplier != 1f)
                {
                    textMesh.fontSize *= fontSizeMultiplier;
                }
            }
            
            // Make the text face the camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                transform.rotation = mainCam.transform.rotation;
            }
            
            float lifetime = customLifetime;
            float floatDistance = customDistance;

            // DOTween 애니메이션: 위로 이동
            transform.DOMoveY(transform.position.y + floatDistance, lifetime).SetEase(Ease.OutCubic).SetLink(gameObject);
            
            if (textMesh != null)
            {
                // 수동 페이드 사용
                DOTween.To(() => textMesh.color, x => textMesh.color = x, new Color(color.r, color.g, color.b, 0f), lifetime).SetEase(Ease.InExpo).SetLink(gameObject);
            }

            Destroy(gameObject, lifetime);
        }

        public void Initialize(int amount, Color color, bool isCritical = false)
        {
            string displayText = isCritical ? $"{amount}!" : amount.ToString();
            float fontSizeMul = isCritical ? 1.25f : 1f;
            float lifetime = isCritical ? 1.2f : 1.0f;

            Initialize(displayText, color, fontSizeMul, lifetime);
        }

        public static void OnEntityDamaged(EntityDamagedEvent evt)
        {
            if (evt.Damage <= 0 || evt.Target is not _prototype_LifeData) return;

            var pointView = _prototype_GridManager.Instance.GetPointView(evt.Point);
            if (pointView != null)
            {
                Color textColor = GetDamageColor(evt.DamageType, evt.IsCritical);
                var targetView = pointView.PlacedEntityViews?.Find(v => v.EntityData == evt.Target);
                if (targetView != null)
                {
                    string displayText = evt.IsCritical ? $"{evt.Damage}!" : evt.Damage.ToString();
                    float fontSizeMul = evt.IsCritical ? 1.25f : 1f;
                    float lifetime = evt.IsCritical ? 1.2f : 1.0f;
                    float distance = evt.IsCritical ? 2.5f : 2.0f;
                    SpawnOnEntity(targetView, displayText, textColor, fontSizeMul, lifetime, distance);
                }
                else
                {
                    Spawn(pointView.transform.position, evt.Damage, textColor, evt.IsCritical);
                }
            }
        }

        private static Color GetDamageColor(_prototype_DamageType damageType, bool isCritical)
        {
            if (isCritical)
            {
                // 크리티컬 시 기본 데미지 타입 색상의 채도/명도를 높이거나 강조 골드 계열과 블렌딩
                return damageType switch
                {
                    _prototype_DamageType.Physical => new Color(1f, 0.45f, 0.05f),// 강렬한 주황-금빛 (레드-오렌지 크리티컬)
                    _prototype_DamageType.Magical => new Color(0.65f, 0.4f, 1f),// 밝은 네온 바이올렛/마젠타 블루
                    _prototype_DamageType.True => new Color(1f, 1f, 0.8f),// 밝은 백금/순백
                    _ => new Color(1f, 0.85f, 0.2f),
                };

            }

            return damageType switch
            {
                _prototype_DamageType.Physical => new Color(1f, 0.28f, 0.1f),// 빨강 ~ 주황 사이 (따뜻한 진홍-다홍 주황)
                _prototype_DamageType.Magical => new Color(0.45f, 0.35f, 1f),// 보라 ~ 파랑 사이 (신비로운 바이올렛-인디고 블루)
                _prototype_DamageType.True => Color.white,// 흰색
                _ => new Color(1f, 0.28f, 0.1f),
            };

        }

        public static void OnEntityStatusChanged(EntityStatusChangedEvent evt)
        {
            if (!evt.IsAdded || evt.Effect == null || evt.Effect.type == _prototype_StatusType.None) return;
            if (evt.Target is not _prototype_LifeData) return;

            var pointView = _prototype_GridManager.Instance.GetPointView(evt.Target.point);
            if (pointView != null)
            {
                string statusName = GetStatusEffectName(evt.Effect.type);
                Color statusColor = GetStatusEffectColor(evt.Effect.type);
                var targetView = pointView.PlacedEntityViews?.Find(v => v.EntityData == evt.Target);
                if (targetView != null)
                {
                    SpawnOnEntity(targetView, statusName, statusColor, 1.15f, 1.2f, 2.2f);
                }
                else
                {
                    Spawn(pointView.transform.position, statusName, statusColor, 1.15f, 1.2f, 2.2f);
                }
            }
        }

        private static string GetStatusEffectName(_prototype_StatusType type)
        {
            var db = _prototype_StatusVisualDatabase.Instance;
            if (db != null)
            {
                var entry = db.GetEntry(type);
                if (entry != null && !string.IsNullOrEmpty(entry.displayName))
                    return $"{entry.displayName}!";
            }
            return type switch
            {
                _prototype_StatusType.Stun => "스턴!",
                _prototype_StatusType.Groggy => "그로기!",
                _prototype_StatusType.Silence => "침묵!",
                _prototype_StatusType.Fear => "공포!",
                _prototype_StatusType.Curse => "저주!",
                _prototype_StatusType.Bleeding => "출혈!",
                _prototype_StatusType.Burning => "화상!",
                _prototype_StatusType.Freeze => "빙결!",
                _prototype_StatusType.Poisoning => "중독!",
                _prototype_StatusType.Unstoppable => "저지 불가!",
                _prototype_StatusType.DeathsDoor => "사경!",
                _prototype_StatusType.Provocation => "도발!",
                _prototype_StatusType.Airborne => "에어본!",
                _prototype_StatusType.Invincible => "무적!",
                _prototype_StatusType.EnhanceStab => "찌르기 강화!",
                _ => $"{type}!"
            };
        }

        private static Color GetStatusEffectColor(_prototype_StatusType type)
        {
            switch (type)
            {
                case _prototype_StatusType.Stun:
                    return new Color(1f, 0.9f, 0.2f); // Yellow
                case _prototype_StatusType.Groggy:
                    return new Color(1f, 0.5f, 0.1f); // Orange
                case _prototype_StatusType.Silence:
                    return new Color(0.6f, 0.6f, 0.8f); // Light slate/gray-blue
                case _prototype_StatusType.Fear:
                    return new Color(0.7f, 0.3f, 0.9f); // Purple
                case _prototype_StatusType.Curse:
                    return new Color(0.5f, 0.1f, 0.7f); // Deep violet
                case _prototype_StatusType.Bleeding:
                    return new Color(0.9f, 0.1f, 0.1f); // Blood red
                case _prototype_StatusType.Burning:
                    return new Color(1f, 0.4f, 0.0f); // Flame orange-red
                case _prototype_StatusType.Freeze:
                    return new Color(0.3f, 0.85f, 1f); // Ice cyan
                case _prototype_StatusType.Poisoning:
                    return new Color(0.3f, 0.9f, 0.3f); // Toxic green
                case _prototype_StatusType.Unstoppable:
                    return new Color(0.9f, 0.85f, 0.4f); // Golden amber
                case _prototype_StatusType.Provocation:
                    return new Color(1f, 0.3f, 0.1f); // Crimson-orange
                case _prototype_StatusType.Airborne:
                    return new Color(0.4f, 0.85f, 1f); // Sky blue
                case _prototype_StatusType.Invincible:
                    return new Color(1f, 0.95f, 0.4f); // Brilliant Gold
                case _prototype_StatusType.EnhanceStab:
                    return new Color(1f, 0.55f, 0.1f); // Vivid orange
                default:
                    return Color.white;
            }
        }

        private static GameObject s_prefab;

        public static void SetPrefab(GameObject prefab)
        {
            s_prefab = prefab;
        }

        /// <summary>
        /// 정확한 월드 좌표 위치에 플로팅 텍스트를 스폰합니다.
        /// </summary>
        public static _prototype_FloatingText SpawnAtPosition(Vector3 exactWorldPosition, string text, Color color, float fontSizeMultiplier = 1f, float lifetime = 1.0f, float distance = .5f)
        {
            GameObject prefab = s_prefab;
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab);
                obj.transform.position = exactWorldPosition;
                if (obj.TryGetComponent<_prototype_FloatingText>(out var floatingText))
                {
                    floatingText.Initialize(text, color, fontSizeMultiplier, lifetime, distance);
                    return floatingText;
                }
                else
                {
                    Debug.LogError("FloatingTextPrefab does not have a _prototype_FloatingText component!");
                    Destroy(obj);
                    return null;
                }
            }
            else
            {
                Debug.LogError("FloatingTextPrefab is not assigned! Please assign it to BootStrapper.");
                return null;
            }
        }

        /// <summary>
        /// 대상 EntityView의 외형 렌더러 상단에 맞추어 플로팅 텍스트를 정확하게 스폰합니다.
        /// 엔티티 높이(오브젝트/캐릭터)에 따라 자연스럽게 머리 위에 표시됩니다.
        /// </summary>
        public static _prototype_FloatingText SpawnOnEntity(_prototype_EntityView entityView, string text, Color color, float fontSizeMultiplier = 1f, float lifetime = 1.0f, float distance = .5f)
        {
            if (entityView == null) return null;
            // FloatingText는 Life 엔티티에서만 발생하도록 제한 (Projectile, Obstacle 등 제외)
            if (entityView.EntityData != null && entityView.EntityData is not _prototype_LifeData) return null;

            Vector3 spawnPos = entityView.transform.position;
            var renderers = entityView.GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                float maxY = float.MinValue;
                Vector3 center = entityView.transform.position;
                foreach (var r in renderers)
                {
                    if (r is ParticleSystemRenderer || r is TrailRenderer) continue;
                    if (r.bounds.max.y > maxY)
                    {
                        maxY = r.bounds.max.y;
                        center.x = r.bounds.center.x;
                        center.z = r.bounds.center.z;
                    }
                }
                if (maxY > float.MinValue)
                {
                    spawnPos = new Vector3(center.x, maxY + 0.3f, center.z);
                }
                else
                {
                    spawnPos += Vector3.up * 1.0f;
                }
            }
            else
            {
                spawnPos += Vector3.up * 1.0f;
            }

            return SpawnAtPosition(spawnPos, text, color, fontSizeMultiplier, lifetime, distance);
        }

        public static _prototype_FloatingText Spawn(Vector3 worldPosition, string text, Color color, float fontSizeMultiplier = 1f, float lifetime = 1.0f, float distance = .5f)
        {
            return SpawnAtPosition(worldPosition + Vector3.up * 1.5f, text, color, fontSizeMultiplier, lifetime, distance);
        }

        public static _prototype_FloatingText Spawn(Vector3 worldPosition, int amount, Color color, bool isCritical = false)
        {
            string displayText = isCritical ? $"{amount}!" : amount.ToString();
            float fontSizeMul = isCritical ? 1.25f : 1f;
            float lifetime = isCritical ? 1.2f : 1.0f;
            float distance = isCritical ? 2.5f : 2.0f;

            return Spawn(worldPosition, displayText, color, fontSizeMul, lifetime, distance);
        }

        /// <summary>
        /// 넉백 등의 저항 발생 시 대상 엔티티 머리 위에 "Resist!" 텍스트를 스폰합니다.
        /// </summary>
        public static _prototype_FloatingText SpawnResistText(_prototype_EntityView targetView)
        {
            if (targetView == null || targetView.EntityData is not _prototype_LifeData) return null;
            return SpawnOnEntity(targetView, "Resist!", new Color(0.85f, 0.85f, 0.95f), 1.1f, 1.0f, 1.8f);
        }
    }
}
