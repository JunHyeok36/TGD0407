using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 시전자 위치를 기준으로 상/하/좌/우 및 대각선을 포함한 8방향 직선 방향으로
    /// 지정된 사거리(Range)까지의 좌표를 반환하는 시전 범위 선택기 (AroundLine).
    /// </summary>
    [Serializable]
    public class _prototype_AroundLineCastSelector : _prototype_ICastRangeSelector
    {
        [SerializeField] private int _range = 2;
        [SerializeField] private bool _includeDiagonals = true;

        public int Range => _range;
        public bool IncludeDiagonals => _includeDiagonals;

        public _prototype_AroundLineCastSelector() { }
        public _prototype_AroundLineCastSelector(int range, bool includeDiagonals = true)
        {
            _range = Mathf.Max(1, range);
            _includeDiagonals = includeDiagonals;
        }

        public List<_prototype_Point> GetValidCastPoints(_prototype_Point selfPoint)
        {
            List<_prototype_Point> ret = new();

            // 8방향 단위 벡터 정의
            var directions = new List<_prototype_Point>
            {
                new(1, 0),   // 우
                new(-1, 0),  // 좌
                new(0, 1),   // 상
                new(0, -1)   // 하
            };

            if (_includeDiagonals)
            {
                directions.Add(new(1, 1));   // 우상
                directions.Add(new(1, -1));  // 우하
                directions.Add(new(-1, 1));  // 좌상
                directions.Add(new(-1, -1)); // 좌하
            }

            for (int d = 1; d <= _range; d++)
            {
                foreach (var dir in directions)
                {
                    ret.Add(new _prototype_Point(selfPoint.x + dir.x * d, selfPoint.y + dir.y * d));
                }
            }

            return ret;
        }
    }
}
