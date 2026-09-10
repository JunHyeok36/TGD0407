using System;

namespace TDG0407._prototype
{
    public interface IEventBus
    {
        void Publish<T>(T eventData);
        IDisposable Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Clear();
    }
}
