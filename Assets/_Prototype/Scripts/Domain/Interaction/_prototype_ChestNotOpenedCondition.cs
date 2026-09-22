using System;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_ChestNotOpenedCondition : _prototype_Condition
    {
        public override _prototype_ConditionResult Evaluate(_prototype_ConditionContext context)
        {
            if (context == null) return _prototype_ConditionResult.Fail("Context is null.");

            var targetEntity = context.Target;
            if (targetEntity == null)
            {
                return _prototype_ConditionResult.Fail("Target entity is null.");
            }

            if (targetEntity.components == null)
            {
                return _prototype_ConditionResult.Fail("Entity has no components.");
            }

            _prototype_ChestComponentData chestComp = null;
            foreach (var comp in targetEntity.components)
            {
                if (comp is _prototype_ChestComponentData cc)
                {
                    chestComp = cc;
                    break;
                }
            }

            if (chestComp == null)
            {
                return _prototype_ConditionResult.Fail("Entity does not have ChestComponentData.");
            }

            if (!chestComp.isOpened)
            {
                return _prototype_ConditionResult.Success();
            }

            return _prototype_ConditionResult.Fail("Chest is already opened.");
        }
    }
}
