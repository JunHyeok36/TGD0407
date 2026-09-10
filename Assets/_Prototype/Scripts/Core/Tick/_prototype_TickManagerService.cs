using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    public class _prototype_TickManagerService : MonoBehaviour, ITickManager
    {
        private static ITickManager _instance;
        public static ITickManager Instance
        {
            get
            {
                if (_instance == null || (_instance is UnityEngine.Object obj && obj == null))
                {
                    _instance = UnityEngine.Object.FindAnyObjectByType<_prototype_TickManagerService>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[_prototype_TickManagerService]");
                        _instance = go.AddComponent<_prototype_TickManagerService>();
                    }
                }
                return _instance;
            }
            set => _instance = value;
        }

        [SerializeField, ReadOnly] private int currentTick = 0;
        public int CurrentTick => currentTick;
        public bool IsTickProcessing { get; private set; }

        private event Func<UniTask> OnPreTick;
        private event TickActionPlanner OnTickPlan;
        private event Func<UniTask> OnPostTick;

        private void Awake()
        {
            if (_instance == null || (_instance is UnityEngine.Object obj && obj == null))
            {
                _instance = this;
            }
            else if ((object)_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Reset();
        }

        private void OnDestroy()
        {
            if ((object)_instance == this)
            {
                Reset();
                _instance = null;
            }
        }

        public void Reset()
        {
            currentTick = 0;
            IsTickProcessing = false;
            OnPreTick = null;
            OnTickPlan = null;
            OnPostTick = null;
        }

        public async UniTask AdvanceTick(Func<UniTask> playerAction)
        {
            if (IsTickProcessing) return;
            IsTickProcessing = true;

            try
            {
                currentTick++;

                // Publish tick event via EventBus
                _prototype_EventBus.Fire(new TickAdvancedEvent(currentTick));

                await InvokeEventAsync(OnPreTick);
                if (playerAction != null) await playerAction.Invoke();

                List<_prototype_TickIntent> playerTargetingIntents = new();
                List<_prototype_TickIntent> otherIntents = new();

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

                List<UniTask> parallelTasks = new();
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

        public void RegisterPreTick(Func<UniTask> callback) => OnPreTick += callback;
        public void UnregisterPreTick(Func<UniTask> callback) => OnPreTick -= callback;
        public void RegisterTick(TickActionPlanner callback) => OnTickPlan += callback;
        public void UnregisterTick(TickActionPlanner callback) => OnTickPlan -= callback;
        public void RegisterPostTick(Func<UniTask> callback) => OnPostTick += callback;
        public void UnregisterPostTick(Func<UniTask> callback) => OnPostTick -= callback;

        private async UniTask InvokeEventAsync(Func<UniTask> evt)
        {
            if (evt == null) return;
            foreach (Func<UniTask> handler in evt.GetInvocationList())
            {
                await handler();
            }
        }
    }
}
