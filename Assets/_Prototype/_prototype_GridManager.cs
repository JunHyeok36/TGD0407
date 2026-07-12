using System;
using System.Collections.Generic;
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

        public static _prototype_GridManager Instance { get; private set; }

        [Header("References")]
        [SerializeField, ReadOnly] private _prototype_Point minPoint = _prototype_Point.zero;
        [SerializeField, ReadOnly] private _prototype_Point maxPoint = _prototype_Point.zero; 
        [SerializeField, ReadOnly] private List<_prototype_PointView> pointViews;


        private Dictionary<_prototype_Point, _prototype_PointView> pointViewMap;


        public _prototype_Point MinPoint { get { return minPoint; } }
        public _prototype_Point MaxPoint { get { return maxPoint; } }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void Initialize()
        {
            // Initialize pointViews and pointViewMap
            pointViewMap = new();
            foreach (var pointView in pointViews)
            {
                Vector3 pointViewPosition = pointView.transform.localPosition;
                _prototype_Point point = new((int)pointViewPosition.x, (int)pointViewPosition.z);

                pointView.name = $"Point ({point.x}, {point.y})";
                pointView.Initialize(point);
                pointViewMap.Add(point, pointView);

                if (point.x < minPoint.x) minPoint.x = point.x;
                if (point.y < minPoint.y) minPoint.y = point.y;
                if (point.x > maxPoint.x) maxPoint.x = point.x;
                if (point.y > maxPoint.y) maxPoint.y = point.y;
            }
        }

        public _prototype_PointView GetPointView(_prototype_Point point)
        {
            if (pointViewMap.TryGetValue(point, out var pointView)) return pointView;
            return null;
        }

        public List<_prototype_PointView> FindPath(_prototype_Point startPoint, _prototype_Point endPoint, bool includeDiagonals = false, bool checkEntityPlaceableInTerminatedPoints = false)
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

            Func<_prototype_PointView, bool> CheckNode = null;
            if (checkEntityPlaceableInTerminatedPoints)
                CheckNode = (pointView) => pointView == startPointView || pointView == endPointView || pointView.IsEntityPlaceable;
            else
                CheckNode = (_) => true;

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

                    // 새로운 G Cost 계산 (여기서는 한 칸 이동 비용을 1으로 가점)
                    float newMovementCostToNeighbor = currentNode.gCost + 1;
                    
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