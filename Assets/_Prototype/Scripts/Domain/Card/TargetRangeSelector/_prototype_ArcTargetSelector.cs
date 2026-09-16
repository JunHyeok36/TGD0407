using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_ArcTargetSelector : _prototype_ITargetRangeSelector
    {
        [Header("Arc Settings")]
        [Tooltip("호의 내부 길이 (시전자로부터 영역에서 제외되는 내부 거리)")]
        [SerializeField] private int innerRadius = 0;

        [Tooltip("호의 외부 길이 (시전자로부터 영역에 포함되는 최대 외부 거리)")]
        [SerializeField] private int outerRadius = 1;

        [Tooltip("호의 너비 (타일 수, 홀수 권장: 3, 5 등)")]
        [SerializeField] private int arcWidth = 3;

        [Tooltip("대각선 방향 조준 허용 여부")]
        [SerializeField] private bool allowDiagonals = true;

        [Tooltip("다음 줄(바깥 반경)로 갈 때마다 양옆으로 1칸씩(총 2칸) 너비 확장 여부")]
        [SerializeField] private bool expandWidthPerLayer = true;

        [Tooltip("빈 타일 포함 여부 (하이라이터 표시 및 빈 타일 타겟팅 지원)")]
        public bool includeEmptyPoints = true;

        // 이전 radius 직렬화 필드 보존 (하위 호환용)
        [SerializeField, HideInInspector] private int radius = 1;

        public bool IncludeEmptyPoints => includeEmptyPoints;
        public int InnerRadius { get => innerRadius; set => innerRadius = value; }
        public int OuterRadius { get => outerRadius; set => outerRadius = value; }
        public int ArcWidth { get => arcWidth; set => arcWidth = value; }
        public bool AllowDiagonals { get => allowDiagonals; set => allowDiagonals = value; }
        public bool ExpandWidthPerLayer { get => expandWidthPerLayer; set => expandWidthPerLayer = value; }

        public List<_prototype_Point> GetValidTargetPoints(_prototype_Point selfPoint, _prototype_Point targetPoint)
        {
            HashSet<_prototype_Point> resultSet = new();

            int dx = targetPoint.x - selfPoint.x;
            int dy = targetPoint.y - selfPoint.y;

            if (dx == 0 && dy == 0)
            {
                // 기본값: 북쪽
                dy = 1;
            }

            // 각도 계산 (-180 ~ 180도)
            float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

            _prototype_Point dir;
            bool isDiagonal = false;

            if (allowDiagonals)
            {
                // 8방향 스냅 (각 45도 간격, 중심 +- 22.5도)
                if (angle >= -22.5f && angle < 22.5f)
                    dir = new(1, 0); // East
                else if (angle >= 22.5f && angle < 67.5f)
                {
                    dir = new(1, 1); // North-East
                    isDiagonal = true;
                }
                else if (angle >= 67.5f && angle < 112.5f)
                    dir = new(0, 1); // North
                else if (angle >= 112.5f && angle < 157.5f)
                {
                    dir = new(-1, 1); // North-West
                    isDiagonal = true;
                }
                else if (angle >= -67.5f && angle < -22.5f)
                {
                    dir = new(1, -1); // South-East
                    isDiagonal = true;
                }
                else if (angle >= -112.5f && angle < -67.5f)
                    dir = new(0, -1); // South
                else if (angle >= -157.5f && angle < -112.5f)
                {
                    dir = new(-1, -1); // South-West
                    isDiagonal = true;
                }
                else
                    dir = new(-1, 0); // West
            }
            else
            {
                // 4방향 스냅 (각 90도 간격, 중심 +- 45도)
                if (angle >= -45f && angle < 45f)
                    dir = new(1, 0); // East
                else if (angle >= 45f && angle < 135f)
                    dir = new(0, 1); // North
                else if (angle >= -135f && angle < -45f)
                    dir = new(0, -1); // South
                else
                    dir = new(-1, 0); // West
            }

            // 반경 마이그레이션 및 유효성 검증
            int inR = Mathf.Max(0, innerRadius);
            int outR = outerRadius > 0 ? outerRadius : Mathf.Max(1, radius);
            if (outR <= inR)
            {
                outR = inR + 1;
            }

            int startR = inR + 1;
            int endR = outR;
            int width = Mathf.Max(1, arcWidth);
            int baseHalf = width / 2;

            for (int r = startR; r <= endR; r++)
            {
                int layerOffset = expandWidthPerLayer ? (r - startR) : 0;
                int currentHalf = baseHalf + layerOffset;

                if (!isDiagonal)
                {
                    // 직교 방향 (N, S, E, W)
                    if (dir.x == 0) // North or South
                    {
                        int targetY = selfPoint.y + dir.y * r;
                        for (int offset = -currentHalf; offset <= currentHalf; offset++)
                        {
                            resultSet.Add(new _prototype_Point(selfPoint.x + offset, targetY));
                        }
                    }
                    else // East or West
                    {
                        int targetX = selfPoint.x + dir.x * r;
                        for (int offset = -currentHalf; offset <= currentHalf; offset++)
                        {
                            resultSet.Add(new _prototype_Point(targetX, selfPoint.y + offset));
                        }
                    }
                }
                else
                {
                    // 대각선 방향 (NE, NW, SE, SW): 각 r 단계마다 L자형 코너 띠
                    int cornerX = selfPoint.x + dir.x * r;
                    int cornerY = selfPoint.y + dir.y * r;
                    resultSet.Add(new _prototype_Point(cornerX, cornerY));

                    for (int step = 1; step <= currentHalf; step++)
                    {
                        resultSet.Add(new _prototype_Point(cornerX - dir.x * step, cornerY));
                        resultSet.Add(new _prototype_Point(cornerX, cornerY - dir.y * step));
                    }
                }
            }

            return new List<_prototype_Point>(resultSet);
        }
    }
}
