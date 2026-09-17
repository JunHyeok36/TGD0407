using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public abstract class _prototype_EntityDataModel : ScriptableObject
    {

        public string ename = "entity";
        public _prototype_BoundedValue<int> health = new(0, 100);
        public _prototype_BoundedValue<int> stamina = new(0, 10);
        public _prototype_Point point = _prototype_Point.zero;
        public _prototype_Point size = new(1, 1);
        public _prototype_MovementType movementType = _prototype_MovementType.Ground;
        public _prototype_Bounds<float> heightBounds = new(0.0f, 1.8f);
        
        [SerializeReference, SubclassSelector] public List<_prototype_EntityComponentDataModel> components = new();

        public abstract _prototype_EntityData CreateData(
            _prototype_EntityData source = null);

        protected void PopulateComponents(_prototype_EntityData data)
        {
            if (components != null)
            {
                foreach (var comp in components)
                {
                    if (comp != null)
                    {
                        data.components.Add(comp.CreateComponentData());
                    }
                }
            }
        }

    }

}
