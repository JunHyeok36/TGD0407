using UnityEngine;
using TMPro;
using DG.Tweening;

namespace TDG0407._prototype
{
    public class _prototype_DamageText : MonoBehaviour
    {
        public void Initialize(int damageAmount, Color color)
        {
            var textMesh = GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = damageAmount.ToString();
                textMesh.color = new Color(color.r, color.g, color.b, 1f);
            }
            
            // Make the text face the camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                transform.rotation = mainCam.transform.rotation;
            }
            
            float lifetime = 1.0f;
            float floatDistance = 2.0f;

            // DOTween 애니메이션: 위로 이동
            transform.DOMoveY(transform.position.y + floatDistance, lifetime).SetEase(Ease.OutCubic).SetLink(gameObject);
            
            if (textMesh != null)
            {
                // 수동 페이드 사용
                DOTween.To(() => textMesh.color, x => textMesh.color = x, new Color(color.r, color.g, color.b, 0f), lifetime).SetEase(Ease.InExpo).SetLink(gameObject);
            }

            Destroy(gameObject, lifetime);
        }
    }
}
