using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    public abstract class _prototype_AreaEffectDataModel : _prototype_EntityDataModel
    {
        [Header("Area Effect Base")]
        [SerializeReference, SubclassSelector] 
        public List<_prototype_EntityAction> onTriggerActions = new();
        
        public _prototype_Side side = _prototype_Side.None;
        public bool triggerOncePerEntity = false;
        
        public GameObject visualPiecePrefab; // 각 타일마다 생성할 시각용 프리팹
    }
}
