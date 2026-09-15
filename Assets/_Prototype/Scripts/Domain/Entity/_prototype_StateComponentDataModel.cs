using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_StateEntry
    {
        public string key;
        public string value;

        public _prototype_StateEntry() { }
        public _prototype_StateEntry(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public class _prototype_StateComponentDataModel : _prototype_EntityComponentDataModel
    {
        public List<_prototype_StateEntry> initialStates = new();

        public override _prototype_EntityComponentData CreateComponentData()
        {
            return new _prototype_StateComponentData(initialStates);
        }
    }
}
