using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_AroundRectCastSelector : _prototype_ICastRangeSelector
    {

        [SerializeField] private int _range = 1;

        public List<_prototype_Point> GetValidCastPoints(_prototype_Point selfPoint)
        {
            List<_prototype_Point> ret = new();
            for (int x = -_range; x <= _range; x++)
            {
                for (int y = -_range; y <= _range; y++)
                {
                    if (x == 0 && y == 0) continue;
                    ret.Add(new(selfPoint.x + x, selfPoint.y + y));
                }
            }
            return ret;
        }

    }
}
