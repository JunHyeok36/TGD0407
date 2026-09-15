using System;

namespace TDG0407._prototype
{
    public sealed class _prototype_ConditionContext
    {
        public _prototype_EntityData Actor { get; }
        public _prototype_EntityData Target { get; }
        public _prototype_Point Point => Target != null ? Target.point : _prototype_Point.zero;
        public _prototype_PlayMode PlayMode { get; }

        public _prototype_ConditionContext(
            _prototype_EntityData actor,
            _prototype_EntityData target,
            _prototype_PlayMode playMode)
        {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Target = target;
            PlayMode = playMode;
        }
    }
}
