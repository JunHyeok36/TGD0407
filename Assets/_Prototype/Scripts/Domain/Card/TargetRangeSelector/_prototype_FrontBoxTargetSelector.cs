using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 시전자 위치 및 조준 방향을 기준으로 전방 너비(Width) x 전방 길이(Depth) 직사각형 영역을 지정하는 타겟 범위 선택기.
    /// 예:
    /// - FrontLine(2) => width = 1, depth = 2 (전방 2칸 선형)
    /// - FrontLine(3,3) / FrontBox(3,3) => width = 3, depth = 3 (전방 3x3 직사각형)
    /// </summary>
    [Serializable]
    public class _prototype_FrontBoxTargetSelector : _prototype_ITargetRangeSelector
    {
        [SerializeField] private int _width = 1;
        [SerializeField] private int _depth = 2;
        [SerializeField] private bool _includeEmptyPoints = true;

        public int Width => _width;
        public int Depth => _depth;
        public bool IncludeEmptyPoints => _includeEmptyPoints;

        public _prototype_FrontBoxTargetSelector() { }
        public _prototype_FrontBoxTargetSelector(int width, int depth, bool includeEmptyPoints = true)
        {
            _width = Mathf.Max(1, width);
            _depth = Mathf.Max(1, depth);
            _includeEmptyPoints = includeEmptyPoints;
        }

        public List<_prototype_Point> GetValidTargetPoints(_prototype_Point selfPoint, _prototype_Point targetPoint)
        {
            List<_prototype_Point> ret = new();

            // 1. 방향 벡터 계산 (targetPoint - selfPoint)
            int diffX = targetPoint.x - selfPoint.x;
            int diffY = targetPoint.y - selfPoint.y;

            // 주 방향 결정 (절댓값이 더 큰 축 기준 4방향 노멀라이즈)
            int dirX = 0;
            int dirY = 0;

            if (Mathf.Abs(diffX) >= Mathf.Abs(diffY))
            {
                dirX = diffX != 0 ? (int)Mathf.Sign(diffX) : 1; // 기본 우측
                dirY = 0;
            }
            else
            {
                dirX = 0;
                dirY = diffY != 0 ? (int)Mathf.Sign(diffY) : 1; // 기본 상향
            }

            // 2. 수직(좌우 측면) 벡터 계산
            // 전방이 (dirX, dirY)일 때 오른쪽 수직 벡터는 (dirY, -dirX)
            int sideX = dirY;
            int sideY = -dirX;

            int halfWidth = _width / 2;

            // 3. 전방 depth(1 ~ depth), 측면 width(-halfWidth ~ halfWidth) 타일 생성
            for (int d = 1; d <= _depth; d++)
            {
                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    int pointX = selfPoint.x + (dirX * d) + (sideX * w);
                    int pointY = selfPoint.y + (dirY * d) + (sideY * w);
                    ret.Add(new _prototype_Point(pointX, pointY));
                }
            }

            return ret;
        }
    }
}
