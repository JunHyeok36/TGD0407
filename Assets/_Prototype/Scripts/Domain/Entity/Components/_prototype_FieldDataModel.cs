using UnityEngine;

namespace TDG0407._prototype
{
    [CreateAssetMenu(fileName = "FieldDataModel_", menuName = "TDG0407/Entity/_prototype_FieldDataModel")]
    public class _prototype_FieldDataModel : _prototype_AreaEffectDataModel
    {
        public int durationTicks = 1;

        public override _prototype_EntityData CreateData(_prototype_EntityData source = null)
        {
            var data = new _prototype_FieldData(
                ename,
                health,
                stamina,
                new _prototype_Point(0, 0),
                movementType,
                new System.Collections.Generic.HashSet<_prototype_Point>(), // 초기화 후 나중에 SpawnAction에서 채움
                onTriggerActions,
                null,
                side,
                durationTicks
            );
            data.triggerOncePerEntity = triggerOncePerEntity;
            data.heightBounds = heightBounds;
            PopulateComponents(data);
            return data;
        }
    }
}
