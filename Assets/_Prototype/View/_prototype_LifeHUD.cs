using UnityEngine;
using TMPro;

namespace TDG0407._prototype
{
    [RequireComponent(typeof(_prototype_LifeView))]
    public class _prototype_LifeHUD : MonoBehaviour
    {
        private _prototype_LifeView _lifeView;

        private Transform _hudRoot;
        // private SpriteRenderer _hpBg;
        private SpriteRenderer _hpFill;
        // private SpriteRenderer _spBg;
        private SpriteRenderer _spFill;
        private TextMeshPro _hpText;

        private static Sprite _squareSprite;

        private void Awake()
        {
            _lifeView = GetComponent<_prototype_LifeView>();
            InitializeHUD();
        }

        private void InitializeHUD()
        {
            GameObject prefab = Resources.Load<GameObject>("LifeHUD");
            if (prefab == null)
            {
                Debug.LogError("LifeHUD prefab not found in Resources folder.");
                return;
            }

            GameObject hudInstance = Instantiate(prefab, transform);
            hudInstance.name = "LifeHUD";
            _hudRoot = hudInstance.transform;
            _hudRoot.localPosition = new Vector3(0, 0.25f, -0.5f);

            _hpFill = _hudRoot.Find("HP_Fill").GetComponent<SpriteRenderer>();
            _spFill = _hudRoot.Find("SP_Fill").GetComponent<SpriteRenderer>();
            _hpText = _hudRoot.Find("HUD_Text").GetComponent<TextMeshPro>();
        }

        public void SetupEvents()
        {
            if (_lifeView != null && _lifeView.Data != null)
            {
                // 중복 등록 방지를 위해 뺐다가 다시 넣기
                _lifeView.Data.health.OnValueChanged -= UpdateHUD;
                _lifeView.Data.stamina.OnValueChanged -= UpdateHUD;

                _lifeView.Data.health.OnValueChanged += UpdateHUD;
                _lifeView.Data.stamina.OnValueChanged += UpdateHUD;
                UpdateHUD(); // Initial update
            }
        }

        private void OnDestroy()
        {
            if (_lifeView != null && _lifeView.Data != null)
            {
                _lifeView.Data.health.OnValueChanged -= UpdateHUD;
                _lifeView.Data.stamina.OnValueChanged -= UpdateHUD;
            }
        }

        public void UpdateHUD()
        {
            if (_lifeView == null || _lifeView.Data == null) return;

            var hp = _lifeView.Data.health;
            var sp = _lifeView.Data.stamina;

            float hpRatio = hp.Max > 0 ? (float)hp.Current / hp.Max : 0;
            float spRatio = sp.Max > 0 ? (float)sp.Current / sp.Max : 0;

            if (_hpFill != null)
                _hpFill.transform.localScale = new Vector3(0.8f * hpRatio, 0.1f, 1f);
            if (_spFill != null)
                _spFill.transform.localScale = new Vector3(0.8f * spRatio, 0.05f, 1f);

            if (_hpText != null)
                _hpText.text = $"<color=red>{hp.Current}</color> | <color=yellow>{sp.Current}</color>";
        }

        private void LateUpdate()
        {
            if (_hudRoot != null)
            {
                _hudRoot.position = _lifeView.transform.position + new Vector3(0, 0.25f, -0.5f);
                //_hudRoot.rotation = Quaternion.Euler(30f, 45f, 0f);
            }
        }
    }
}
