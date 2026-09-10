using System;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_CostValue
    {
        public _prototype_CostType costType = _prototype_CostType.None;
        public float value;

        public _prototype_CostValue(_prototype_CostType costType, float value)
        {
            this.costType = costType;
            this.value = value;
        }

        public static implicit operator float(_prototype_CostValue costValue) => costValue.value;
    }
}
