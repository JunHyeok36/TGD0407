using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_CrossSplashTargetSelector : _prototype_ITargetRangeSelector
    {
        [SerializeField] private int _range = 1;

        public List<_prototype_Point> GetValidTargetPoints(_prototype_Point selfPoint, _prototype_Point targetPoint)
        {
            List<_prototype_Point> ret = new();
            ret.Add(targetPoint);
            for (int i = 1; i <= _range; i++)
            {
                ret.Add(new(targetPoint.x + i, targetPoint.y));
                ret.Add(new(targetPoint.x - i, targetPoint.y));
                ret.Add(new(targetPoint.x, targetPoint.y + i));
                ret.Add(new(targetPoint.x, targetPoint.y - i));
            }
            return ret;
        }
    }
}
