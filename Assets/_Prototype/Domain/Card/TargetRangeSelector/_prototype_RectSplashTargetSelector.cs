using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    
    public class _prototype_RectSplashTargetSelector : _prototype_ITargetRangeSelector
    {
        [SerializeField] private int _range = 1;

        public List<_prototype_Point> GetValidTargetPoints(_prototype_Point selfPoint, _prototype_Point targetPoint)
        {
            List<_prototype_Point> ret = new();
            for (int y = -_range; y <= _range; y++)
            {
                for (int x = -_range; x <= _range; x++)
                {
                    ret.Add(new(targetPoint.x + x, targetPoint.y + y));
                }
            }
            return ret;
        }

    }

}