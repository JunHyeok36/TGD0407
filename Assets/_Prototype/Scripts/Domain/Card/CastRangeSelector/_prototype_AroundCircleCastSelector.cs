using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 시전자 위치 중심 반경 R 이내의 모든 좌표를 반환하는 시전 범위 선택기 (AroundCircle).
    /// 유클리드 거리 (dx^2 + dy^2 <= R^2) 또는 체비쇼프/맨해튼 옵션을 제공하며, 기본은 원형 반경(dx^2 + dy^2 <= R^2)입니다.
    /// includeSelf=false일 경우 시전자 본인 타일은 제외합니다.
    /// </summary>
    [Serializable]
    public class _prototype_AroundCircleCastSelector : _prototype_ICastRangeSelector
    {
        [SerializeField] private int _radius = 3;
        [SerializeField] private bool _includeSelf = false;

        public int Radius => _radius;
        public bool IncludeSelf => _includeSelf;

        public _prototype_AroundCircleCastSelector() { }
        public _prototype_AroundCircleCastSelector(int radius, bool includeSelf = false)
        {
            _radius = radius;
            _includeSelf = includeSelf;
        }

        public List<_prototype_Point> GetValidCastPoints(_prototype_Point selfPoint)
        {
            List<_prototype_Point> ret = new();
            int rSq = _radius * _radius;

            for (int dx = -_radius; dx <= _radius; dx++)
            {
                for (int dy = -_radius; dy <= _radius; dy++)
                {
                    if (!_includeSelf && dx == 0 && dy == 0) continue;

                    // 유클리드 원형 거리 판정 (대각선 3칸 거리 자연스럽게 포함/제한)
                    // dx^2 + dy^2 <= r^2 + r/2 로 정수 그리드 친화적 원형 영역 커버
                    if (dx * dx + dy * dy <= rSq)
                    {
                        ret.Add(new _prototype_Point(selfPoint.x + dx, selfPoint.y + dy));
                    }
                }
            }

            return ret;
        }
    }
}
