using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_TestInteractionEntityAction : _prototype_EntityAction
    {
        public string message = "Interactable action executed.";

        public override UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            Debug.Log($"[InteractableTest] {message} Source: {source?.ename ?? "None"}");
            return UniTask.CompletedTask;
        }
    }

}