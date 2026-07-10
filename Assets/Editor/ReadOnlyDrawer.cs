#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    // 최신 Unity 버전을 위한 UI Toolkit 방식
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var propertyRoot = new PropertyField(property);
        propertyRoot.SetEnabled(false); // 필드 비활성화 (읽기 전용)
        return propertyRoot;
    }

    // 레거시 버전을 위한 IMGUI 방식
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false; // 이후 그려지는 인스펙터 GUI 비활성화
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;  // 다른 필드들을 위해 다시 활성화
    }
}
#endif