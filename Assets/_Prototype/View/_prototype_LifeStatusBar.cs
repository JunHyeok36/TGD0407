using UnityEngine;
using UnityEngine.UI;

namespace TDG0407._prototype
{
    public class _prototype_LifeStatusBar : MonoBehaviour
    {
        public Image hpFill;
        public Image spFill;
        
        private _prototype_LifeView _lifeView;
        private RectTransform _rectTransform;

        private static Canvas _sharedWorldCanvas;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(_prototype_LifeView lifeView)
        {
            this._lifeView = lifeView;
            
            // Find references if not set
            if (hpFill == null) hpFill = transform.Find("HP_BG/HP_Fill")?.GetComponent<Image>();
            if (spFill == null) spFill = transform.Find("SP_BG/SP_Fill")?.GetComponent<Image>();

            // Setup Shared World Canvas from Scene
            if (_sharedWorldCanvas == null)
            {
                var canvasObj = GameObject.Find("GlobalWorldCanvas");
                if (canvasObj != null)
                {
                    _sharedWorldCanvas = canvasObj.GetComponent<Canvas>();
                }
            }

            if (_sharedWorldCanvas != null)
            {
                transform.SetParent(_sharedWorldCanvas.transform, false);
            }
            
            // In World Space canvas, setting rotation ensures it faces exactly the same way
            transform.localScale = Vector3.one;
        }

        private void LateUpdate()
        {
            if (_lifeView == null || _lifeView.Data == null)
            {
                Destroy(gameObject);
                return;
            }

            if (hpFill != null)
            {
                float hpPct = (float)_lifeView.Data.health.Current / _lifeView.Data.health.Max;
                hpFill.fillAmount = Mathf.Clamp01(hpPct);
            }

            if (spFill != null)
            {
                float spPct = (float)_lifeView.Data.stamina.Current / _lifeView.Data.stamina.Max;
                spFill.fillAmount = Mathf.Clamp01(spPct);
            }
            
            // Track entity position
            Vector3 targetPos = _lifeView.transform.position + new Vector3(0, -0.4f, 0);
            transform.position = targetPos;

            // Always face the main camera
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
    }
}
