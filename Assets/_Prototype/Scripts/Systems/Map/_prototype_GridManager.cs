using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{

    public class _prototype_GridManager : MonoBehaviour
    {
        private class Node : IComparable<Node>
        {
            public _prototype_PointView pointView;
            public Node parent;
            public float gCost; // 시작점부터의 거리
            public float hCost; // 목적지까지의 예상 거리
            public float FCost => gCost + hCost; // 총점

            public Node(_prototype_PointView pointView) => this.pointView = pointView;

            // 점수가 낮은 노드가 우선순위를 갖도록 정렬 기준 정의
            public int CompareTo(Node other)
            {
                int compare = FCost.CompareTo(other.FCost);
                if (compare == 0) compare = hCost.CompareTo(other.hCost);
                return compare;
            }
        }

        private static _prototype_GridManager _instance;
        public static _prototype_GridManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<_prototype_GridManager>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("References")]
        [SerializeField, ReadOnly] private _prototype_Point minPoint = _prototype_Point.zero;
        [SerializeField, ReadOnly] private _prototype_Point maxPoint = _prototype_Point.zero; 
        [SerializeField, ReadOnly] private List<_prototype_PointView> pointViews;


        private Dictionary<_prototype_Point, _prototype_PointView> pointViewMap;


        public _prototype_Point MinPoint { get { return minPoint; } }
        public _prototype_Point MaxPoint { get { return maxPoint; } }

        public List<_prototype_PointView> PointViews { get { return pointViews; } }
        public List<_prototype_EntityView> GetAllEntityViews()
        {
            HashSet<_prototype_EntityView> entityViews = new();
            if (pointViewMap == null) return entityViews.ToList();
            foreach (var pointView in pointViewMap.Values)
            {
                if (pointView.PlacedEntityViews != null)
                {
                    foreach(var view in pointView.PlacedEntityViews) entityViews.Add(view);
                }
            }
            return entityViews.ToList();
        }
        public List<_prototype_LifeView> GetAllLifeViews()
        {
            HashSet<_prototype_LifeView> lifeViews = new();
            if (pointViewMap == null) return lifeViews.ToList();
            foreach (var pointView in pointViewMap.Values)
            {
                if (pointView.PlacedEntityViews != null)
                {
                    foreach (var entityView in pointView.PlacedEntityViews)
                    {
                        if (entityView is _prototype_LifeView lifeView)
                            lifeViews.Add(lifeView);
                    }
                }
            }
            return lifeViews.ToList();
        }

        public List<_prototype_PointView> GetFootprint(_prototype_Point point, _prototype_Point size)
        {
            if (size.x <= 1 && size.y <= 1)
            {
                var pv = GetPointView(point);
                return pv != null ? new List<_prototype_PointView> { pv } : null;
            }

            List<_prototype_PointView> footprint = new();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var pv = GetPointView(point + new _prototype_Point(x, y));
                    if (pv == null) return null; // out of bounds footprint
                    footprint.Add(pv);
                }
            }
            return footprint;
        }

        public bool CanPlaceEntityFootprint(_prototype_EntityData entityData, _prototype_Point point)
        {
            if (entityData == null) return false;
            var fp = GetFootprint(point, entityData.size);
            if (fp == null) return false;
            foreach (var pv in fp)
            {
                if (!pv.CanPlaceEntitySingleTile(entityData)) return false;
            }
            return true;
        }

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else if (_instance != this) Destroy(gameObject);
        }

        public void Initialize()
        {
            // Initialize pointViews and pointViewMap
            pointViewMap = new();
            foreach (var pointView in pointViews)
            {
                Vector3 pointViewPosition = pointView.transform.localPosition;
                _prototype_Point point = new(Mathf.RoundToInt(pointViewPosition.x), Mathf.RoundToInt(pointViewPosition.z));

                pointView.name = $"Point ({point.x}, {point.y})";
                pointView.Initialize(point);
                if (!pointViewMap.ContainsKey(point))
                {
                    pointViewMap.Add(point, pointView);
                }
                else
                {
                    Debug.LogWarning($"[GridManager] Duplicate point at ({point.x}, {point.y}) by {pointView.name}. Already registered by {pointViewMap[point].name}");
                }

                if (point.x < minPoint.x) minPoint.x = point.x;
                if (point.y < minPoint.y) minPoint.y = point.y;
                if (point.x > maxPoint.x) maxPoint.x = point.x;
                if (point.y > maxPoint.y) maxPoint.y = point.y;
            }

            foreach (var entityView in GetAllEntityViews())
            {
                if (entityView != null && entityView.EntityData != null && (entityView.EntityData.size.x > 1 || entityView.EntityData.size.y > 1))
                {
                    var fp = GetFootprint(entityView.EntityData.point, entityView.EntityData.size);
                    if (fp != null)
                    {
                        foreach (var pv in fp)
                        {
                            if (!pv.PlacedEntityViews.Contains(entityView))
                            {
                                pv.PlacedEntityViews.Add(entityView);
                                pv.PointData.placedEntityDatas.Add(entityView.EntityData);
                            }
                        }
                    }
                }
            }
        }

        public _prototype_PointView GetPointView(_prototype_Point point)
        {
            if (pointViewMap == null)
            {
                pointViewMap = new();
                if (pointViews != null)
                {
                    foreach (var pv in pointViews)
                    {
                        if (pv != null)
                        {
                            Vector3 pos = pv.transform.localPosition;
                            _prototype_Point pt = new(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
                            pointViewMap[pt] = pv;
                        }
                    }
                }
            }
            if (pointViewMap != null && pointViewMap.TryGetValue(point, out var pointView)) return pointView;
            return null;
        }

        public List<_prototype_PointView> FindPath(
            _prototype_Point startPoint,
            _prototype_Point endPoint,
            _prototype_EntityData entityData,
            bool includeDiagonals = false,
            bool checkEntityPlaceableInTerminatedPoints = false,
            HashSet<_prototype_Point> hazardousPoints = null)
        {
            if (!IsWithinBounds(startPoint) || !IsWithinBounds(endPoint)) return null;
            _prototype_PointView startPointView = GetPointView(startPoint);
            _prototype_PointView endPointView = GetPointView(endPoint);
            if (startPointView == null || endPointView == null) return null;

            // 2. 오픈 리스트(탐색 예정)와 클로즈 리스트(탐색 완료) 준비
            List<Node> openList = new();
            HashSet<_prototype_PointView> closedList = new();

            Node startNode = new(startPointView);
            Node endNode = new(endPointView);
            openList.Add(startNode);

            Func<_prototype_PointView, bool> CheckNode = (pointView) =>
            {
                if (entityData != null && (entityData.size.x > 1 || entityData.size.y > 1))
                {
                    var fp = GetFootprint(pointView.Point, entityData.size);
                    if (fp == null) return false;
                    foreach (var pv in fp)
                    {
                        bool valid = pv == startPointView;
                        if (checkEntityPlaceableInTerminatedPoints && pointView == endPointView && pv == endPointView) valid = true;
                        if (!valid && !pv.CanPlaceEntitySingleTile(entityData)) return false;
                    }
                    return true;
                }
                else
                {
                    if (checkEntityPlaceableInTerminatedPoints)
                        return pointView == startPointView || pointView == endPointView || pointView.CanPlaceEntitySingleTile(entityData);
                    else
                        return pointView.CanPlaceEntitySingleTile(entityData);
                }
            };

            Func<_prototype_PointView, bool> IsSafeNode = (pointView) =>
            {
                if (hazardousPoints == null) return true;
                if (entityData != null && (entityData.size.x > 1 || entityData.size.y > 1))
                {
                    var fp = GetFootprint(pointView.Point, entityData.size);
                    if (fp != null)
                    {
                        foreach (var pv in fp)
                        {
                            if (hazardousPoints.Contains(pv.Point) && pv != startPointView) return false;
                        }
                    }
                }
                return !hazardousPoints.Contains(pointView.Point) || pointView == startPointView;
            };

            while (openList.Count > 0)
            {
                // 오픈 리스트에서 가장 점수(F)가 낮은 노드를 꺼냅니다.
                openList.Sort();
                Node currentNode = openList[0];
                openList.RemoveAt(0);

                closedList.Add(currentNode.pointView);

                // 목적지에 도착했다면 역추적하여 경로 완성
                if (currentNode.pointView.Point == endNode.pointView.Point)
                {
                    return RetracePath(startNode, currentNode);
                }

                // 인접한 이웃 타일(4방향 또는 8방향)을 순회합니다.
                foreach (_prototype_PointView neighborPointView in GetNeighbors(currentNode.pointView, includeDiagonals))
                {
                    // 이미 탐색 완료한 곳이면 패스
                    if (closedList.Contains(neighborPointView)) continue;
                    if (!CheckNode(neighborPointView)) continue;

                    // 새로운 G Cost 계산 (여기서는 한 칸 이동 비용을 1으로 가점, 위험한 곳은 비용 100 추가)
                    float newMovementCostToNeighbor = currentNode.gCost + 1;
                    if (!IsSafeNode(neighborPointView)) newMovementCostToNeighbor += 100f;

                    Node neighborNode = openList.Find(n => n.pointView == neighborPointView);

                    if (neighborNode == null)
                    {
                        neighborNode = new(neighborPointView)
                        {
                            gCost = newMovementCostToNeighbor ,
                            hCost = GetDistance(neighborPointView.Point, endPoint),
                            parent = currentNode
                        };
                        openList.Add(neighborNode);
                    }
                    else if (newMovementCostToNeighbor < neighborNode.gCost)
                    {
                        // 더 유효한(짧은) 경로를 찾았다면 비용과 부모 갱신
                        neighborNode.gCost = newMovementCostToNeighbor;
                        neighborNode.parent = currentNode;
                    }
                }
            }

            return null; // 경로를 찾지 못함
        }

        private List<_prototype_PointView> RetracePath(Node startNode, Node endNode)
        {
            List<_prototype_PointView> path = new();
            Node currentNode = endNode;

            // 목적지부터 부모를 타고 올라가며 역추적
            while (currentNode != startNode)
            {
                path.Add(currentNode.pointView);
                currentNode = currentNode.parent;
            }
            path.Reverse(); // 시작점 -> 목적지 순서가 되도록 뒤집기
            return path;
        }


        private float GetDistance(_prototype_Point a, _prototype_Point b)
        {
            float distanceX = Mathf.Abs(a.x - b.x);
            float distanceY = Mathf.Abs(a.y - b.y);
            return distanceX*distanceX + distanceY*distanceY; 
        }

        // 맵 밖으로 벗어났는지 확인하는 함수
        public bool IsWithinBounds(_prototype_Point p) 
        {
            if (pointViewMap == null) return true;
            return pointViewMap.ContainsKey(p);
        }
        // 인접한 4방향 타일을 가져오는 함수
        private List<_prototype_PointView> GetNeighbors(_prototype_PointView pv, bool includeDiagonals = false)
        {
            List<_prototype_PointView> neighbors = new();

            // 상하좌우 4방향
            _prototype_Point[] directions = new _prototype_Point[]
            {
                new(0, 1),   // 위
                new(1, 0),   // 오른쪽
                new(0, -1),  // 아래
                new(-1, 0)   // 왼쪽
            };

            foreach (var dir in directions)
            {
                _prototype_Point neighborPoint = pv.Point + dir;
                if (IsWithinBounds(neighborPoint))
                {
                    neighbors.Add(GetPointView(neighborPoint));
                }
            }

            if (includeDiagonals)
            {
                // 대각선 방향 추가
                _prototype_Point[] diagonalDirections = new _prototype_Point[]
                {
                    new(1, 1),    // 오른쪽 위
                    new(1, -1),   // 오른쪽 아래
                    new(-1, -1),  // 왼쪽 아래
                    new(-1, 1)    // 왼쪽 위
                };

                foreach (var dir in diagonalDirections)
                {
                    _prototype_Point neighborPoint = pv.Point + dir;
                    if (IsWithinBounds(neighborPoint))
                    {
                        neighbors.Add(GetPointView(neighborPoint));
                    }
                }
            }

            return neighbors;
        }

#if UNITY_EDITOR
        [ContextMenu("Get Point Views")]
        private void GetPointViews()
        {
            _prototype_PointView[] pointViewsArray = GetComponentsInChildren<_prototype_PointView>();
            pointViews = new(pointViewsArray);

            foreach (var pointView in pointViews)
            {
                Vector3 pointViewPosition = pointView.transform.localPosition;
                _prototype_Point point = new(Mathf.RoundToInt(pointViewPosition.x), Mathf.RoundToInt(pointViewPosition.z));

                pointView.name = $"Point ({point.x}, {point.y})";
                pointView.Initialize(point);

                if (point.x < minPoint.x) minPoint.x = point.x;
                if (point.y < minPoint.y) minPoint.y = point.y;
                if (point.x > maxPoint.x) maxPoint.x = point.x;
                if (point.y > maxPoint.y) maxPoint.y = point.y;
            }

            var sortedPointViews = new List<_prototype_PointView>(pointViews);
            sortedPointViews.Sort((a, b) => a.Point.x != b.Point.x ? a.Point.x.CompareTo(b.Point.x) : a.Point.y.CompareTo(b.Point.y));

            pointViews = sortedPointViews;
            for (int i = 0; i < pointViews.Count; i++)
            {
                pointViews[i].transform.SetSiblingIndex(i);
            }
        }
#endif

    }

}