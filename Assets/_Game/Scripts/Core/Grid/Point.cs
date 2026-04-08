using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDG0407.Core.Grid
{

    /// <summary>
    /// 지점에 대한 좌표 값을 나타냅니다.
    /// </summary>
    [Serializable]
    public struct Point : IComparable<Point>, IEquatable<Point>
    {
#region Fields

        [SerializeField] private int _x;
        [SerializeField] private int _y;

#endregion
#region Properties

        /// <summary>
        /// X좌표를 나타냅니다.
        /// </summary>
        public int X { readonly get => _x; set => _x = value; }
        /// <summary>
        /// Y좌표를 나타냅니다. 
        /// </summary>
        public int Y { readonly get => _y; set => _y = value; }

#endregion
#region Constructors

        public Point(int x = 0, int y = 0)
        {
            _x = x;
            _y = y;
        }
        public Point(Point other) : this(other._x, other._y) { }
        public Point(Vector2 vector) : this((int)vector.x, (int)vector.y) { }
        public Point(Vector3 vector) : this((int)vector.x, (int)vector.y) { }

#endregion
#region Methods

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(_x, _y);
        }

        /// <summary>
        /// 두 좌표를 비교합니다.
        /// </summary>
        /// <param name="other">비교할 좌표입니다.</param>
        /// <returns>두 좌표를 비교하여 this가 더 크면 -1을, other가 더 크면 1을, 같으면 0을 int 형식으로 반환합니다.</returns>
        public readonly int CompareTo(Point other)
        {
            if(_y != other._y)
                return _y.CompareTo(other._y);
            else
                return _x.CompareTo(other._x);
        }

        /// <summary>
        /// 두 지점의 좌표가 같은지 여부를 반환합니다.
        /// </summary>
        /// <param name="other">비교할 지점을 나타냅니다..</param>
        /// <returns>두 지점의 좌표가 같으면 true, 다르면 false를 반환합니다.</returns>
        public readonly bool Equals(Point other)
        {
            return _x == other._x && _y == other._y;
        }
        public override readonly bool Equals(object obj)
        {
            return obj is Point point && Equals(point);
        }

        /// <summary>
        /// 지점을 문자열로 변환하여 반환합니다.
        /// </summary>
        /// <returns>해당 문자열을 (x, y)의 문자열 형태로 반환합니다.</returns>
        public override readonly string ToString()
        {
            return $"({X}, {Y})";
        }

#endregion
#region Operators

        public static implicit operator Vector2(Point point) => new(point._x, point._y);
        public static implicit operator Point(Vector2 vector) => new((int)vector.x, (int)vector.y);

        public static Point operator +(Point a, Point b) => new(a._x + b._x, a._y + b._y);
        public static Point operator -(Point a, Point b) => new(a._x - b._x, a._y - b._y);
        public static Point operator *(Point a, int m) => new(a._x * m, a._y * m);
        public static Point operator /(Point a, int d) => new(a._x / d, a._y / d);

        public static bool operator == (Point a, Point b) => a._x == b._x && a._y == b._y;
        public static bool operator != (Point a, Point b) => a._x != b._x || a._y != b._y;

#endregion
#region Static Fields

        /// <summary>
        /// (0, 0) 저점을 나타냅니다.
        /// </summary>
        public static readonly Point zero = new(0, 0);
        /// <summary>
        /// (1, 1) 지점을 나타냅니다.
        /// </summary>
        public static readonly Point one = new(1, 1);

#endregion
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Point))]
    public class PointDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
        {
            SerializedProperty xProp = property.FindPropertyRelative("_x"); 
            SerializedProperty yProp = property.FindPropertyRelative("_y"); 

            EditorGUI.BeginProperty(position, label, property); 
            position = EditorGUI.PrefixLabel(position, label); 

            float halfWidth = position.width / 2; 
            Rect xRect = new(position.x, position.y, halfWidth - 5, position.height); 
            Rect yRect = new(position.x + halfWidth + 5, position.y, halfWidth - 5, position.height); 

            EditorGUI.LabelField(xRect, "X");
            EditorGUI.PropertyField(new Rect(xRect.x + 20, xRect.y, xRect.width - 20, xRect.height), xProp, GUIContent.none); 
            EditorGUI.LabelField(yRect, "Y");
            EditorGUI.PropertyField(new Rect(yRect.x + 20, yRect.y, yRect.width - 20, yRect.height), yProp, GUIContent.none); 

            EditorGUI.EndProperty(); 
        }
    }
#endif
}
