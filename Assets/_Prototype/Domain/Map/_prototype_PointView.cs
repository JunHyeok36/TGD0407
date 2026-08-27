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
        [SerializeField] private bool isPlaceable = true;
        [SerializeField] private bool isHoverable = true;
        
        [Header("Runtime State")]
        [SerializeReference, ReadOnly] private _prototype_PointData _pointData;
        [SerializeField, ReadOnly] private List<_prototype_EntityView> placedEntityViews;

        private Tween hoverTween;

        public _prototype_Point Point => _pointData.point;
        public List<_prototype_EntityView> PlacedEntityViews => placedEntityViews;
        public bool IsEntityPlaceable => _pointData.isPlaceable && placedEntityViews.Count == 0;
        public bool IsHoverable => _pointData.isHoverable;
        

        public void Initialize(_prototype_Point point)
        {
            this.point = point;
            this._pointData = new(point, isPlaceable, isHoverable);
            this.placedEntityViews = GetComponentsInChildren<_prototype_EntityView>(true).ToList();
            this._pointData.placedEntityDatas.AddRange(placedEntityViews.Select(entityView => entityView.EntityData));
            foreach (var entityView in placedEntityViews)
            {
                entityView.Initialize(this);
            }
        }

        public async Cysharp.Threading.Tasks.UniTask PlaceEntity(_prototype_EntityView entityView)
        {
            if (!_pointData.isPlaceable) throw new Exception($"Cannot place entity at point {point}. Point is not placeable.");
            if (placedEntityViews.Contains(entityView)) throw new Exception($"Entity {entityView.name} is already placed at point {point}.");

            placedEntityViews.Add(entityView);
            _pointData.placedEntityDatas.Add(entityView.EntityData);
            await entityView.MoveTo(this);
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
