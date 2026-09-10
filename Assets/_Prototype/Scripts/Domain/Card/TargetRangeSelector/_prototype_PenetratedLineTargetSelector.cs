using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_PenetratedLineTargetSelector : _prototype_ITargetRangeSelector
    {
        public bool includeEmptyPoints = false;
        public bool IncludeEmptyPoints => includeEmptyPoints;

        [SerializeField] private int _maxRange = 3;

        public List<_prototype_Point> GetValidTargetPoints(_prototype_Point selfPoint, _prototype_Point targetPoint)
        {
            List<_prototype_Point> ret = new();
            int xDiff = targetPoint.x - selfPoint.x;
            int yDiff = targetPoint.y - selfPoint.y;
            int xStep = xDiff == 0 ? 0 : (xDiff > 0 ? 1 : -1);
            int yStep = yDiff == 0 ? 0 : (yDiff > 0 ? 1 : -1);
            int steps = Mathf.Min(_maxRange, Mathf.Max(Mathf.Abs(xDiff), Mathf.Abs(yDiff)));
            for (int i = 0; i <= steps; i++)
                ret.Add(new(selfPoint.x + i * xStep, selfPoint.y + i * yStep));
            return ret;
        }

    }

}