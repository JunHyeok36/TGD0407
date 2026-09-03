using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace TDG0407._prototype
{
    public class _prototype_LaserProjectileView : _prototype_EntityView
    {
        public _prototype_LaserProjectileData Data => _entityData as _prototype_LaserProjectileData;
        private bool _isDestroyed = false;
        private int _segmentIndex = 0;
        private HashSet<_prototype_EntityData> _damagedEntities;

        public void InitializeLaser(_prototype_LaserProjectileData data, _prototype_PointView pointView, int index = 0, HashSet<_prototype_EntityData> damagedEntities = null)
        {
            _segmentIndex = index;
            _entityData = data;
            _damagedEntities = damagedEntities;
            
            transform.position = pointView.transform.position + Vector3.up * 0.05f;

            var originalScale = transform.localScale;
            transform.localScale = Vector3.zero;

            var pointPv = _prototype_GridManager.Instance.GetPointView(data.point);
            if (pointPv != null)
            {
                pointPv.PlaceEntity(this).Forget();
            }

            var mr = GetComponent<MeshRenderer>();
            if (mr != null) {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0f, 0f, 0.5f);
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                mr.material = mat;
            }

            _prototype_TickManager.RegisterTick(ProcessTick);

            AnimateAppear(originalScale, index).Forget();
        }

        private async UniTaskVoid AnimateAppear(Vector3 targetScale, int index)
        {
            await UniTask.Delay(index * 50);
            if (this == null || gameObject == null || _isDestroyed) return;
            
            transform.DOScale(targetScale, 0.15f).SetEase(Ease.OutBack);
        }

        private void OnDestroy()
        {
            _prototype_TickManager.UnregisterTick(ProcessTick);
        }

        private async UniTask<_prototype_TickIntent> ProcessTick()
        {
            var intent = new _prototype_TickIntent();
            intent.TargetsPlayer = false;

            if (Data == null || _isDestroyed)
            {
                intent.Execute = async () => { await UniTask.Yield(); };
                return intent;
            }

            var ptView = _prototype_GridManager.Instance.GetPointView(Data.point);
            if (ptView != null)
            {
                foreach (var ev in ptView.PlacedEntityViews)
                {
                    if (ev.EntityData is _prototype_LifeData lifeData)
                    {
                        if (lifeData.side != _prototype_Side.None && lifeData.side != Data.side)
                        {
                            if (ev == _prototype_PlayerController.Instance.ControlledEntityView)
                            {
                                intent.TargetsPlayer = true;
                            }
                        }
                    }
                }
            }

            intent.Execute = async () =>
            {
                if (_isDestroyed || this == null || gameObject == null || Data == null) return;

                Data.durationTicks--;

                var currentPtView = _prototype_GridManager.Instance.GetPointView(Data.point);
                if (currentPtView != null)
                {
                    List<_prototype_EntityData> targetsToDamage = new();
                    foreach (var ev in currentPtView.PlacedEntityViews)
                    {
                        var entityData = ev.EntityData;
                        if (entityData is _prototype_LifeData lifeData)
                        {
                            if (lifeData.side != _prototype_Side.None && lifeData.side == Data.side) continue;
                            targetsToDamage.Add(entityData);
                        }
                        else if (entityData is _prototype_ObstacleData obsData)
                        {
                            targetsToDamage.Add(entityData);
                        }
                    }

                    foreach (var target in targetsToDamage)
                    {
                        await HitTarget(target);
                    }
                }

                if (Data.durationTicks <= 0)
                {
                    DestroyLaser();
                }
            };

            return intent;
        }

        public async UniTask HitTarget(_prototype_EntityData target)
        {
            if (_isDestroyed) return;
            if (_damagedEntities != null && _damagedEntities.Contains(target)) return;

            if (_damagedEntities != null)
                _damagedEntities.Add(target);

            var dmgContext = new _prototype_DamageContext(
                Data, target, _prototype_DamageType.Magical, Data.damage, Data.damage
            );
            await _prototype_InteractionManager.ApplyDamage(dmgContext);
        }

        public async void DestroyLaser()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            var pointView = _prototype_GridManager.Instance.GetPointView(Data.point);
            if (pointView != null) pointView.RemoveEntity(this);

            await UniTask.Delay(_segmentIndex * 50);
            if (this == null || gameObject == null) return;

            await transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).AsyncWaitForCompletion();

            if (gameObject != null) Destroy(gameObject);
        }
    }
}
