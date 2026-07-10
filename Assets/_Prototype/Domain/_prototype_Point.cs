using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDG0407._prototype
{

    [Serializable]
    public struct _prototype_Point
    {

        public int x, y;

        public _prototype_Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public _prototype_Point(_prototype_Point other) : this(other.x, other.y) {}

        public override readonly string ToString() => $"({x}, {y})";

        public static bool operator ==(_prototype_Point a, _prototype_Point b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(_prototype_Point a, _prototype_Point b) => !(a == b);

        public static _prototype_Point operator +(_prototype_Point a, _prototype_Point b) => new(a.x + b.x, a.y + b.y);
        public static _prototype_Point operator -(_prototype_Point a, _prototype_Point b) => new(a.x - b.x, a.y - b.y);
        public static _prototype_Point operator *(_prototype_Point a, float m) => new((int)(a.x * m), (int)(a.y * m));
        public static _prototype_Point operator /(_prototype_Point a, float d) => new((int)(a.x / d), (int)(a.y / d));

        public static _prototype_Point zero = new(0, 0);
        public static _prototype_Point one = new(1, 1);
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(_prototype_Point))]
    public class _protortype_PointDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty xProp = property.FindPropertyRelative("x");
            SerializedProperty yProp = property.FindPropertyRelative("y");

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            float halfWidth = position.width / 2;
            Rect xRect = new(position.x, position.y, halfWidth - 5, position.height);
            Rect yRect = new(position.x + halfWidth + 5, position.y, halfWidth - 5, position.height);

            EditorGUI.LabelField(xRect, "x");
            EditorGUI.PropertyField(new Rect(xRect.x + 20, xRect.y, xRect.width - 20, xRect.height), xProp, GUIContent.none);
            EditorGUI.LabelField(yRect, "y");
            EditorGUI.PropertyField(new Rect(yRect.x + 20, yRect.y, yRect.width - 20, yRect.height), yProp, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
#endif
}
