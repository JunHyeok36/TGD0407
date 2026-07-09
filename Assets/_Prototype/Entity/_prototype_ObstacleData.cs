using System;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_ObstacleData : _prototype_EntityData
    {

        public _prototype_ObstacleData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point position)
            : base(name, health, stamina, position) { }

        public _prototype_ObstacleData(_prototype_ObstacleData other)
            : base(other) { }

    }

}
