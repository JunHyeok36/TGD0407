using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    
    [Serializable]
    public class _prototype_LineTargetSelector : _prototype_ITargetRangeSelector
    {
        public bool includeEmptyPoints = false;
        public bool IncludeEmptyPoints => includeEmptyPoints;


        public List<_prototype_Point> GetValidTargetPoints(_prototype_Point selfPoint, _prototype_Point targetPoint)
        {
            List<_prototype_Point> ret = new();
            if (selfPoint == targetPoint)
            {
                ret.Add(selfPoint);
                return ret;
            }

            int diffX = targetPoint.x - selfPoint.x;
            int diffY = targetPoint.y - selfPoint.y;

            int stepX = diffX == 0 ? 0 : (diffX > 0 ? 1 : -1);
            int stepY = diffY == 0 ? 0 : (diffY > 0 ? 1 : -1);

            int steps = Mathf.Max(Mathf.Abs(diffX), Mathf.Abs(diffY));

            for (int i = 0; i <= steps; i++)
            {
                ret.Add(new _prototype_Point(selfPoint.x + i * stepX, selfPoint.y + i * stepY));
            }

            return ret;
        }

    }

}