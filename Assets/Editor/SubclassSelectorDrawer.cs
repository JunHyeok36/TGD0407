#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        bool hasValue = property.managedReferenceValue != null;

        // 1. 첫 번째 줄 레이아웃 계산 (라벨/폴드아웃 + 클래스 선택 드롭다운 버튼)
        Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

        if (hasValue)
        {
            // 클래스가 할당되어 있다면 순정 foldout 화살표 표시
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);
        }
        else
        {
            // null 일 때는 화살표 없이 일반 라벨만 표시
            EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), label);
        }

        // 2. 클래스 선택 드롭다운 버튼 배치
        string typeName = hasValue ? property.managedReferenceValue.GetType().Name : "Null (클래스 선택)";
        if (GUI.Button(buttonRect, typeName, EditorStyles.popup))
        {
            ShowTypeMenu(property);
        }

        // 3. 화살표가 열려있고(isExpanded) 클래스가 존재하면 내부 필드들을 본래 모습 그대로 출력
        if (hasValue && property.isExpanded)
        {
            EditorGUI.indentLevel++; // 자식 필드들 들여쓰기 시작

            SerializedProperty endProperty = property.GetEndProperty();
            SerializedProperty child = property.Copy();

            float currentY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // 첫 번째 자식 필드로 진입
            if (child.NextVisible(true))
            {
                while (!SerializedProperty.EqualContents(child, endProperty))
                {
                    float childHeight = EditorGUI.GetPropertyHeight(child, true);
                    Rect childRect = new Rect(position.x, currentY, position.width, childHeight);

                    // 원래 해당 클래스가 가졌던 모양 그대로 필드를 그려줌
                    EditorGUI.PropertyField(childRect, child, true);

                    currentY += childHeight + EditorGUIUtility.standardVerticalSpacing;

                    // 다음 형제 필드로 이동 (더 깊은 자식으로 들어가지 않음)
                    if (!child.NextVisible(false))
                        break;
                }
            }

            EditorGUI.indentLevel--; // 들여쓰기 종료
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        bool hasValue = property.managedReferenceValue != null;

        // 접혀있거나 null 이면 한 줄 높이만 반환, 펼쳐져 있으면 내부 모든 자식 필드의 높이를 더함
        if (hasValue && property.isExpanded)
        {
            SerializedProperty endProperty = property.GetEndProperty();
            SerializedProperty child = property.Copy();

            if (child.NextVisible(true))
            {
                while (!SerializedProperty.EqualContents(child, endProperty))
                {
                    height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
                    if (!child.NextVisible(false))
                        break;
                }
            }
        }
        return height;
    }

    private void ShowTypeMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Null"), property.managedReferenceValue == null, () =>
        {
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        });

        Type fieldType = GetFieldType();
        if (fieldType == null) return;

        var derivedTypes = TypeCache.GetTypesDerivedFrom(fieldType)
            .Where(t => !t.IsAbstract && !t.IsInterface && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (Type type in derivedTypes)
        {
            menu.AddItem(new GUIContent(type.Name), property.managedReferenceValue?.GetType() == type, () =>
            {
                property.managedReferenceValue = Activator.CreateInstance(type);
                property.isExpanded = true; // 클래스 선택 시 자동으로 펼쳐지도록 설정
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }

    private Type GetFieldType()
    {
        Type type = fieldInfo.FieldType;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return type.GetGenericArguments()[0];
        }
        else if (type.IsArray)
        {
            return type.GetElementType();
        }
        return type;
    }
}
#endif