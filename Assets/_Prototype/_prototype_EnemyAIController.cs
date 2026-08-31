using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        private async UniTask DetermineNextAction()
        {
            if (_entityView == null || _entityView.EntityData == null) return;
            if (_entityView.EntityData.health.Current <= 0) return; // 죽었으면 행동 안 함

            var lifeData = _entityView.EntityData as _prototype_LifeData;
            if (lifeData != null && lifeData.aiLogic != null)
            {
                await lifeData.aiLogic.ExecuteAction(_entityView);
            }
        }
    }
}