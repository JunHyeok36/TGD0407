using System;
using System.Collections.Generic;
using System.Linq;

namespace TDG0407.Domain.Grid
{

    /// <summary>
    /// 좌표들의 집합을 나타냅니다.
    /// </summary>
    [Serializable]
    public class PointArea : IEquatable<PointArea>, ICloneable
    {
#region Fields

        /// <summary>
        /// 좌표들의 집합을 나타냅니다.
        /// </summary>
        private readonly HashSet<Point> _points = new();

#endregion
#region Properties

        /// <summary>
        /// 좌표 집합의 개수를 나타냅니다.
        /// </summary>
        public int Count => _points.Count;

#endregion
#region Constructors

        public PointArea(IEnumerable<Point> points)
        {
            _points = new HashSet<Point>(points);
        }
        public PointArea(Point point): this(new Point[] { point }) { }
        public PointArea(PointArea other) : this(other._points) { }
        public PointArea(params Point[] points) : this((IEnumerable<Point>)points) { }

#endregion
#region Methods

        public bool Equals(PointArea other) => _points.SetEquals(other._points);
        public override bool Equals(object obj) => obj is PointArea other && Equals(other);
        public override int GetHashCode() => _points != null ? _points.GetHashCode() : 0;
        public object Clone()
        {
            return new PointArea(this);
        }

        /// <summary>
        /// 지정된 좌표가 이 좌표 집합에 포함되어 있는지 여부를 반환합니다.
        /// </summary>
        /// <param name="point">확인할 좌표를 나타냅니다.</param>
        /// <returns>이 좌표 집합에 포함된 좌표라면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool Contains(Point point) => _points.Contains(point);

#endregion
#region Operators

        public static bool operator == (PointArea a, PointArea b) => a.Equals(b);
        public static bool operator != (PointArea a, PointArea b) => !(a == b);
        public static PointArea operator + (PointArea area, Point point)
        {
            PointArea res = new(area);
            res._points.Select(p => new Point(p) + point);
            return res;
        }
        public static PointArea operator - (PointArea area, Point point)
        {
            PointArea res = new(area);
            res._points.Select(p => new Point(p) - point);
            return res;
        }

        public static implicit operator HashSet<Point>(PointArea area) => area._points;

#endregion

    }

}