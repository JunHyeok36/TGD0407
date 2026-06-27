using System;
using System.Collections.Generic;
using System.Linq;

namespace TDG0407.Core.Grid
{

    public static class PointsExtensions
    {

        public static List<Point> SizeToPoints(this Point size)
        {
            List<Point> points = new();

            int halfWidth = size.X / 2;
            int halfHeight = size.Y / 2;
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                for (int y = -halfHeight; y <= halfHeight; y++)
                {
                    points.Add(new Point(x, y));
                }
            }

            return points;
        }

        public static List<Point> GetPath(IEnumerable<Point> points, Point start, Point end, Predicate<Point> filter = null)
        { // based on BFS (Breadth-First Search) algorithm
            if (!points.Contains(start) || !points.Contains(end))
                throw new ArgumentException("Start or end point is not in the provided points collection.");
            if (start == end)
                throw new ArgumentException("Start and end points cannot be the same.");


            List<Point> path = new();
            HashSet<Point> visited = new();
            Queue<Point> queue = new();
            Dictionary<Point, Point> cameFrom = new();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0) 
            {
                Point current = queue.Dequeue();

                if (current == end)
                {
                    while (current != start)
                    {
                        path.Add(current);
                        current = cameFrom[current];
                    }
                    path.Add(start);
                    path.Reverse();
                    return path;
                }

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }
            return null; // No path found

            IEnumerable<Point> GetNeighbors(Point point)
            {
                List<Point> neighbors = new();
                Point[] directions = new Point[]
                {
                    new(1, 0),  // Right
                    new(-1, 0), // Left
                    new(0, 1),  // Up
                    new(0, -1)  // Down
                };

                foreach (var dir in directions)
                {
                    Point neighbor = point + dir;
                    if (points.Contains(neighbor) && (filter == null || filter(neighbor)))
                    {
                        neighbors.Add(neighbor);
                    }
                }

                return neighbors;
            }
        }

        public static List<Point> GetNeighborPoints(this IEnumerable<Point> points, Point target)
        {
            List<Point> neighborPoints = new();
            Point[] directions = new Point[]
            {
                new(1, 0),  // Right
                new(-1, 0), // Left
                new(0, 1),  // Up
                new(0, -1)  // Down
            };

            foreach (var dir in directions)
            {
                Point neighbor = target + dir;
                if (points.Contains(neighbor))
                {
                    neighborPoints.Add(neighbor);
                }
            }

            return neighborPoints;
        }
        public static List<Point> GetAroundPoints(this IEnumerable<Point> points, Point target, byte radius = 1, bool isContainTarget = false, Predicate<Point> filter = null)
        {
            List<Point> pointList = new();
            foreach (var point in points)
            {
                if (Math.Abs(point.X - target.X) <= radius && Math.Abs(point.Y - target.Y) <= radius)
                {
                    if (!isContainTarget && point == target)
                        continue;
                    if (filter == null || filter(point))
                        pointList.Add(point);
                }
            }
            return pointList;
        }

        public static Point GetRandomPoint(this IEnumerable<Point> points, Random random = null, Predicate<Point> filter = null)
        {
            var pointList = new List<Point>(points);
            if (pointList.Count == 0) throw new InvalidOperationException("Cannot select a random point from an empty collection.");
            
            if (filter != null)
            {
                pointList = pointList.FindAll(filter);
                if (pointList.Count == 0) 
                    throw new InvalidOperationException("No points match the specified filter.");    
            }
            
            int index = random?.Next(pointList.Count) ?? new Random().Next(pointList.Count);
            return pointList[index];
        }

        public static Point GetRandomAroundPoint(this IEnumerable<Point> points, Point target, byte radius = 1, bool isContainTarget = false, Random random = null, Predicate<Point> filter = null)
        {
            var pointList = new List<Point>(points);
            if (pointList.Count == 0) throw new InvalidOperationException("Cannot select a random point from an empty collection.");
            if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be greater than zero.");

            if (filter != null)
            {
                pointList = pointList.FindAll(filter);
                if (pointList.Count == 0) 
                    throw new InvalidOperationException("No points match the specified filter.");    
            }
            if (!isContainTarget)
            {
                pointList.RemoveAll(p => p == target);
            }

            pointList.RemoveAll(p => Math.Abs(p.X - target.X) > radius || Math.Abs(p.Y - target.Y) > radius);
            int index = random?.Next(pointList.Count) ?? new Random().Next(pointList.Count);
            return pointList[index];
        }
        
    }

}