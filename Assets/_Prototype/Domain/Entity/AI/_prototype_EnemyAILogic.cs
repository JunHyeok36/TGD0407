using System;
using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_EnemyAILogic
    {
        [NonSerialized] public _prototype_CardData plannedCard;
        [NonSerialized] public _prototype_Point plannedTarget;
        [NonSerialized] public bool hasPlannedIntent;

        public virtual void EvaluateIntent(_prototype_EntityView entityView) { }
        public virtual UniTask ExecuteAction(_prototype_EntityView entityView) { return UniTask.CompletedTask; }
        public abstract UniTask<_prototype_TickIntent> PlanAction(_prototype_EntityView entityView);
        public abstract _prototype_EnemyAILogic Clone();

        protected async UniTask<bool> HandleStatusEffectsOverride(_prototype_EntityView entityView)
        {
            var lifeData = entityView.EntityData as _prototype_LifeData;
            if (lifeData == null) return false;

            var fearEffect = lifeData.GetStatusEffect(_prototype_StatusType.Fear);
            if (fearEffect != null && fearEffect.sourceEntity != null)
            {
                if (!lifeData.HasStatusEffect(_prototype_StatusType.Freeze))
                {
                    // Move away
                    _prototype_Point p = entityView.Point;
                    var pts = new _prototype_Point[] {
                        new _prototype_Point(p.x + 1, p.y),
                        new _prototype_Point(p.x - 1, p.y),
                        new _prototype_Point(p.x, p.y + 1),
                        new _prototype_Point(p.x, p.y - 1)
                    };
                    _prototype_Point bestPt = entityView.Point;
                    int maxDist = UnityEngine.Mathf.Abs(bestPt.x - fearEffect.sourceEntity.point.x) + UnityEngine.Mathf.Abs(bestPt.y - fearEffect.sourceEntity.point.y);
                    foreach (var pt in pts)
                    {
                        var ptView = _prototype_GridManager.Instance.GetPointView(pt);
                        if (ptView != null && ptView.CanPlaceEntity(lifeData))
                        {
                            int dist = UnityEngine.Mathf.Abs(pt.x - fearEffect.sourceEntity.point.x) + UnityEngine.Mathf.Abs(pt.y - fearEffect.sourceEntity.point.y);
                            if (dist > maxDist)
                            {
                                maxDist = dist;
                                bestPt = pt;
                            }
                        }
                    }
                    if (bestPt != entityView.Point)
                    {
                        var nextStep = _prototype_GridManager.Instance.GetPointView(bestPt);
                        await _prototype_InteractionManager.MoveEntity(entityView, _prototype_GridManager.Instance.GetPointView(entityView.Point), nextStep);
                    }
                }
                return true; // Overridden by fear
            }
            return false;
        }
    }
}
