using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace TDG0407._prototype
{
    
    public static class _prototype_TickManager
    {
        
        private static int currentTick = 0;

        private static event Func<UniTask> OnPreTick;
        private static event Func<UniTask> OnTick;
        private static event Func<UniTask> OnPostTick;

        public static int CurrentTick { get { return currentTick; } }

        public static void Initialize()
        {
            currentTick = 0;

            OnPreTick = null;
            OnTick = null;
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
                await InvokeEventAsync(OnTick);
                await InvokeEventAsync(OnPostTick);
            }
            finally
            {
                IsTickProcessing = false;
            }
        }

        public static void RegisterPreTick(Func<UniTask> callback) => OnPreTick += callback;
        public static void UnregisterPreTick(Func<UniTask> callback) => OnPreTick -= callback;
        public static void RegisterTick(Func<UniTask> callback) => OnTick += callback;
        public static void UnregisterTick(Func<UniTask> callback) => OnTick -= callback;
        public static void RegisterPostTick(Func<UniTask> callback) => OnPostTick += callback;
        public static void UnregisterPostTick(Func<UniTask> callback) => OnPostTick -= callback;



    }

}