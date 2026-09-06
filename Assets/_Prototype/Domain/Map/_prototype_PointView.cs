using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace TDG0407._prototype
{

    public class _prototype_PointView : MonoBehaviour
    {
        [Header("Dev References")]
        [SerializeField] private _prototype_Point point;
        [SerializeField] private _prototype_PointType type = _prototype_PointType.Normal;
        [SerializeField] private bool isHoverable = true;
        
        [Header("Runtime State")]
        [SerializeReference, ReadOnly] private _prototype_PointData _pointData;
        [SerializeField, ReadOnly] private List<_prototype_EntityView> placedEntityViews;

        private Tween hoverTween;

        public _prototype_Point Point => _pointData.point;
        public _prototype_PointData PointData => _pointData;
        public List<_prototype_EntityView> PlacedEntityViews => placedEntityViews;
        
        public bool IsTraversable(_prototype_MovementType movementType)
        {
            if (movementType == _prototype_MovementType.Ground)
                return _pointData.type == _prototype_PointType.Normal;
            else if (movementType == _prototype_MovementType.Flying)
                return _pointData.type == _prototype_PointType.Normal || _pointData.type == _prototype_PointType.Abyss;
            else // Ghost
                return _pointData.type != _prototype_PointType.OuterWall;
        }

        public bool CanPlaceEntity(_prototype_MovementType movementType)
        {
            return IsTraversable(movementType) && placedEntityViews.Count == 0;
        }

        public bool CanPlaceEntity(_prototype_EntityData entityData)
        {
            if (entityData == null) return CanPlaceEntity(_prototype_MovementType.Ground);
            if (!IsTraversable(entityData.movementType)) return false;

            if (entityData is _prototype_LifeData)
            {
                foreach (var entityView in placedEntityViews)
                {
                    if (entityView.EntityData is _prototype_LifeData || entityView.EntityData is _prototype_ObstacleData)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (entityData is _prototype_ObstacleData)
            {
                foreach (var entityView in placedEntityViews)
                {
                    if (entityView.EntityData is _prototype_ObstacleData || entityView.EntityData is _prototype_LifeData)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (entityData is _prototype_ProjectileData || entityData is _prototype_LaserProjectileData)
            {
                return true;
            }
            
            return placedEntityViews.Count == 0;
        }

        public bool IsHoverable => _pointData.isHoverable;
        

        public void Initialize(_prototype_Point point)
        {
            this.point = point;
            this._pointData = new(point, type, isHoverable);
            this.placedEntityViews = GetComponentsInChildren<_prototype_EntityView>(true).ToList();
            this._pointData.placedEntityDatas.AddRange(placedEntityViews.Select(entityView => entityView.EntityData));
            foreach (var entityView in placedEntityViews)
            {
                entityView.Initialize(this);
            }
        }

        public async Cysharp.Threading.Tasks.UniTask PlaceEntity(_prototype_EntityView entityView, bool animate = true)
        {
            if (!IsTraversable(entityView.EntityData.movementType)) throw new Exception($"Cannot place entity at point {point}. Point is blocked by terrain.");
            if (placedEntityViews.Contains(entityView)) throw new Exception($"Entity {entityView.name} is already placed at point {point}.");

            placedEntityViews.Add(entityView);
            _pointData.placedEntityDatas.Add(entityView.EntityData);
            if (animate)
            {
                await entityView.MoveTo(this);
            }
            else
            {
                entityView.SetPointImmediate(this);
            }
        }

        public void RemoveEntity(_prototype_EntityView entityView)
        {
            if (!placedEntityViews.Contains(entityView)) throw new Exception($"Entity {entityView.name} is not placed at point {point}.");

            placedEntityViews.Remove(entityView);
            _pointData.placedEntityDatas.Remove(entityView.EntityData);
        }

        public void Hovering(bool isHovering)
        {
            if (hoverTween != null && hoverTween.IsActive())
                hoverTween.Kill();

            if (isHovering)
                hoverTween = transform.DOLocalMoveY(0.1f, 0.1f).SetEase(Ease.OutBack);
            else
                hoverTween = transform.DOLocalMoveY(0f, 0.1f).SetEase(Ease.OutBack);

        }
    }

}
