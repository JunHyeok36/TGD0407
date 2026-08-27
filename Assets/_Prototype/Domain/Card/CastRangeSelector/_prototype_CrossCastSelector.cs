using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_CrossCastSelector : _prototype_ICastRangeSelector
    {
        [SerializeField] private int _range = 1;

        public List<_prototype_Point> GetValidCastPoints(_prototype_Point selfPoint)
        {
            List<_prototype_Point> ret = new();
            for (int i = 1; i <= _range; i++)
            {
                ret.Add(new(selfPoint.x + i, selfPoint.y));
                ret.Add(new(selfPoint.x - i, selfPoint.y));
                ret.Add(new(selfPoint.x, selfPoint.y + i));
                ret.Add(new(selfPoint.x, selfPoint.y - i));
            }
            return ret;
        }
    }
}
