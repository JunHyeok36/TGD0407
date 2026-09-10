namespace TDG0407._prototype
{
    public struct EntityDamagedEvent
    {
        public _prototype_EntityData Target { get; }
        public _prototype_EntityData Source { get; }
        public int Damage { get; }
        public _prototype_DamageType DamageType { get; }
        public _prototype_Point Point { get; }
        public _prototype_DamageContext Context { get; }
        public bool IsCritical => Context != null && Context.isCritical;

        public EntityDamagedEvent(
            _prototype_EntityData target,
            _prototype_EntityData source,
            int damage,
            _prototype_DamageType damageType,
            _prototype_Point point,
            _prototype_DamageContext context)
        {
            Target = target;
            Source = source;
            Damage = damage;
            DamageType = damageType;
            Point = point;
            Context = context;
        }
    }

    public struct EntityDiedEvent
    {
        public _prototype_EntityData Victim { get; }
        public _prototype_EntityData Killer { get; }

        public EntityDiedEvent(_prototype_EntityData victim, _prototype_EntityData killer)
        {
            Victim = victim;
            Killer = killer;
        }
    }

    public struct EntityMovedEvent
    {
        public _prototype_EntityData Entity { get; }
        public _prototype_Point From { get; }
        public _prototype_Point To { get; }

        public EntityMovedEvent(_prototype_EntityData entity, _prototype_Point from, _prototype_Point to)
        {
            Entity = entity;
            From = from;
            To = to;
        }
    }

    public struct EntityInteractedEvent
    {
        public _prototype_EntityData Source { get; }
        public _prototype_InteractableData Target { get; }

        public EntityInteractedEvent(_prototype_EntityData source, _prototype_InteractableData target)
        {
            Source = source;
            Target = target;
        }
    }

    public struct EntityStatusChangedEvent
    {
        public _prototype_EntityData Target { get; }
        public _prototype_StatusEffect Effect { get; }
        public bool IsAdded { get; }

        public EntityStatusChangedEvent(_prototype_EntityData target, _prototype_StatusEffect effect, bool isAdded)
        {
            Target = target;
            Effect = effect;
            IsAdded = isAdded;
        }
    }

    public struct TickAdvancedEvent
    {
        public int TickNumber { get; }

        public TickAdvancedEvent(int tickNumber)
        {
            TickNumber = tickNumber;
        }
    }

    /// <summary>
    /// 플레이 모드가 변경될 때 발행되는 이벤트입니다.
    /// </summary>
    public struct PlayModeChangedEvent
    {
        public _prototype_PlayMode PreviousMode { get; }
        public _prototype_PlayMode NewMode { get; }

        public PlayModeChangedEvent(_prototype_PlayMode previousMode, _prototype_PlayMode newMode)
        {
            PreviousMode = previousMode;
            NewMode = newMode;
        }
    }
}
