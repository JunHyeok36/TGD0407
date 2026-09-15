using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// _prototype_StateComponentData의 상태 변화를 감지하여 Unity Animator의 파라미터 및 스테이트를 제어하는 뷰 컨트롤러입니다.
    /// </summary>
    public class _prototype_StateAnimatorController : MonoBehaviour
    {
        [Serializable]
        public class StateAnimationMapping
        {
            [Tooltip("감지할 상태 키 (예: Switch, Door, State)")]
            public string stateKey;

            [Tooltip("일치해야 하는 상태 값 (예: On, Off, Open, Closed). 비워둘 경우 해당 키의 모든 값 변경 시 발동")]
            public string stateValue;

            [Tooltip("재생할 Animator State 이름")]
            public string animationStateName;

            [Tooltip("전환 시간(초). 0이면 즉시 Play")]
            public float crossFadeDuration = 0.1f;

            [Tooltip("Animator 레이어 인덱스 (기본: 0)")]
            public int layer = 0;
        }

        [Serializable]
        public class StateParameterMapping
        {
            [Tooltip("감지할 상태 키")]
            public string stateKey;

            [Tooltip("Animator 파라미터 이름")]
            public string parameterName;

            [Tooltip("파라미터 타입")]
            public AnimatorControllerParameterType parameterType = AnimatorControllerParameterType.Bool;

            [Tooltip("Trigger 타입일 때, 이 값과 일치할 경우 Trigger 발동 (기본: On/True/1 등 활성 상태)")]
            public string triggerValue = "On";
        }

        [Header("References")]
        [SerializeField]
        [Tooltip("연동할 Animator 컴포넌트 (비워둘 시 자동 검색)")]
        private Animator _animator;

        [SerializeField]
        [Tooltip("연동할 EntityView 컴포넌트 (비워둘 시 자동 검색)")]
        private _prototype_EntityView _entityView;

        [Header("Settings")]
        [SerializeField]
        [Tooltip("Animator에 상태 키와 동일한 이름의 파라미터가 있으면 자동으로 동기화할지 여부")]
        private bool _autoBindParameters = true;

        [Header("Explicit Mappings")]
        [SerializeField]
        [Tooltip("특정 상태 및 값 변경 시 직접 Animator State를 재생/CrossFade할 매핑 목록")]
        private List<StateAnimationMapping> _stateAnimationMappings = new();

        [SerializeField]
        [Tooltip("상태 키와 다른 이름의 파라미터를 제어하거나 세부 동작을 지정할 매핑 목록")]
        private List<StateParameterMapping> _customParameterMappings = new();

        private _prototype_StateComponentData _stateData;
        private readonly Dictionary<string, AnimatorControllerParameterType> _cachedAnimatorParams = new(StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
                if (_animator == null)
                {
                    _animator = GetComponentInChildren<Animator>();
                }
            }

            if (_entityView == null)
            {
                _entityView = GetComponent<_prototype_EntityView>();
                if (_entityView == null)
                {
                    _entityView = GetComponentInParent<_prototype_EntityView>();
                }
            }

            CacheAnimatorParameters();
        }

        private void Start()
        {
            InitializeAsync().Forget();
        }

        private void CacheAnimatorParameters()
        {
            _cachedAnimatorParams.Clear();
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            foreach (var param in _animator.parameters)
            {
                if (!_cachedAnimatorParams.ContainsKey(param.name))
                {
                    _cachedAnimatorParams.Add(param.name, param.type);
                }
            }
        }

        private async UniTaskVoid InitializeAsync()
        {
            if (_entityView == null)
            {
                _entityView = GetComponent<_prototype_EntityView>() ?? GetComponentInParent<_prototype_EntityView>();
                if (_entityView == null)
                {
                    Debug.LogWarning($"[{nameof(_prototype_StateAnimatorController)}] EntityView not found on {gameObject.name}.", this);
                    return;
                }
            }

            // EntityData가 로드될 때까지 대기
            await UniTask.WaitUntil(() => _entityView.EntityData != null);

            if (_entityView.EntityData.components != null)
            {
                _stateData = _entityView.EntityData.components.FirstOrDefault(c => c is _prototype_StateComponentData) as _prototype_StateComponentData;
                if (_stateData != null)
                {
                    _stateData.OnStateChanged += HandleStateChanged;

                    // 초기 상태 동기화 (초기화 시에는 부드러운 전환 대신 즉시 적용)
                    ApplyAllInitialStates();
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

        /// <summary>
        /// 엔티티 생성 시점의 모든 기존 상태를 Animator에 즉시 반영합니다.
        /// </summary>
        private void ApplyAllInitialStates()
        {
            if (_stateData == null || _animator == null) return;

            if (_stateData.States != null)
            {
                foreach (var state in _stateData.States)
                {
                    if (state != null && !string.IsNullOrEmpty(state.key))
                    {
                        ApplyStateToAnimator(state.key, state.value, isInitial: true);
                    }
                }
            }
        }

        private void HandleStateChanged(string key, string newValue)
        {
            ApplyStateToAnimator(key, newValue, isInitial: false);
        }

        /// <summary>
        /// 상태 키와 값에 따라 Animator 파라미터 및 State 전환을 처리합니다.
        /// </summary>
        public void ApplyStateToAnimator(string key, string value, bool isInitial)
        {
            if (_animator == null || !_animator.gameObject.activeInHierarchy)
                return;

            // 1. 자동 파라미터 바인딩 처리 (키 이름과 일치하는 파라미터가 있을 때)
            if (_autoBindParameters && _cachedAnimatorParams.TryGetValue(key, out var paramType))
            {
                ApplyParameterValue(key, paramType, value, key);
            }

            // 2. 커스텀 파라미터 매핑 처리
            if (_customParameterMappings != null && _customParameterMappings.Count > 0)
            {
                foreach (var mapping in _customParameterMappings)
                {
                    if (string.Equals(mapping.stateKey, key, StringComparison.OrdinalIgnoreCase))
                    {
                        ApplyCustomParameterMapping(mapping, value);
                    }
                }
            }

            // 3. 명시적 애니메이션 스테이트 재생 매핑 처리
            if (_stateAnimationMappings != null && _stateAnimationMappings.Count > 0)
            {
                foreach (var mapping in _stateAnimationMappings)
                {
                    if (string.Equals(mapping.stateKey, key, StringComparison.OrdinalIgnoreCase))
                    {
                        bool matchesValue = string.IsNullOrEmpty(mapping.stateValue) ||
                                            string.Equals(mapping.stateValue, value, StringComparison.OrdinalIgnoreCase);

                        if (matchesValue && !string.IsNullOrEmpty(mapping.animationStateName))
                        {
                            PlayAnimationState(mapping.animationStateName, isInitial ? 0f : mapping.crossFadeDuration, mapping.layer);
                        }
                    }
                }
            }
        }

        private void ApplyParameterValue(string paramName, AnimatorControllerParameterType paramType, string value, string stateKey)
        {
            switch (paramType)
            {
                case AnimatorControllerParameterType.Bool:
                    bool boolVal = _stateData != null ? _stateData.GetBoolState(stateKey) : ParseBool(value);
                    _animator.SetBool(paramName, boolVal);
                    break;

                case AnimatorControllerParameterType.Int:
                    if (int.TryParse(value, out int intVal))
                    {
                        _animator.SetInteger(paramName, intVal);
                    }
                    break;

                case AnimatorControllerParameterType.Float:
                    if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float floatVal))
                    {
                        _animator.SetFloat(paramName, floatVal);
                    }
                    break;

                case AnimatorControllerParameterType.Trigger:
                    if (ParseBool(value))
                    {
                        _animator.SetTrigger(paramName);
                    }
                    break;
            }
        }

        private void ApplyCustomParameterMapping(StateParameterMapping mapping, string value)
        {
            if (string.IsNullOrEmpty(mapping.parameterName)) return;

            switch (mapping.parameterType)
            {
                case AnimatorControllerParameterType.Bool:
                    bool boolVal = _stateData != null ? _stateData.GetBoolState(mapping.stateKey) : ParseBool(value);
                    _animator.SetBool(mapping.parameterName, boolVal);
                    break;

                case AnimatorControllerParameterType.Int:
                    if (int.TryParse(value, out int intVal))
                    {
                        _animator.SetInteger(mapping.parameterName, intVal);
                    }
                    break;

                case AnimatorControllerParameterType.Float:
                    if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float floatVal))
                    {
                        _animator.SetFloat(mapping.parameterName, floatVal);
                    }
                    break;

                case AnimatorControllerParameterType.Trigger:
                    bool shouldTrigger = string.IsNullOrEmpty(mapping.triggerValue)
                        ? ParseBool(value)
                        : string.Equals(mapping.triggerValue, value, StringComparison.OrdinalIgnoreCase);

                    if (shouldTrigger)
                    {
                        _animator.SetTrigger(mapping.parameterName);
                    }
                    break;
            }
        }

        private void PlayAnimationState(string stateName, float crossFadeDuration, int layer)
        {
            if (crossFadeDuration > 0f)
            {
                _animator.CrossFade(stateName, crossFadeDuration, layer);
            }
            else
            {
                _animator.Play(stateName, layer);
            }
        }

        private static bool ParseBool(string val)
        {
            if (string.IsNullOrEmpty(val)) return false;
            if (bool.TryParse(val, out bool result)) return result;
            if (val.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                val.Equals("on", StringComparison.OrdinalIgnoreCase) ||
                val.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
