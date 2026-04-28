using System;
using System.Collections.Generic;

namespace TDG0407.Systems.Signals
{

    /// <summary>
    /// 시스템 간 결합도를 낮추기 위한 경량 시그널 허브입니다.
    /// </summary>
    public static class SignalHub
    {
        #region Fields

        private static readonly Dictionary<Type, Delegate> handlersByType = new();

        #endregion
        #region Methods

        public static void Subscribe<TSignal>(Action<TSignal> handler)
        {
            if(handler == null)
                return;

            Type signalType = typeof(TSignal);
            if(handlersByType.TryGetValue(signalType, out Delegate existingHandlers))
            {
                handlersByType[signalType] = Delegate.Combine(existingHandlers, handler);
                return;
            }

            handlersByType.Add(signalType, handler);
        }

        public static void Unsubscribe<TSignal>(Action<TSignal> handler)
        {
            if(handler == null)
                return;

            Type signalType = typeof(TSignal);
            if(handlersByType.TryGetValue(signalType, out Delegate existingHandlers) == false)
                return;

            Delegate updatedHandlers = Delegate.Remove(existingHandlers, handler);
            if(updatedHandlers == null)
            {
                handlersByType.Remove(signalType);
                return;
            }

            handlersByType[signalType] = updatedHandlers;
        }

        public static void Publish<TSignal>(TSignal signal)
        {
            if(handlersByType.TryGetValue(typeof(TSignal), out Delegate handlers) == false)
                return;

            if(handlers is Action<TSignal> signalHandlers)
                signalHandlers.Invoke(signal);
        }

        public static void Clear()
        {
            handlersByType.Clear();
        }

        #endregion
    }

}
