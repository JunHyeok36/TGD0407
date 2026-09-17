using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    public class _prototype_AreaEffectView : _prototype_EntityView
    {
        public _prototype_AreaEffectData areaData => EntityData as _prototype_AreaEffectData;
        public _prototype_AreaEffectDataModel areaModel;

        private List<_prototype_PointView> occupiedPointViews = new();
        private HashSet<_prototype_EntityData> _affectedEntities = new();
        private bool _isDestroyed = false;

        public override void Initialize(_prototype_EntityData data, _prototype_PointView currentPointView)
        {
            base.Initialize(data, currentPointView);

            // Register this view to all point views inside areaData.areaPoints
            if (areaData != null && areaData.areaPoints != null)
            {
                foreach (var pt in areaData.areaPoints)
                {
                    var pv = _prototype_GridManager.Instance.GetPointView(pt);
                    if (pv != null)
                    {
                        pv.PlaceEntity(this, false).Forget();
                        occupiedPointViews.Add(pv);
                        
                        // Instantiate visual piece
                        if (areaModel != null && areaModel.visualPiecePrefab != null)
                        {
                            Instantiate(areaModel.visualPiecePrefab, pv.transform.position + Vector3.up * 0.1f, Quaternion.identity, this.transform);
                        }
                    }
                }
            }

            if (areaData is _prototype_FieldData)
            {
                // 소환 즉시 현재 범위 내 대상들에게 첫 트리거 적용
                ExecuteAreaTrigger().Forget();
                _prototype_TickManager.RegisterPostTick(ProcessTick);
            }
            else if (areaData is _prototype_ExplosionData)
            {
                // 즉발형은 바로 액션을 실행하고 지연 후 파괴
                ExecuteExplosion().Forget();
            }
        }

        public async UniTask ProcessTick()
        {
            if (_isDestroyed) return;

            if (areaData is _prototype_FieldData fieldData)
            {
                if (fieldData.durationTicks <= 0)
                {
                    DestroyView();
                }
                else
                {
                    fieldData.durationTicks--;
                    // 지속 턴이 더 남아있으면 매 틱 트리거 실행
                    await ExecuteAreaTrigger();
                }
            }
        }

        public async UniTask OnEntityEntered(_prototype_LifeData target)
        {
            if (_isDestroyed || areaData == null || areaData.onTriggerActions == null || target == null) return;
            if (areaData.triggerOncePerEntity && _affectedEntities.Contains(target)) return;

            _affectedEntities.Add(target);
            var targets = new List<_prototype_EntityData> { target };
            foreach (var action in areaData.onTriggerActions)
            {
                if (action != null)
                {
                    await action.ExecuteAction(areaData.caster, targets, null);
                }
            }
        }

        private async UniTask ExecuteAreaTrigger()
        {
            if (_isDestroyed || areaData == null || areaData.onTriggerActions == null) return;

            // Collect all entities in the area (excluding self)
            HashSet<_prototype_EntityData> targets = new();
            foreach (var pv in occupiedPointViews)
            {
                if (pv == null) continue;
                foreach (var ev in pv.PlacedEntityViews)
                {
                    if (ev != null && ev != this && ev.EntityData is _prototype_LifeData lifeData)
                    {
                        if (areaData.triggerOncePerEntity && _affectedEntities.Contains(lifeData)) continue;
                        targets.Add(lifeData);
                    }
                }
            }

            if (targets.Count > 0)
            {
                if (areaData.triggerOncePerEntity)
                {
                    foreach (var t in targets) _affectedEntities.Add(t);
                }

                foreach (var action in areaData.onTriggerActions)
                {
                    if (action != null)
                    {
                        await action.ExecuteAction(areaData.caster, targets, null);
                    }
                }
            }
        }

        private async UniTask ExecuteExplosion()
        {
            await ExecuteAreaTrigger();
            await UniTask.Delay(500);
            DestroyView();
        }

        public override void SetPointImmediate(_prototype_PointView targetPointView)
        {
            // Do not change parent or move the main view, because we span multiple points.
            if (_entityData != null && targetPointView != null)
            {
                _entityData.point = targetPointView.Point;
            }
        }

        public void Die()
        {
            DestroyView();
        }

        private void DestroyView()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            if (areaData is _prototype_FieldData)
            {
                _prototype_TickManager.UnregisterPostTick(ProcessTick);
            }

            foreach (var pv in occupiedPointViews)
            {
                if (pv != null)
                {
                    pv.RemoveEntity(this);
                }
            }
            occupiedPointViews.Clear();

            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        protected override void OnDestroy()
        {
            DestroyView();
            base.OnDestroy();
        }
    }
}
