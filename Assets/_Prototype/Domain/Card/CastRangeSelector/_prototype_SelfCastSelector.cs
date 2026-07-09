using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_SelfCastSelector : _prototype_ICastRangeSelector
    {

        public List<_prototype_Point> GetValidCastPoints(_prototype_Point selfPoint)
        {
            return new List<_prototype_Point> { selfPoint };
        }

    }

}