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
        [SerializeField] private bool _isInfinite = false;
        [SerializeField] private bool _stopAtWall = true;
        [SerializeField] private bool _includeCasterPoint = true;

        public bool IsInfinite { get => _isInfinite; set => _isInfinite = value; }
        public bool StopAtWall { get => _stopAtWall; set => _stopAtWall = value; }
        public bool IncludeCasterPoint { get => _includeCasterPoint; set => _includeCasterPoint = value; }
        public int MaxRange { get => _maxRange; set => _maxRange = value; }

        public List<_prototype_Point> GetValidTargetPoints(_prototype_Point selfPoint, _prototype_Point targetPoint)
        {
            List<_prototype_Point> ret = new();
            int xDiff = targetPoint.x - selfPoint.x;
            int yDiff = targetPoint.y - selfPoint.y;
            int xStep = xDiff == 0 ? 0 : (xDiff > 0 ? 1 : -1);
            int yStep = yDiff == 0 ? 0 : (yDiff > 0 ? 1 : -1);

            if (xStep == 0 && yStep == 0)
            {
                if (_includeCasterPoint) ret.Add(selfPoint);
                return ret;
            }

            int startStep = _includeCasterPoint ? 0 : 1;
            int limitSteps = _isInfinite ? 999 : Mathf.Min(_maxRange, Mathf.Max(Mathf.Abs(xDiff), Mathf.Abs(yDiff)));

            for (int i = startStep; i <= limitSteps; i++)
            {
                var pt = new _prototype_Point(selfPoint.x + i * xStep, selfPoint.y + i * yStep);

                // 벽(지형 벽: Wall, OuterWall) 또는 맵 경계 체크
                if (_stopAtWall && _prototype_GridManager.Instance != null)
                {
                    var pointView = _prototype_GridManager.Instance.GetPointView(pt);
                    // 맵 밖이거나 Wall/OuterWall이면 벽 바로 앞에서 정지 (벽 타일은 미포함)
                    if (pointView == null || pointView.PointData == null ||
                        pointView.PointData.type == _prototype_PointType.Wall ||
                        pointView.PointData.type == _prototype_PointType.OuterWall)
                    {
                        break;
                    }
                }

                ret.Add(pt);
            }
            return ret;
        }
    }
}