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

        private IDisposable _statusSub;
        private IDisposable _movedSub;

        private void OnDestroy()
        {
            _statusSub?.Dispose();
            _movedSub?.Dispose();
            _prototype_TickManager.UnregisterTick(DetermineNextAction);
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
            _statusSub = _prototype_EventBus.Listen<EntityStatusChangedEvent>(OnEntityStatusChanged);
            _movedSub = _prototype_EventBus.Listen<EntityMovedEvent>(OnEntityMoved);
            _prototype_TickManager.RegisterTick(DetermineNextAction);
        }

        private void OnEntityMoved(EntityMovedEvent evt)
        {
            if (_entityView == null || evt.Entity != _entityView.EntityData) return;
            if (_entityView.EntityData is not _prototype_LifeData lifeData || lifeData.aiLogic == null) return;

            if (lifeData.IsDead || lifeData.HasStatusEffect(_prototype_StatusType.Stun) || lifeData.HasStatusEffect(_prototype_StatusType.Silence))
            {
                CancelPlannedAttack();
                return;
            }

            if (lifeData.aiLogic.hasPlannedIntent && lifeData.aiLogic.plannedCard is _prototype_BattleCardData battleCard)
            {
                _prototype_Point displacement = evt.To - evt.From;

                if (battleCard.targetAnchorType == _prototype_TargetAnchorType.FollowCaster)
                {
                    lifeData.aiLogic.plannedTarget += displacement;
                }

                List<_prototype_Point> newTargetPoints;
                if (battleCard.targetRange != null)
                {
                    newTargetPoints = battleCard.targetRange.GetValidTargetPoints(evt.To, lifeData.aiLogic.plannedTarget);
                }
                else
                {
                    newTargetPoints = new List<_prototype_Point> { lifeData.aiLogic.plannedTarget };
                }

                bool showsHazard = false;
                if (battleCard.actionList != null)
                {
                    foreach (var action in battleCard.actionList)
                    {
                        if (action is _prototype_DamageEntityAction) showsHazard = true;
                        if (action is _prototype_SpawnTargetedProjectileEntityAction ||
                            action is _prototype_SpawnDirectionalProjectileEntityAction)
                        {
                            showsHazard = false;
                            break;
                        }
                    }
                }

                if (_prototype_GridVisualManager.Instance != null)
                {
                    _prototype_GridVisualManager.Instance.ClearAllHazards(_entityView);
                    if (showsHazard)
                    {
                        foreach (var pt in newTargetPoints)
                        {
                            _prototype_GridVisualManager.Instance.ShowHazard(pt, _entityView);
                        }
                    }
                }
            }
        }

        private void OnEntityStatusChanged(EntityStatusChangedEvent evt)
        {
            if (_entityView == null || evt.Target != _entityView.EntityData) return;
            if (evt.Effect == null) return;

            if (evt.IsAdded)
            {
                if (evt.Effect.type == _prototype_StatusType.Stun || evt.Effect.type == _prototype_StatusType.Silence)
                {
                    CancelPlannedAttack();
                }
            }
            else
            {
                if (evt.Effect.type == _prototype_StatusType.Stun || evt.Effect.type == _prototype_StatusType.Silence)
                {
                    if (!_prototype_TickManager.IsTickProcessing && _entityView.EntityData != null && !_entityView.EntityData.IsDead)
                    {
                        EvaluateInitialIntent();
                    }
                }
            }
        }

        public void CancelPlannedAttack()
        {
            if (_entityView != null && _prototype_GridVisualManager.Instance != null)
            {
                _prototype_GridVisualManager.Instance.ClearAllHazards(_entityView);
            }

            if (_entityView?.EntityData is _prototype_LifeData lifeData && lifeData.aiLogic != null)
            {
                lifeData.aiLogic.hasPlannedIntent = false;
                lifeData.aiLogic.plannedCard = null;
                lifeData.aiLogic.plannedTarget = _entityView.Point;
            }
        }

        /// <summary>
        /// BootStrapper에서 시스템 초기화 완료 후 첫 턴 전에 호출하여 초기 의도를 계산하고 위험 타일을 표시합니다.
        /// </summary>
        public void EvaluateInitialIntent()
        {
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
            if (_entityView.EntityData.IsDead) 
            {
                _prototype_GridVisualManager.Instance.ClearAllHazards(_entityView);
                return intent; // 죽었으면 행동 안 함
            }

            var lifeData = _entityView.EntityData as _prototype_LifeData;
            if (lifeData != null)
            {
                if (lifeData.statusEffects.Find(s => s.type == _prototype_StatusType.Stun) != null)
                {
                    CancelPlannedAttack();
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

                if (lifeData.statusEffects.Find(s => s.type == _prototype_StatusType.Silence) != null)
                {
                    CancelPlannedAttack();
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