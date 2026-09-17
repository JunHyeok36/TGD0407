using Cysharp.Threading.Tasks;
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

        public _prototype_Point Point => _pointData != null ? _pointData.point : point;
        public _prototype_PointData PointData
        {
            get
            {
                if (_pointData == null) _pointData = new(point, type, isHoverable);
                if (_pointData.placedEntityDatas == null) _pointData.placedEntityDatas = new();
                _pointData.placedEntityDatas.RemoveAll(d => d == null);
                return _pointData;
            }
        }
        public List<_prototype_EntityView> PlacedEntityViews
        {
            get
            {
                if (placedEntityViews == null) placedEntityViews = new();
                placedEntityViews.RemoveAll(v => v == null);
                return placedEntityViews;
            }
        }

        public bool IsTraversable(_prototype_MovementType movementType)
        {
            if (movementType == _prototype_MovementType.Ground)
                return PointData.type == _prototype_PointType.Normal;
            else if (movementType == _prototype_MovementType.Flying)
                return PointData.type == _prototype_PointType.Normal || PointData.type == _prototype_PointType.Abyss;
            else // Ghost
                return PointData.type != _prototype_PointType.OuterWall;
        }

        public bool CanPlaceEntity(_prototype_MovementType movementType)
        {
            return IsTraversable(movementType) && PlacedEntityViews.Count == 0;
        }

        public bool CanPlaceEntitySingleTile(_prototype_EntityData entityData)
        {
            if (entityData == null) return CanPlaceEntity(_prototype_MovementType.Ground);
            if (!IsTraversable(entityData.movementType)) return false;

            if (entityData is _prototype_ProjectileData || entityData is _prototype_AreaEffectData)
            {
                return true;
            }

            bool isPlacingTrap = entityData.TryGetComponent<_prototype_TrapComponentData>(out var placingTrap);

            foreach (var entityView in PlacedEntityViews)
            {
                var otherData = entityView.EntityData;
                if (otherData == null || otherData == entityData) continue;

                // 투사체나 장판은 다른 엔티티의 배치를 차단하지 않음
                if (otherData is _prototype_ProjectileData || otherData is _prototype_AreaEffectData) continue;

                // 고도가 겹치지 않는다면 물리적으로 충돌하지 않으므로 동일 타일에 공존 가능
                if (!entityData.heightBounds.Overlaps(otherData.heightBounds))
                {
                    continue;
                }

                // 고도가 겹치는 경우의 충돌 검사
                bool isOtherTrap = otherData.TryGetComponent<_prototype_TrapComponentData>(out var otherTrap);

                if (isPlacingTrap)
                {
                    // 같은 타일/고도에 트랩 중복 설치 불가
                    if (isOtherTrap) return false;
                    // 솔리드 트랩인 경우 다른 솔리드 엔티티와 충돌
                    if (placingTrap.isSolidObstacle && otherData.IsSolid) return false;
                }
                else
                {
                    // 일반 장애물/엔티티 설치 시 이미 트랩이 있는 경우
                    if (isOtherTrap)
                    {
                        // 캐릭터는 비솔리드 트랩을 밟고 설 수 있음 (공존 허용)
                        if (entityData is _prototype_LifeData && !otherTrap.isSolidObstacle)
                        {
                            continue;
                        }
                        // 일반 장애물(바위 등)은 트랩 위에 겹쳐 놓을 수 없음
                        if (entityData is _prototype_ObstacleData)
                        {
                            return false;
                        }
                    }

                    // 둘 다 Solid인 경우 충돌 (배치 불가)
                    if (entityData.IsSolid && otherData.IsSolid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool CanPlaceEntity(_prototype_EntityData entityData, bool checkFootprint = true)
        {
            if (entityData != null && checkFootprint && (entityData.size.x > 1 || entityData.size.y > 1) && _prototype_GridManager.Instance != null)
            {
                return _prototype_GridManager.Instance.CanPlaceEntityFootprint(entityData, this.Point);
            }
            return CanPlaceEntitySingleTile(entityData);
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
                entityView.Initialize(null, this); // test
            }
        }

        public async UniTask PlaceEntity(_prototype_EntityView entityView, bool animate = true, bool rotateTowardsDestination = true)
        {
            if (!IsTraversable(entityView.EntityData.movementType)) throw new Exception($"Cannot place entity at point {point}. Point is blocked by terrain.");
            if (PlacedEntityViews.Contains(entityView)) throw new Exception($"Entity {entityView.name} is already placed at point {point}.");

            PlacedEntityViews.Add(entityView);
            PointData.placedEntityDatas.Add(entityView.EntityData);

            if (_prototype_GridManager.Instance != null && entityView.EntityData != null)
            {
                var fp = _prototype_GridManager.Instance.GetFootprint(this.Point, entityView.EntityData.size);
                if (fp != null)
                {
                    foreach (var pv in fp)
                    {
                        if (pv == this) continue;
                        if (!pv.PlacedEntityViews.Contains(entityView))
                        {
                            pv.PlacedEntityViews.Add(entityView);
                            pv.PointData.placedEntityDatas.Add(entityView.EntityData);
                        }
                    }
                }
            }

            if (animate)
            {
                await entityView.MoveTo(this, rotateTowardsDestination);
            }
            else
            {
                entityView.SetPointImmediate(this);
            }
        }

        public void RemoveEntity(_prototype_EntityView entityView)
        {
            if (!PlacedEntityViews.Contains(entityView)) throw new Exception($"Entity {entityView.name} is not placed at point {point}.");

            PlacedEntityViews.Remove(entityView);
            if (entityView.EntityData != null)
                PointData.placedEntityDatas.Remove(entityView.EntityData);

            if (_prototype_GridManager.Instance != null && entityView.EntityData != null)
            {
                var fp = _prototype_GridManager.Instance.GetFootprint(this.Point, entityView.EntityData.size);
                if (fp != null)
                {
                    foreach (var pv in fp)
                    {
                        if (pv == this) continue;
                        if (pv.PlacedEntityViews.Contains(entityView))
                        {
                            pv.PlacedEntityViews.Remove(entityView);
                            pv.PointData.placedEntityDatas.Remove(entityView.EntityData);
                        }
                    }
                }
            }
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
