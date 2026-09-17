using UnityEngine;

namespace TDG0407._prototype
{

    [CreateAssetMenu(fileName = "_prototype_ObstacleDataModel", menuName = "_Prototype/ObstacleData Model")]
    public class _prototype_ObstacleDataModel : _prototype_EntityDataModel
    {
        [Tooltip("넉백 면역 여부")]
        public bool isKnockbackImmune = true;

        [Tooltip("데미지를 입어 파괴될 수 있는지 여부 (false이면 무적/파괴 불가)")]
        public bool isDestructible = true;
        
        public _prototype_ObstacleData CreateObstacleData()
        {
            return new()
            {
                ename = ename,
                health = health.Clone(),
                stamina = stamina.Clone(),
                point = point,
                size = size,
                movementType = movementType,
                heightBounds = heightBounds,
                isKnockbackImmune = isKnockbackImmune,
                isDestructible = isDestructible,
            };
        }

        public override _prototype_EntityData CreateData(_prototype_EntityData source = null)
        {
            var data = CreateObstacleData();
            PopulateComponents(data);
            return data;
        }

    }

}
