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
            
            

            await ChasePlayerEntity();

            
        }

        async UniTask ChasePlayerEntity()
        {
            _prototype_Point playerPoint = _prototype_PlayerController.Instance.ControlledEntityLastPoint;
            _prototype_Point myPoint = _entityView.Point;

            List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(myPoint, playerPoint);
            if (path != null && path.Count > 0)
            {
                _prototype_PointView nextStep = path[0];
                
                if (nextStep != null && nextStep.IsEntityPlaceable)
                {
                    _prototype_PointView currentPointView = _prototype_GridManager.Instance.GetPointView(myPoint);
                    
                    if (currentPointView != null)
                        await _prototype_InteractionManager.MoveEntity(_entityView, currentPointView, nextStep);
                }
            }
        }

    }

}