using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace TDG0407._prototype
{
    
    public static class _prototype_TickManager
    {
        
        private static int currentTick = 0;

        private static event Func<UniTask> OnPreTick;
        public delegate UniTask<_prototype_TickIntent> TickActionPlanner();
        private static event TickActionPlanner OnTickPlan;
        private static event Func<UniTask> OnPostTick;

        public static int CurrentTick { get { return currentTick; } }

        public static void Initialize()
        {
            currentTick = 0;

            OnPreTick = null;
            OnTickPlan = null;
            OnPostTick = null;
        }

        private static async UniTask InvokeEventAsync(Func<UniTask> evt)
        {
            if (evt == null) return;
            foreach (Func<UniTask> handler in evt.GetInvocationList())
            {
                await handler();
            }
        }

        public static bool IsTickProcessing { get; private set; }

        public static async UniTask AdvanceTick(Func<UniTask> playerAction)
        {
            if (IsTickProcessing) return;
            IsTickProcessing = true;
            
            try
            {
                currentTick++;

                await InvokeEventAsync(OnPreTick);
                if (playerAction != null) await playerAction.Invoke();
                
                System.Collections.Generic.List<_prototype_TickIntent> playerTargetingIntents = new();
                System.Collections.Generic.List<_prototype_TickIntent> otherIntents = new();

                if (OnTickPlan != null)
                {
                    foreach (TickActionPlanner planner in OnTickPlan.GetInvocationList())
                    {
                        var intent = await planner();
                        if (intent != null)
                        {
                            if (intent.TargetsPlayer) playerTargetingIntents.Add(intent);
                            else otherIntents.Add(intent);
                        }
                    }
                }

                foreach (var intent in playerTargetingIntents)
                {
                    if (intent.Execute != null) await intent.Execute();
                }

                System.Collections.Generic.List<UniTask> parallelTasks = new();
                foreach (var intent in otherIntents)
                {
                    if (intent.Execute != null) parallelTasks.Add(intent.Execute());
                }

                if (parallelTasks.Count > 0)
                {
                    await UniTask.WhenAll(parallelTasks);
                }

                await InvokeEventAsync(OnPostTick);
            }
            finally
            {
                IsTickProcessing = false;
            }
        }

        public static void RegisterPreTick(Func<UniTask> callback) => OnPreTick += callback;
        public static void UnregisterPreTick(Func<UniTask> callback) => OnPreTick -= callback;
        public static void RegisterTick(TickActionPlanner callback) => OnTickPlan += callback;
        public static void UnregisterTick(TickActionPlanner callback) => OnTickPlan -= callback;
        public static void RegisterPostTick(Func<UniTask> callback) => OnPostTick += callback;
        public static void UnregisterPostTick(Func<UniTask> callback) => OnPostTick -= callback;



    }

}