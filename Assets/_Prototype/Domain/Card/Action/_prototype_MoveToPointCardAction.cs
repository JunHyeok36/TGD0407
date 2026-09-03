using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_MoveToPointCardAction : _prototype_CardAction
    {
        public bool isTeleport = false;

        public override async UniTask ExecuteCardAction(
            _prototype_EntityData source, 
            IEnumerable<_prototype_EntityData> targets,
            _prototype_ICardActionParams @params)
        {
            var sourceLife = source as _prototype_LifeData;
            if (sourceLife == null) return;

            var sourceView = _prototype_GridManager.Instance.GetPointView(source.point)?.PlacedEntityViews.Find(v => v.EntityData == source);
            if (sourceView == null) return;

            foreach (var target in targets)
            {
                var targetPointView = _prototype_GridManager.Instance.GetPointView(target.point);
                if (targetPointView == null) continue;
                
                if (isTeleport)
                {
                    if (targetPointView.CanPlaceEntity(source))
                    {
                        var currentPointView = _prototype_GridManager.Instance.GetPointView(source.point);
                        await _prototype_InteractionManager.MoveEntity(sourceView, currentPointView, targetPointView);
                    }
                }
                else
                {
                    // Pathfind to target
                    var path = _prototype_GridManager.Instance.FindPath(source.point, target.point, source, false, true);
                    if (path != null && path.Count > 0)
                    {
                        var currentPointView = _prototype_GridManager.Instance.GetPointView(source.point);
                        _prototype_PointView furthestValidPoint = null;

                        foreach (var nextStep in path)
                        {
                            if (nextStep.CanPlaceEntity(source))
                            {
                                furthestValidPoint = nextStep;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (furthestValidPoint != null)
                        {
                            await _prototype_InteractionManager.MoveEntity(sourceView, currentPointView, furthestValidPoint);
                        }
                    }
                }

                break; // Only move to the first target
            }
        }
    }
}
