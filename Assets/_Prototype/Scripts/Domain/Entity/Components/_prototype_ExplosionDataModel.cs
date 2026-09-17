using UnityEngine;

namespace TDG0407._prototype
{
    [CreateAssetMenu(fileName = "ExplosionDataModel_", menuName = "TDG0407/Entity/_prototype_ExplosionDataModel")]
    public class _prototype_ExplosionDataModel : _prototype_AreaEffectDataModel
    {
        public override _prototype_EntityData CreateData(_prototype_EntityData source = null)
        {
            var data = new _prototype_ExplosionData(
                ename,
                health,
                stamina,
                new _prototype_Point(0, 0),
                movementType,
                new System.Collections.Generic.HashSet<_prototype_Point>(), // 초기화 후 나중에 SpawnAction에서 채움
                onTriggerActions,
                null,
                side
            );
            data.triggerOncePerEntity = triggerOncePerEntity;
            data.heightBounds = heightBounds;
            PopulateComponents(data);
            return data;
        }
    }
}
