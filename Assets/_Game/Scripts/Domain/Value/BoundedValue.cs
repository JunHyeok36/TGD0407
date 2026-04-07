using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDG0407.Domain.Value
{

    /// <summary>
    /// 최소값과 최대값 사이의 범위를 가지는 값을 나타냅니다.
    /// </summary>
    [Serializable]
    public class BoundedValue<T> : IEquatable<BoundedValue<T>> where T : IComparable<T>, IEquatable<T>
    {
#region Fields

        [SerializeField] private T _min;
        [SerializeField] private T _max;
        [SerializeField] private T _cur;

#endregion
#region Properties

        /// <summary>
        /// 최소값을 나타냅니다.
        /// </summary>
        public T Min 
        { 
            get => _min; 
            set 
            {
                _min = value;
                if (_cur.CompareTo(_min) < 0)
                    _cur = _min;
            } 
        }
        /// <summary> 
        /// 최대값을 나타냅니다.
        /// </summary>
        public T Max 
        { 
            get => _max; 
            set
            {
                _max = value;
                if (_cur.CompareTo(_max) > 0)
                    _cur = _max;
            }
        }
        /// <summary>
        /// 현재 값을 나타냅니다. 현재 값이 최소값보다 작거나 최대값보다 크면, 최소값 또는 최대값으로 설정됩니다.
        /// </summary>
        public T Current
        {
            get => _cur;
            set
            {
                if (value.CompareTo(_min) < 0)
                    _cur = _min;
                else if (value.CompareTo(_max) > 0)
                    _cur = _max;
                else
                    _cur = value;
            }
        }

        /// <summary>
        /// 현재 값이 최소값인지 여부를 반환합니다.
        /// </summary>
        public bool IsMinimum => _cur.Equals(_min);
        /// <summary>
        /// 현재 값이 최대값인지 여부를 반환합니다.
        /// </summary>
        public bool IsMaximum => _cur.Equals(_max);
        /// <summary>
        /// 최소값과 최대값에 대한 현재 값의 비율을 반환합니다. 최소값과 최대값이 같으면 0을 반환합니다.
        /// </summary>
        public float Ratio 
        {
            get
            {
                if (_max.CompareTo(_min) == 0)
                    return 0f;
                else
                    return (Convert.ToSingle(_cur) - Convert.ToSingle(_min)) / (Convert.ToSingle(_max) - Convert.ToSingle(_min));
            }
        }

#endregion
#region Constructors

        public BoundedValue(T min, T max, T cur)
        {
            _min = min;
            _max = max;
            _cur = cur;
        }
        public BoundedValue(T min, T max) : this(min, max, min) { }
        public BoundedValue(BoundedValue<T> other) : this(other._min, other._max, other._cur) { }

#endregion
#region Methods

        public bool Equals(BoundedValue<T> other)
        {
            return _min.Equals(other._min) && _max.Equals(other._max) && _cur.Equals(other._cur);
        }
        public override bool Equals(object obj)
        {
            return obj is BoundedValue<T> other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(_min, _max, _cur);
        }

#endregion
#region Operators

        public static bool operator ==(BoundedValue<T> left, BoundedValue<T> right) => left.Current.Equals(right.Current);
        public static bool operator !=(BoundedValue<T> left, BoundedValue<T> right) => !left.Current.Equals(right.Current);

        public static implicit operator T(BoundedValue<T> boundedValue) => boundedValue.Current;

#endregion

    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BoundedValue<>), true)]
    public class BoundedValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var minProp = property.FindPropertyRelative("_min");
            var maxProp = property.FindPropertyRelative("_max");
            var curProp = property.FindPropertyRelative("_cur");

            float fieldWidth = position.width / 3f;
            Rect minRect = new Rect(position.x, position.y, fieldWidth, position.height);
            Rect maxRect = new Rect(position.x + fieldWidth, position.y, fieldWidth, position.height);
            Rect curRect = new Rect(position.x + 2 * fieldWidth, position.y, fieldWidth, position.height);

            EditorGUI.PropertyField(minRect, minProp, new GUIContent("Min"));
            EditorGUI.PropertyField(maxRect, maxProp, new GUIContent("Max"));
            EditorGUI.PropertyField(curRect, curProp, new GUIContent("Current"));

            EditorGUI.EndProperty();
        }
    }
#endif
}