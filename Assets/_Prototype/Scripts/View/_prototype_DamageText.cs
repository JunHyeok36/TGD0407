using UnityEngine;
using TMPro;
using DG.Tweening;

namespace TDG0407._prototype
{
    public class _prototype_DamageText : MonoBehaviour
    {
        public void Initialize(int damageAmount, Color color, bool isCritical = false)
        {
            var textMesh = GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = isCritical ? $"{damageAmount}!" : damageAmount.ToString();
                textMesh.color = new Color(color.r, color.g, color.b, 1f);
                if (isCritical)
                {
                    textMesh.fontSize *= 1.25f;
                }
            }
            
            // Make the text face the camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                transform.rotation = mainCam.transform.rotation;
            }
            
            float lifetime = isCritical ? 1.2f : 1.0f;
            float floatDistance = isCritical ? 2.5f : 2.0f;

            // DOTween 애니메이션: 위로 이동
            transform.DOMoveY(transform.position.y + floatDistance, lifetime).SetEase(Ease.OutCubic).SetLink(gameObject);
            
            if (textMesh != null)
            {
                // 수동 페이드 사용
                DOTween.To(() => textMesh.color, x => textMesh.color = x, new Color(color.r, color.g, color.b, 0f), lifetime).SetEase(Ease.InExpo).SetLink(gameObject);
            }

            Destroy(gameObject, lifetime);
        }

        public static void OnEntityDamaged(EntityDamagedEvent evt)
        {
            if (evt.Damage <= 0 || evt.Target is _prototype_ObstacleData) return;

            var pointView = _prototype_GridManager.Instance.GetPointView(evt.Point);
            if (pointView != null)
            {
                Color textColor;
                if (evt.IsCritical)
                {
                    textColor = new Color(1f, 0.82f, 0.15f); // Vibrant Gold for Critical
                }
                else
                {
                    textColor = evt.DamageType == _prototype_DamageType.Magical ? Color.cyan : Color.red;
                }
                Spawn(pointView.transform.position, evt.Damage, textColor, evt.IsCritical);
            }
        }

        public static void Spawn(Vector3 worldPosition, int damageAmount, Color color, bool isCritical = false)
        {
            GameObject prefab = Resources.Load<GameObject>("DamageTextPrefab");
            if (prefab != null)
            {
                GameObject dmgObj = UnityEngine.Object.Instantiate(prefab);
                dmgObj.transform.position = worldPosition + Vector3.up * 1.5f;
                var dmgText = dmgObj.GetComponent<_prototype_DamageText>();
                if (dmgText == null) dmgText = dmgObj.AddComponent<_prototype_DamageText>();
                dmgText.Initialize(damageAmount, color, isCritical);
            }
            else
            {
                Debug.LogError("DamageTextPrefab not found in Resources!");
            }
        }
    }
}
