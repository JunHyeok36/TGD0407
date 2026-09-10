using System.Collections.Generic;

namespace TDG0407._prototype
{

    public interface _prototype_ITargetRangeSelector
    {
        bool IncludeEmptyPoints { get; }
        List<_prototype_Point> GetValidTargetPoints(_prototype_Point selfPoint, _prototype_Point targetPoint);
    }

}