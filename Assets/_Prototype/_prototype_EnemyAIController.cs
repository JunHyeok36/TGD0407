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
        
        [SerializeReference, ReadOnly] private _prototype_EntityView _entityView;
        public _prototype_EntityView EntityView => _entityView;

        private void Awake()
        {
            if (TryGetComponent(out _prototype_EntityView entityView))
                _entityView = entityView;
            else
                throw new Exception("EnemyAI requires an _prototype_EntityView component.");
        }

        public void Initialize(_prototype_EntityView entityView)
        {
            _entityView = entityView;
            _prototype_TickManager.RegisterTick(DetermineNextAction);
        }

        private async UniTask<_prototype_TickIntent> DetermineNextAction()
        {
            var intent = new _prototype_TickIntent
            {
                TargetsPlayer = false,
                Execute = async () => { await UniTask.Yield(); }
            };

            if (_entityView == null || _entityView.EntityData == null) return intent;
            if (_entityView.EntityData.health.Current <= 0) return intent; // 죽었으면 행동 안 함

            var lifeData = _entityView.EntityData as _prototype_LifeData;
            if (lifeData != null)
            {
                if (lifeData.statusEffects.Find(s => s.type == _prototype_StatusType.Stun) != null)
                {
                    intent.Execute = async () => 
                    {
                        // 시각적 효과 (기절)
                        await _entityView.transform.DOShakePosition(0.3f, 0.1f, 10, 90f, false, true).AsyncWaitForCompletion();
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