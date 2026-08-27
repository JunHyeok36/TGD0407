using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_CardAction
    {
        [Header("Targeting Options")]
        public bool includeSelf = false;

        public abstract UniTask ExecuteCardAction(
            _prototype_EntityData source, 
            IEnumerable<_prototype_EntityData> targets,
            _prototype_CardActionParams @params);
    }
}
