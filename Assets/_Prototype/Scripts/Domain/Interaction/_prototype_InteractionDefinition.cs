using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    
    [Serializable]
    public sealed class _prototype_InteractionDefinition 
    {
        [SerializeReference, SubclassSelector]
        public List<_prototype_InteractionCondition> conditions = new();

        [SerializeReference, SubclassSelector]
        public List<_prototype_EntityAction> actionList = new();

        public _prototype_BoundedValue<int> cooldown = new(0, 0);

        public bool CanExecute(_prototype_InteractionContext context, out string failureReason)
        {
            if (cooldown == null || !cooldown.IsMinimum)
            {
                failureReason = "This interaction is on cooldown.";
                return false;
            }

            if (conditions != null)
            {
                foreach (var condition in conditions)
                {
                    if (condition == null) continue;

                    var result = condition.Evaluate(context);
                    if (!result.IsSatisfied)
                    {
                        failureReason = result.FailureReason;
                        return false;
                    }
                }
            }

            failureReason = null;
            return true;
        }

        public async UniTask Execute(
            _prototype_EntityData actor,
            _prototype_InteractableData interactable)
        {
            if (actionList != null)
            {
                foreach (var action in actionList)
                {
                    if (action == null) continue;
                    await action.ExecuteAction(interactable, new[] { actor }, null);
                }
            }

            if (cooldown != null)
                cooldown.Current = cooldown.Max;
        }

        public void AdvanceCooldown()
        {
            if (cooldown != null && !cooldown.IsMinimum)
                cooldown.Current--;
        }

        public _prototype_InteractionDefinition Clone()
        {
            _prototype_InteractionDefinition clone = new()
            {
                conditions = conditions != null ? new(conditions) : new(),
                actionList = actionList != null ? new(actionList) : new(),
                cooldown = cooldown != null ? new _prototype_BoundedValue<int>(cooldown) : new(0, 0)
            };
            return clone;
        }
    }
}