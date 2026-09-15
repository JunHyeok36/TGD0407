using System;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public abstract class _prototype_EntityComponentDataModel
    {
        public abstract _prototype_EntityComponentData CreateComponentData();
    }
    
}
