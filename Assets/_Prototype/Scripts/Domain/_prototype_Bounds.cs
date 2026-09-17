using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDG0407._prototype
{
    [Serializable]
    public struct _prototype_Bounds<T> : IEquatable<_prototype_Bounds<T>> where T : IComparable<T>, IEquatable<T>
    {
        [SerializeField] private T _min;
        [SerializeField] private T _max;

        public T Min
        {
            get => _min;
            set => _min = value;
        }

        public T Max
        {
            get => _max;
            set => _max = value;
        }

        public _prototype_Bounds(T min, T max)
        {
            if (typeof(T) == typeof(float))
            {
                float fMin = (float)(object)min;
                float fMax = (float)(object)max;
                fMin = Mathf.Round(fMin * 10f) / 10f;
                fMax = Mathf.Round(fMax * 10f) / 10f;
                _min = (T)(object)fMin;
                _max = (T)(object)fMax;
            }
            else
            {
                _min = min;
                _max = max;
            }
        }

        public bool Overlaps(_prototype_Bounds<T> other)
        {
            return _min.CompareTo(other._max) <= 0 && _max.CompareTo(other._min) >= 0;
        }

        public bool Contains(T value)
        {
            return value.CompareTo(_min) >= 0 && value.CompareTo(_max) <= 0;
        }

        public bool Equals(_prototype_Bounds<T> other)
        {
            return _min.Equals(other._min) && _max.Equals(other._max);
        }

        public override bool Equals(object obj)
        {
            return obj is _prototype_Bounds<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = _min != null ? _min.GetHashCode() : 0;
                hash = (hash * 397) ^ (_max != null ? _max.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(_prototype_Bounds<T> left, _prototype_Bounds<T> right) => left.Equals(right);
        public static bool operator !=(_prototype_Bounds<T> left, _prototype_Bounds<T> right) => !left.Equals(right);

        public override string ToString() => $"[{_min} ~ {_max}]";
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(_prototype_Bounds<>))]
    public class BoundsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty minProp = property.FindPropertyRelative("_min");
            SerializedProperty maxProp = property.FindPropertyRelative("_max");

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float spacing = 4f;
            float symbolWidth = 14f;
            float fieldWidth = (position.width - symbolWidth - spacing * 2) * 0.5f;

            Rect minRect = new(position.x, position.y, fieldWidth, position.height);
            Rect tildeRect = new(minRect.xMax + spacing, position.y, symbolWidth, position.height);
            Rect maxRect = new(tildeRect.xMax + spacing, position.y, fieldWidth, position.height);

            GUIStyle centerStyle = new(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(tildeRect, "~", centerStyle);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(minRect, minProp, GUIContent.none);
            EditorGUI.PropertyField(maxRect, maxProp, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
            {
                if (minProp.propertyType == SerializedPropertyType.Float)
                {
                    minProp.floatValue = Mathf.Round(minProp.floatValue * 10f) / 10f;
                    maxProp.floatValue = Mathf.Round(maxProp.floatValue * 10f) / 10f;
                    if (minProp.floatValue > maxProp.floatValue)
                    {
                        maxProp.floatValue = minProp.floatValue;
                    }
                }
                else if (minProp.propertyType == SerializedPropertyType.Integer)
                {
                    if (minProp.intValue > maxProp.intValue)
                    {
                        maxProp.intValue = minProp.intValue;
                    }
                }
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
#endif
}
