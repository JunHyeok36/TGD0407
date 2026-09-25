using System;

namespace TDG0407._prototype
{

    [Serializable]
    public sealed class _prototype_CoefficientValue
    {

        public _prototype_StatSource source = _prototype_StatSource.Caster;
        public _prototype_Stat stat;
        public float coefficient;

        public _prototype_CoefficientValue() { }
        public _prototype_CoefficientValue(_prototype_StatSource source, _prototype_Stat stat, float coefficient)
        {
            this.source = source;
            this.stat = stat;
            this.coefficient = coefficient;
        }
    }
}
