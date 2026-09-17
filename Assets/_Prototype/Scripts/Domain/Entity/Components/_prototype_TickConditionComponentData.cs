using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_TickConditionComponentData : _prototype_EntityComponentData
    {
        [UnityEngine.SerializeReference, SubclassSelector]
        public List<_prototype_Condition> conditions = new();

        [UnityEngine.SerializeReference, SubclassSelector]
        public List<_prototype_EntityAction> actionList = new();

        public override async UniTask OnTick(_prototype_EntityView owner)
        {
            if (conditions != null && conditions.Count > 0)
            {
                var playMode = _prototype_PlayModeManager.Instance != null
                    ? _prototype_PlayModeManager.Instance.CurrentMode
                    : _prototype_PlayMode.Exploration;
                    
                var context = new _prototype_ConditionContext(owner.EntityData, null, playMode);

                foreach (var condition in conditions)
                {
                    if (condition == null) continue;
                    var result = condition.Evaluate(context);
                    if (!result.IsSatisfied) return;
                }
            }

            if (actionList != null)
            {
                foreach (var action in actionList)
                {
                    if (action == null) continue;
                    await action.ExecuteAction(owner.EntityData, new[] { owner.EntityData }, null);
                }
            }
        }

        public override _prototype_EntityComponentData Clone()
        {
            var clone = new _prototype_TickConditionComponentData();
            if (conditions != null) clone.conditions = new List<_prototype_Condition>(conditions);
            if (actionList != null) clone.actionList = new List<_prototype_EntityAction>(actionList);
            return clone;
        }
    }
}
