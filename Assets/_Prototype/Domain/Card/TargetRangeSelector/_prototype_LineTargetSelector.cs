using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    
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

            int diffX = Mathf.Abs(targetPoint.x - selfPoint.x);
            int diffY = Mathf.Abs(targetPoint.y - selfPoint.y);

            int stepX = diffX > 0 ? (targetPoint.x > selfPoint.x ? 1 : -1) : 0;
            int stepY = diffY > 0 ? (targetPoint.y > selfPoint.y ? 1 : -1) : 0;

            int err = diffX - diffY;

            _prototype_Point curPoint = selfPoint;
            while (true)
            {
                ret.Add(curPoint);

                if (curPoint == targetPoint)
                    break;

                int err2 = err * 2;
                if (err2 > -diffY)
                {
                    err -= diffY;
                    curPoint.x += stepX;
                }
                if (err2 < diffX)
                {
                    err += diffX;
                    curPoint.y += stepY;
                }
            }

            return ret;
        }

    }

}