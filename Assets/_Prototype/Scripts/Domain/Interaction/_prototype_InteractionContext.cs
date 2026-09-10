using System;

namespace TDG0407._prototype
{
    public sealed class _prototype_InteractionContext
    {
        public _prototype_EntityData Actor { get; }
        public _prototype_InteractableData Target { get; }
        public _prototype_Point Point => Target != null ? Target.point : _prototype_Point.zero;
        public _prototype_PlayMode PlayMode { get; }

        public _prototype_InteractionContext(
            _prototype_EntityData actor,
            _prototype_InteractableData target,
            _prototype_PlayMode playMode)
        {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            PlayMode = playMode;
        }
    }
}