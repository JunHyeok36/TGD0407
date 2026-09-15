using UnityEngine;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [System.Serializable]
    public class _prototype_TickConditionComponentDataModel : _prototype_EntityComponentDataModel
    {
        [SerializeReference, SubclassSelector]
        public List<_prototype_Condition> conditions = new();

        [SerializeReference, SubclassSelector]
        public List<_prototype_EntityAction> actionList = new();

        public override _prototype_EntityComponentData CreateComponentData()
        {
            var component = new _prototype_TickConditionComponentData();
            
            // Shallow copy of conditions and actions since they are mostly stateless descriptors
            if (conditions != null)
                component.conditions = new List<_prototype_Condition>(conditions);
                
            if (actionList != null)
                component.actionList = new List<_prototype_EntityAction>(actionList);

            return component;
        }
    }
}
