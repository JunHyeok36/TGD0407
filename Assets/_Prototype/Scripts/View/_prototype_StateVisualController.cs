using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace TDG0407._prototype
{
    [RequireComponent(typeof(_prototype_EntityView))]
    public class _prototype_StateVisualController : MonoBehaviour
    {
        private _prototype_EntityView _entityView;
        private _prototype_StateComponentData _stateData;
        private Renderer _renderer;
        private Color _originalColor = Color.white;

        private void Awake()
        {
            _entityView = GetComponent<_prototype_EntityView>();
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null && _renderer.material.HasProperty("_BaseColor"))
            {
                _originalColor = _renderer.material.GetColor("_BaseColor");
            }
            else if (_renderer != null && _renderer.material.HasProperty("_Color"))
            {
                _originalColor = _renderer.material.color;
            }
        }

        private void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            // Wait for EntityView to be fully initialized with Data
            await UniTask.WaitUntil(() => _entityView.EntityData != null);

            if (_entityView.EntityData.components != null)
            {
                _stateData = _entityView.EntityData.components.FirstOrDefault(c => c is _prototype_StateComponentData) as _prototype_StateComponentData;
                if (_stateData != null)
                {
                    _stateData.OnStateChanged += HandleStateChanged;
                    // Apply initial state
                    var currentState = _stateData.GetState("Switch");
                    if (!string.IsNullOrEmpty(currentState))
                    {
                        ApplyStateVisual(currentState, false);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_stateData != null)
            {
                _stateData.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(string key, string newValue)
        {
            if (key == "Switch")
            {
                ApplyStateVisual(newValue, true);
            }
        }

        private void ApplyStateVisual(string state, bool animate)
        {
            if (_renderer == null) return;

            Color targetColor = _originalColor;
            if (state == "On")
            {
                targetColor = Color.red;
            }
            else if (state == "Off")
            {
                targetColor = Color.gray;
            }

            if (animate)
            {
                if (_renderer.material.HasProperty("_BaseColor"))
                {
                    _renderer.material.DOColor(targetColor, "_BaseColor", 0.3f);
                }
                else
                {
                    _renderer.material.DOColor(targetColor, 0.3f);
                }
                
                transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 2, 1f);
            }
            else
            {
                if (_renderer.material.HasProperty("_BaseColor"))
                {
                    _renderer.material.SetColor("_BaseColor", targetColor);
                }
                else
                {
                    _renderer.material.color = targetColor;
                }
            }
        }
    }
}
