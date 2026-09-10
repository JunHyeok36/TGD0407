using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    public class _prototype_EventBus : IEventBus
    {
        private static IEventBus _instance;
        public static IEventBus Instance => _instance ??= new _prototype_EventBus();

        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Publish<T>(T eventData)
        {
            var eventType = typeof(T);
            if (!_handlers.TryGetValue(eventType, out var list) || list == null || list.Count == 0)
            {
                return;
            }

            // Copy list to allow subscribers to unsubscribe during handling without collection modified exception
            var snapshot = list.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i] is Action<T> action)
                {
                    try
                    {
                        action.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogException(ex);
                    }
                }
            }
        }

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return EmptyDisposable.Instance;

            var eventType = typeof(T);
            if (!_handlers.TryGetValue(eventType, out var list))
            {
                list = new List<Delegate>();
                _handlers[eventType] = list;
            }

            list.Add(handler);
            return new Subscription<T>(this, handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;

            var eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out var list))
            {
                list.Remove(handler);
                if (list.Count == 0)
                {
                    _handlers.Remove(eventType);
                }
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }

        // Static Facade
        public static void SetInstance(IEventBus instance) => _instance = instance;
        public static void Fire<T>(T eventData) => Instance.Publish(eventData);
        public static IDisposable Listen<T>(Action<T> handler) => Instance.Subscribe(handler);
        public static void StopListening<T>(Action<T> handler) => Instance.Unsubscribe(handler);
        public static void Reset() => Instance.Clear();

        private sealed class Subscription<T> : IDisposable
        {
            private _prototype_EventBus _bus;
            private Action<T> _handler;

            public Subscription(_prototype_EventBus bus, Action<T> handler)
            {
                _bus = bus;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_bus != null && _handler != null)
                {
                    _bus.Unsubscribe(_handler);
                    _bus = null;
                    _handler = null;
                }
            }
        }

        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new();
            public void Dispose() { }
        }
    }
}
