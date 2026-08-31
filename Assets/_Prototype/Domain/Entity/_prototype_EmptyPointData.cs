using System;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_EmptyPointData : _prototype_EntityData
    {
        public _prototype_EmptyPointData(_prototype_Point point) 
            : base("EmptyPoint", new _prototype_BoundedValue<int>(0, 1), new _prototype_BoundedValue<int>(0, 1), point, _prototype_MovementType.Ground)
        {
        }
    }
}
