using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace TDG0407._prototype
{
    
    [RequireComponent(typeof(_prototype_EntityView))]
    public class _prototype_EnemyAIController : MonoBehaviour
    {
        
        [SerializeField, ReadOnly] private _prototype_EntityView _entityView;
        public _prototype_EntityView EntityView => _entityView;

        [Header("AI Mistake Settings")]
        [Tooltip("개별 적 오브젝트에서 AI 실수 빈도를 오버라이드할지 여부")]
        [SerializeField] private bool _overrideMistakeRate = false;
        [Range(0f, 1f)]
        [Tooltip("오버라이드할 실수 빈도 (0 = 실수 안 함, 1 = 항상 실수)")]
        [SerializeField] private float _customMistakeRate = 0.05f;

        private void Awake()
        {
            if (TryGetComponent(out _prototype_EntityView entityView))
                _entityView = entityView;
            else
                throw new Exception("EnemyAI requires an _prototype_EntityView component.");
        }

        private void OnDestroy()
        {
            if (_entityView != null && _prototype_GridVisualManager.Instance != null)
            {
                _prototype_GridVisualManager.Instance.ClearAllHazards(_entityView);
            }
        }

        public void Initialize(_prototype_EntityView entityView)
        {
            _entityView = entityView;
            if (_overrideMistakeRate && _entityView?.EntityData is _prototype_LifeData life && life.aiLogic != null)
            {
                life.aiLogic.mistakeRate = _customMistakeRate;
            }
            _prototype_TickManager.RegisterTick(DetermineNextAction);
        }

        private void Start()
        {
            // 게임 시작 직후 (첫 플레이어 턴 전)에 최초의 의도를 계산하고 위험 타일을 표시합니다.
            if (_entityView != null && _entityView.EntityData is _prototype_LifeData lifeData)
            {
                if (_overrideMistakeRate && lifeData.aiLogic != null)
                {
                    lifeData.aiLogic.mistakeRate = _customMistakeRate;
                }

                if (lifeData.aiLogic != null)
                {
                    lifeData.aiLogic.EvaluateIntent(_entityView);
                }
            }
        }

        private async UniTask<_prototype_TickIntent> DetermineNextAction()
        {
            var intent = new _prototype_TickIntent
            {
                TargetsPlayer = false,
                Execute = async () => { await UniTask.Yield(); }
            };

            if (_entityView == null || _entityView.EntityData == null) return intent;
            if (_entityView.EntityData.health.Current <= 0) 
            {
                _prototype_GridVisualManager.Instance.ClearAllHazards(_entityView);
                return intent; // 죽었으면 행동 안 함
            }

            var lifeData = _entityView.EntityData as _prototype_LifeData;
            if (lifeData != null)
            {
                if (lifeData.statusEffects.Find(s => s.type == _prototype_StatusType.Stun) != null)
                {
                    _prototype_GridVisualManager.Instance.ClearAllHazards(_entityView);
                    intent.Execute = async () => 
                    {
                        // 시각적 효과 (기절)
                        await _entityView.transform.DOShakePosition(0.3f, 0.1f, 10, 90f, false, true).AsyncWaitForCompletion();
                        if (lifeData.aiLogic != null)
                        {
                            lifeData.aiLogic.EvaluateIntent(_entityView);
                        }
                    };
                    return intent;
                }

                if (lifeData.aiLogic != null)
                {
                    var plannedIntent = await lifeData.aiLogic.PlanAction(_entityView);
                    if (plannedIntent != null)
                    {
                        return plannedIntent;
                    }
                }
            }
            return intent;
        }
    }
}