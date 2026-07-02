using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
#endif

namespace TDG0407.Domain.Entities
{
    
    using Core.Value;
    using Core.Grid;

    /// <summary>
    /// 레벨의 모든 Point 위에 존재할 수 있는 공통 Entity 상태입니다.
    /// </summary>
    [Serializable]
    public abstract class EntityState
    {
        #region Fields

        public int? entityInstanceId = null;
        public string entityId = null;
        public Point position = Point.zero;
        public readonly BoundedValue<int> health = new(0, 50);
        public readonly BoundedValue<int> stamina = new(0, 10);
        public readonly Queue<Shield> shields = new();
        
        #endregion
        #region Constructors

        protected EntityState() {}
        protected EntityState(
            int entityInstanceId,
            string entityId,
            Point position,
            BoundedValue<int> health,
            BoundedValue<int> stamina,
            IEnumerable<Shield> shields = null)
        {
            this.entityInstanceId = entityInstanceId;
            this.entityId = entityId;
            this.position = position;
            this.health = health;
            this.stamina = stamina;

            if (shields != null)
            {
                foreach (var shield in shields)
                    this.shields.Enqueue(shield);
            }   
        }

        public EntityState(EntityState other)
        {
            this.entityInstanceId = other.entityInstanceId;
            this.entityId = other.entityId;
            this.position = other.position;
            this.health = other.health.Clone();
            this.stamina = other.stamina.Clone();

            foreach (var shield in other.shields)
                this.shields.Enqueue(shield.Clone());
        }

        #endregion
        #region Methods

        public virtual void Initialize()
        {
            
        }

        public abstract EntityState Clone();

        #endregion
        
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EntityState), true)]
    public class EntityStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            string typeName = property.managedReferenceValue?.GetType().Name ?? "None";
            
            Rect buttonRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent($"{label.text} ({typeName})"), FocusType.Keyboard))
            {
                GenericMenu menu = new();
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => typeof(EntityState).IsAssignableFrom(p) && !p.IsAbstract);

                foreach (var type in types)
                {
                    menu.AddItem(new GUIContent(type.Name), false, () => {
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }

            if (property.managedReferenceValue != null)
            {
                position.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(position, property, true);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue != null && property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(property, true);
            }
            
            return EditorGUIUtility.singleLineHeight * 2;
        }
    }
    #endif

}