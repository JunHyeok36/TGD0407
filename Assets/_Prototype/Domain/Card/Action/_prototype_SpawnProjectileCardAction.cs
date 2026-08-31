using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_SpawnProjectileCardAction : _prototype_CardAction
    {
        public _prototype_ProjectileDataModel projectileModel;
        public GameObject projectilePrefab;
        public _prototype_CoefficientValue[] damageCoefficients;
        public int speed;

        public override async UniTask ExecuteCardAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_ICardActionParams @params)
        {
            var sourceLife = source as _prototype_LifeData;
            _prototype_Side mySide = sourceLife != null ? sourceLife.side : _prototype_Side.None;

            if (projectileModel == null || projectilePrefab == null)
            {
                Debug.LogError("ProjectileModel or ProjectilePrefab is missing!");
                return;
            }

            foreach (var target in targets)
            {
                // Calculate direction towards target
                int dx = target.point.x - source.point.x;
                int dy = target.point.y - source.point.y;

                // Normalize direction
                int dirX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
                int dirY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

                // If diagonal, pick the dominant axis
                if (Mathf.Abs(dx) > Mathf.Abs(dy)) dirY = 0;
                else if (Mathf.Abs(dy) > Mathf.Abs(dx)) dirX = 0;

                _prototype_Point direction = new(dirX, dirY);
                if (direction == _prototype_Point.zero) continue;

                var projectileData = projectileModel.CreateProjectileData(mySide, direction, CalculateDamage(sourceLife), speed);
                projectileData.point = source.point;

                var spawnPointView = _prototype_GridManager.Instance.GetPointView(source.point);
                if (spawnPointView == null) continue;

                var go = GameObject.Instantiate(projectilePrefab);
                var view = go.GetComponent<_prototype_ProjectileView>() ?? go.AddComponent<_prototype_ProjectileView>();

                // Add to GridManager pointView

                view.InitializeProjectile(projectileData, spawnPointView);
                await spawnPointView.PlaceEntity(view);
            }

            await UniTask.Yield();
        }

        private int CalculateDamage(_prototype_LifeData sourceLife)
        {
            float calculatedDamage = 0f;
            //_prototype_DamageType damageType = _prototype_DamageType.Physical;

            if (damageCoefficients != null && damageCoefficients.Length > 0)
            {
                foreach (var coeff in damageCoefficients)
                {
                    float statValue = 1f;
                    if (sourceLife != null)
                    {
                        switch (coeff.stat)
                        {
                            case _prototype_Stat.RedPower: statValue = sourceLife.lifeStat.redPower; /*damageType = _prototype_DamageType.Physical;*/ break;
                            case _prototype_Stat.BluePower: statValue = sourceLife.lifeStat.bluePower; /*damageType = _prototype_DamageType.Magical;*/ break;
                            case _prototype_Stat.Health: statValue = sourceLife.health.Current; break;
                            case _prototype_Stat.Stamina: statValue = sourceLife.stamina.Current; break;
                            default: break; // fallback
                        }
                    }
                    calculatedDamage += statValue * coeff.coefficient;
                }
            }

            return (int)calculatedDamage;
        }
    }
}
