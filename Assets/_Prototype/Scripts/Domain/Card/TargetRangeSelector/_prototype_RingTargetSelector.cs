using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_RingTargetSelector : _prototype_ITargetRangeSelector
    {
        public bool includeEmptyPoints = false;
        public bool IncludeEmptyPoints => includeEmptyPoints;

        [SerializeField] private int _radius = 1;

        public List<_prototype_Point> GetValidTargetPoints(_prototype_Point selfPoint, _prototype_Point targetPoint)
        {
            List<_prototype_Point> ret = new();
            // Simple ring based on Chebyshev distance (square ring)
            for (int y = -_radius; y <= _radius; y++)
            {
                for (int x = -_radius; x <= _radius; x++)
                {
                    if (Mathf.Abs(x) == _radius || Mathf.Abs(y) == _radius)
                    {
                        ret.Add(new(targetPoint.x + x, targetPoint.y + y));
                    }
                }
            }
            return ret;
        }
    }
}
