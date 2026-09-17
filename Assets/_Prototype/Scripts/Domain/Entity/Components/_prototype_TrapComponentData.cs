using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_TrapComponentData : _prototype_EntityComponentData, _prototype_IInteractable
    {
        public _prototype_TrapType trapType = _prototype_TrapType.Spike;

        public int triggerDamage = 10;
        public bool isDisarmed = false;
        public bool isSolidObstacle = false;
        public bool triggerOnStep = true;
        public bool triggerOnDamage = false;
        public bool stopsMovement = false;
        public bool isConsumedOnTrigger = true;

        public _prototype_StatusType statusEffectToApply = _prototype_StatusType.None;
        public int statusEffectDuration = 1;

        public int explosionRadius = 1;
        public int knockbackDistance = 1;
        public int hazardDurationTicks = 2;

        [NonSerialized]
        private _prototype_EntityView _owner;
        [NonSerialized]
        private bool _isTriggering = false;

        public _prototype_TrapComponentData() { }

        public _prototype_TrapComponentData(_prototype_TrapComponentData other)
        {
            if (other == null) return;
            this.trapType = other.trapType;
            this.triggerDamage = other.triggerDamage;
            this.isDisarmed = other.isDisarmed;
            this.isSolidObstacle = other.isSolidObstacle;
            this.triggerOnStep = other.triggerOnStep;
            this.triggerOnDamage = other.triggerOnDamage;
            this.stopsMovement = other.stopsMovement;
            this.isConsumedOnTrigger = other.isConsumedOnTrigger;
            this.statusEffectToApply = other.statusEffectToApply;
            this.statusEffectDuration = other.statusEffectDuration;
            this.explosionRadius = other.explosionRadius;
            this.knockbackDistance = other.knockbackDistance;
            this.hazardDurationTicks = other.hazardDurationTicks;
        }

        public override _prototype_EntityComponentData Clone()
        {
            return new _prototype_TrapComponentData(this);
        }

        public override void Initialize(_prototype_EntityView owner)
        {
            base.Initialize(owner);
            _owner = owner;
            if (_owner.EntityData != null)
            {
                _owner.EntityData.health.OnValueChanged += OnHealthChanged;
            }
        }

        public override void OnDestroy(_prototype_EntityView owner)
        {
            base.OnDestroy(owner);
            if (_owner != null && _owner.EntityData != null)
            {
                _owner.EntityData.health.OnValueChanged -= OnHealthChanged;
            }
        }

        private void OnHealthChanged()
        {
            if (_owner != null && _owner.EntityData != null && _owner.EntityData.health.Current <= 0 && !isDisarmed && !_isTriggering)
            {
                if (triggerOnDamage)
                {
                    TriggerTrap(null).Forget();
                }
                else
                {
                    Disarm().Forget();
                }
            }
        }

        public async UniTask Disarm()
        {
            if (isDisarmed) return;
            isDisarmed = true;

            if (_owner != null && _owner.transform != null && Application.isPlaying)
            {
                await _owner.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).AsyncWaitForCompletion();
            }

            if (_owner != null)
            {
                var pointView = _prototype_GridManager.Instance?.GetPointView(_owner.EntityData.point);
                if (pointView != null) pointView.RemoveEntity(_owner);
                if (_owner.gameObject != null) UnityEngine.Object.Destroy(_owner.gameObject);
            }
        }

        public async UniTask Interact(_prototype_EntityData caster, string interactionKey)
        {
            if (string.Equals(interactionKey, "Disarm", StringComparison.OrdinalIgnoreCase))
            {
                await Disarm();
            }
            else if (string.Equals(interactionKey, "Detonate", StringComparison.OrdinalIgnoreCase))
            {
                await TriggerTrap(caster);
            }
            else
            {
                Debug.LogWarning($"[TrapComponentData] Unknown interactionKey: {interactionKey}");
            }
        }

        public async UniTask TriggerTrap(_prototype_EntityData triggeringEntity)
        {
            if (isDisarmed || _isTriggering || _owner == null) return;
            _isTriggering = true;
            isDisarmed = true;

            switch (trapType)
            {
                case _prototype_TrapType.Spike:
                    await HandleSpikeTrigger(triggeringEntity);
                    break;
                case _prototype_TrapType.BearTrap:
                    await HandleBearTrapTrigger(triggeringEntity);
                    break;
                case _prototype_TrapType.ExplosiveBarrel:
                    await HandleExplosiveTrigger(triggeringEntity);
                    break;
                case _prototype_TrapType.HazardField:
                    await HandleHazardFieldTrigger(triggeringEntity);
                    break;
            }

            if (isConsumedOnTrigger)
            {
                var pointView = _prototype_GridManager.Instance?.GetPointView(_owner.EntityData.point);
                if (pointView != null) pointView.RemoveEntity(_owner);

                if (_owner.gameObject != null) UnityEngine.Object.Destroy(_owner.gameObject);
            }
            else
            {
                _isTriggering = false;
            }
        }

        private async UniTask HandleSpikeTrigger(_prototype_EntityData triggeringEntity)
        {
            if (Application.isPlaying && _owner != null)
            {
                var punchTask = _owner.transform.DOPunchPosition(Vector3.up * 0.35f, 0.25f, 10, 1).AsyncWaitForCompletion();
                var scaleTask = _owner.transform.DOPunchScale(new Vector3(0.2f, 0.4f, 0.2f), 0.25f, 5, 1).AsyncWaitForCompletion();
                await UniTask.WhenAll(punchTask.AsUniTask(), scaleTask.AsUniTask());
            }

            if (triggeringEntity != null)
            {
                var ctx = new _prototype_DamageContext(_owner.EntityData,
                    triggeringEntity,
                    _prototype_DamageType.Physical,
                    triggerDamage,
                    triggerDamage,
                    false
                );
                await _prototype_InteractionManager.ApplyDamage(ctx);
            }
        }

        private async UniTask HandleBearTrapTrigger(_prototype_EntityData triggeringEntity)
        {
            if (Application.isPlaying && _owner != null)
            {
                await _owner.transform.DOPunchScale(new Vector3(-0.35f, 0.3f, -0.35f), 0.25f, 8, 1).AsyncWaitForCompletion();
            }

            if (triggeringEntity != null)
            {
                var ctx = new _prototype_DamageContext(_owner.EntityData,
                    triggeringEntity,
                    _prototype_DamageType.Physical,
                    triggerDamage,
                    triggerDamage,
                    false
                );
                await _prototype_InteractionManager.ApplyDamage(ctx);

                if (statusEffectToApply != _prototype_StatusType.None && triggeringEntity is _prototype_LifeData life)
                {
                    life.ApplyStatusEffect(new _prototype_StatusEffect(statusEffectToApply, statusEffectDuration, _owner.EntityData, life, 10f));
                }
            }
        }

        private async UniTask HandleExplosiveTrigger(_prototype_EntityData triggeringEntity)
        {
            if (Application.isPlaying && _owner != null)
            {
                await _owner.transform.DOShakePosition(0.18f, 0.15f, 20).AsyncWaitForCompletion();
            }

            _prototype_Point center = _owner.EntityData.point;
            int r = explosionRadius;

            List<_prototype_Point> blastPoints = new();
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    blastPoints.Add(center + new _prototype_Point(dx, dy));
                }
            }

            List<UniTask> damageTasks = new();
            List<_prototype_TrapComponentData> chainTraps = new();

            foreach (var pt in blastPoints)
            {
                var pv = _prototype_GridManager.Instance?.GetPointView(pt);
                if (pv == null) continue;

                var views = pv.PlacedEntityViews.ToList();
                foreach (var ev in views)
                {
                    if (ev == null || ev == _owner) continue;

                    if (ev.EntityData.TryGetComponent<_prototype_TrapComponentData>(out var otherTrap) && otherTrap.triggerOnDamage && !otherTrap.isDisarmed)
                    {
                        if (!chainTraps.Contains(otherTrap)) chainTraps.Add(otherTrap);
                    }
                    else if (ev.EntityData is _prototype_LifeData life && life.health.Current > 0)
                    {
                        var ctx = new _prototype_DamageContext(_owner.EntityData,
                            life,
                            _prototype_DamageType.Physical,
                            triggerDamage,
                            triggerDamage,
                            false
                        );
                        damageTasks.Add(_prototype_InteractionManager.ApplyDamage(ctx));

                        if (knockbackDistance > 0)
                        {
                            int kx = Mathf.Clamp(life.point.x - center.x, -1, 1);
                            int ky = Mathf.Clamp(life.point.y - center.y, -1, 1);
                            if (kx != 0 || ky != 0)
                            {
                                var kbAction = new _prototype_KnockbackEntityAction
                                {
                                    directionMode = _prototype_KnockbackDirectionType.FixedDirection,
                                    fixedDirection = new _prototype_Point(kx, ky),
                                    distance = knockbackDistance
                                };
                                damageTasks.Add(kbAction.ExecuteAction(_owner.EntityData, new[] { life }, null));
                            }
                        }
                    }
                    else if (ev.EntityData is _prototype_ObstacleData obs && obs.health.Current > 0)
                    {
                        var ctx = new _prototype_DamageContext(_owner.EntityData,
                            obs,
                            _prototype_DamageType.Physical,
                            triggerDamage,
                            triggerDamage,
                            false
                        );
                        damageTasks.Add(_prototype_InteractionManager.ApplyDamage(ctx));
                    }
                }
            }

            if (damageTasks.Count > 0)
            {
                await UniTask.WhenAll(damageTasks);
            }

            if (chainTraps.Count > 0)
            {
                await UniTask.WhenAll(chainTraps.Select(ct => ct.TriggerTrap(this._owner.EntityData)));
            }
        }

        private async UniTask HandleHazardFieldTrigger(_prototype_EntityData triggeringEntity)
        {
            if (Application.isPlaying && _owner != null)
            {
                await _owner.transform.DOScale(Vector3.zero, 0.2f).AsyncWaitForCompletion();
            }

            _prototype_Point center = _owner.EntityData.point;
            List<_prototype_Point> points = new()
            {
                center,
                center + new _prototype_Point(0, 1),
                center + new _prototype_Point(0, -1),
                center + new _prototype_Point(1, 0),
                center + new _prototype_Point(-1, 0)
            };

            _prototype_HazardFieldManager.Instance.SpawnHazardField(
                points,
                hazardDurationTicks,
                triggerDamage,
                _prototype_StatusType.Burning,
                _owner
            );
        }
    }
}
