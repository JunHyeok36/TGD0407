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

        public static async UniTask AdvanceTick(Func<UniTask> playerAction)
        {
            currentTick++;

            if (OnPreTick != null) await OnPreTick.Invoke();
            if (playerAction != null) await playerAction.Invoke();
            if (OnTick != null) await OnTick.Invoke();
            if (OnPostTick != null) await OnPostTick.Invoke();
        }

        public static void RegisterPreTick(Func<UniTask> callback) => OnPreTick += callback;
        public static void UnregisterPreTick(Func<UniTask> callback) => OnPreTick -= callback;
        public static void RegisterTick(Func<UniTask> callback) => OnTick += callback;
        public static void UnregisterTick(Func<UniTask> callback) => OnTick -= callback;
        public static void RegisterPostTick(Func<UniTask> callback) => OnPostTick += callback;
        public static void UnregisterPostTick(Func<UniTask> callback) => OnPostTick -= callback;



    }

}