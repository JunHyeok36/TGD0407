using System;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public abstract class _prototype_EntityDataModel : ScriptableObject
    {

        public string ename = "entity";
        public _prototype_BoundedValue<int> health = new(0, 100);
        public _prototype_BoundedValue<int> stamina = new(0, 10);
        public _prototype_Point position = _prototype_Point.zero;

    }

}
