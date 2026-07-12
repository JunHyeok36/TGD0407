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
            _prototype_Point playerPoint = _prototype_PlayerController.Instance.ControlledEntityView.Point;
            _prototype_Point myPoint = _entityView.Point;

            // 4. 길찾기 수행
            List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(myPoint, playerPoint);
            if (path != null && path.Count > 0)
            {
                _prototype_PointView nextStep = path[0];
                
                // 다음 칸 데이터와 이동 가능한 상태인지 검사
                if (nextStep != null && nextStep.IsEntityPlaceable)
                {
                    _prototype_PointView currentPointView = _prototype_GridManager.Instance.GetPointView(myPoint);
                    
                    // 현재 서 있는 발판 뷰도 안전한지 최종 검사 후 이동
                    if (currentPointView != null)
                    {
                        await _prototype_InteractionManager.MoveEntity(_entityView, currentPointView, nextStep);
                    }
                }
            }
        }

    }

}