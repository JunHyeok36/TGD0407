using UnityEngine;

namespace TDG0407._prototype
{

    [CreateAssetMenu(fileName = "_prototype_ObstacleDataModel", menuName = "_Prototype/ObstacleData Model")]
    public class _prototype_ObstacleDataModel : _prototype_EntityDataModel
    {
        [Tooltip("넉백 면역 여부")]
        public bool isKnockbackImmune = true;
        
        public _prototype_ObstacleData CreateObstacleData()
        {
            return new()
            {
                ename = ename,
                health = health.Clone(),
                stamina = stamina.Clone(),
                point = point,
                movementType = movementType,
                isKnockbackImmune = isKnockbackImmune,
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
