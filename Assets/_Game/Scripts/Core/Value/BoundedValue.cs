using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDG0407.Core.Value
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
            Initalize(min, max, cur);
        }
        public BoundedValue(T min, T max) : this(min, max, max) { }
        public BoundedValue(BoundedValue<T> other) : this(other._min, other._max, other._cur) { }

        #endregion
        #region Methods

        public void Initalize(T min, T max, T cur)
        {
            _min = min;
            _max = max;
            Current = cur;
        }
        public void Initalize(T min, T max) => Initalize(min, max, max);

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

    [CustomPropertyDrawer(typeof(BoundedValue<>))]
    public class BoundedValueDrawer : PropertyDrawer
    {
        private bool _isExpanded = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty minProp = property.FindPropertyRelative("_min");
            SerializedProperty maxProp = property.FindPropertyRelative("_max");
            SerializedProperty curProp = property.FindPropertyRelative("_cur");

            bool isNumeric = minProp.propertyType == SerializedPropertyType.Integer || minProp.propertyType == SerializedPropertyType.Float;

            label = EditorGUI.BeginProperty(position, label, property);

            if (isNumeric)
            {
                position = EditorGUI.PrefixLabel(position, label);

                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                float spacing = 4f;
                float fieldWidth = 40f;
                float symbolWidth = 10f;
                
                float sliderWidth = position.width - (fieldWidth * 2) - (symbolWidth * 2) - spacing;

                Rect minRect = new Rect(position.x, position.y, fieldWidth, position.height);
                Rect hyphenRect = new Rect(minRect.xMax, position.y, symbolWidth, position.height);
                Rect maxRect = new Rect(hyphenRect.xMax, position.y, fieldWidth, position.height);
                Rect barRect = new Rect(maxRect.xMax + spacing, position.y, symbolWidth, position.height);
                Rect sliderRect = new Rect(barRect.xMax, position.y, sliderWidth, position.height);

                GUIStyle centerStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
                GUI.Label(hyphenRect, "-", centerStyle);
                GUI.Label(barRect, "|", centerStyle);

                EditorGUI.BeginChangeCheck();

                EditorGUI.PropertyField(minRect, minProp, GUIContent.none);
                EditorGUI.PropertyField(maxRect, maxProp, GUIContent.none);

                if (minProp.propertyType == SerializedPropertyType.Integer)
                {
                    curProp.intValue = EditorGUI.IntSlider(sliderRect, GUIContent.none, curProp.intValue, minProp.intValue, maxProp.intValue);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (minProp.intValue > maxProp.intValue) minProp.intValue = maxProp.intValue;
                        curProp.intValue = Mathf.Clamp(curProp.intValue, minProp.intValue, maxProp.intValue);
                    }
                }
                else
                {
                    curProp.floatValue = EditorGUI.Slider(sliderRect, GUIContent.none, curProp.floatValue, minProp.floatValue, maxProp.floatValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (minProp.floatValue > maxProp.floatValue) minProp.floatValue = maxProp.floatValue;
                        curProp.floatValue = Mathf.Clamp(curProp.floatValue, minProp.floatValue, maxProp.floatValue);
                    }
                }
                EditorGUI.indentLevel = indent;
            }
            else
            {
                float labelWidth = EditorGUIUtility.labelWidth;
                Rect foldoutRect = new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight);
                
                _isExpanded = EditorGUI.Foldout(foldoutRect, _isExpanded, label, true);

                Rect curRect = new Rect(position.x + labelWidth + 2, position.y, position.width - labelWidth - 2, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(curRect, curProp, GUIContent.none);

                if (_isExpanded)
                {
                    EditorGUI.indentLevel++;
                    float lineHeight = EditorGUIUtility.singleLineHeight + 2;
                    
                    Rect minLineRect = new Rect(position.x, position.y + lineHeight, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(minLineRect, minProp, new GUIContent("Min"));
                    
                    Rect maxLineRect = new Rect(position.x, position.y + lineHeight * 2, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(maxLineRect, maxProp, new GUIContent("Max"));
                    
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty minProp = property.FindPropertyRelative("_min");
            bool isNumeric = minProp.propertyType == SerializedPropertyType.Integer ||
                            minProp.propertyType == SerializedPropertyType.Float;

            if (isNumeric || !_isExpanded)
                return EditorGUIUtility.singleLineHeight;
            
            return (EditorGUIUtility.singleLineHeight + 2) * 3;
        }
    }
#endif
}