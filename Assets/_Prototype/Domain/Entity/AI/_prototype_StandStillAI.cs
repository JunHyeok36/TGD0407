using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_StandStillAI : _prototype_EnemyAILogic
    {
        public override async UniTask<_prototype_TickIntent> PlanAction(_prototype_EntityView entityView)
        {
            var intent = new _prototype_TickIntent
            {
                TargetsPlayer = false,
                Execute = async () => { await UniTask.Yield(); }
            };
            return intent;
        }

        public override UniTask ExecuteAction(_prototype_EntityView entityView)
        {
            // Do nothing
            return UniTask.CompletedTask;
        }

        public override _prototype_EnemyAILogic Clone()
        {
            return new _prototype_StandStillAI();
        }
    }
}
