using System.Collections.Generic;

namespace TDG0407._prototype
{

    public class _prototype_SingleTargetSelector : _prototype_ITargetRangeSelector
    {

        public List<_prototype_Point> GetValidTargetPoints(_prototype_Point selfPoint, _prototype_Point targetPoint)
        {
            return new List<_prototype_Point> { targetPoint };
        }

    }

}