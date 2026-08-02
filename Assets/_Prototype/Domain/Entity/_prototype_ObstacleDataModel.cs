using UnityEngine;

namespace TDG0407._prototype
{

    [CreateAssetMenu(fileName = "_prototype_ObstacleDataModel", menuName = "_Prototype/ObstacleData Model")]
    public class _prototype_ObstacleDataModel : _prototype_EntityDataModel
    {
        
        public _prototype_ObstacleData CreateObstacleData()
        {
            return new()
            {
                ename = ename,
                health = health.Clone(),
                stamina = stamina.Clone(),
                point = point
            };
        }

    }

}
