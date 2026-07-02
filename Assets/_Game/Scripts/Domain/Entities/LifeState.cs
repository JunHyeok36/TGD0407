using System;
using System.Collections.Generic;
using System.Linq;

namespace TDG0407.Domain.Entities
{

    using Core.Grid;
    using Core.Value;
    using Domain.Cards;
    using Domain.Effects;

    /// <summary>
    /// 카드를 사용해 전투 행동을 수행하는 생명체의 상태입니다.
    /// </summary>
    [Serializable]
    public class LifeState : EntityState
    {
        #region Fields

        public readonly Stat stat = new();
        public readonly CardDeck deck = null;
        public readonly List<StatusEffect> statusEffects = new();

        #endregion
        #region Constructors

        public LifeState() : base() {}
        public LifeState(
            int entityInstanceId,
            string entityId,
            Point position,
            BoundedValue<int> health,
            BoundedValue<int> stamina,
            Stat stat,
            CardDeck deck,
            IEnumerable<StatusEffect> statusEffects = null,
            IEnumerable<Shield> shields = null) 
            : base(entityInstanceId, entityId, position, health, stamina, shields)
        {
            this.stat = stat;
            this.deck = deck;
            this.statusEffects.AddRange(statusEffects ?? new List<StatusEffect>());
        }

        public LifeState(LifeState other) 
            : base(
                other.entityInstanceId ?? throw new InvalidOperationException("EntityInstanceId is null."), 
                other.entityId, 
                other.position, 
                other.health, 
                other.stamina, 
                other.shields)
        {
            this.stat = other.stat.Clone();
            this.deck = other.deck.Clone();
            foreach (var statusEffect in other.statusEffects)
                this.statusEffects.Add(statusEffect.Clone());
        }

        #endregion
        #region Methods

        public override void Initialize()
        {
            base.Initialize();
        }

        public override EntityState Clone()
        {
            LifeState clone = new(
                entityId: this.entityId,
                entityInstanceId: this.entityInstanceId ?? throw new InvalidOperationException("EntityInstanceId is null."),
                position: this.position,
                health: this.health,
                stamina: this.stamina,
                stat: this.stat.Clone(),
                deck: this.deck.Clone(),
                statusEffects: this.statusEffects.Select(se => se.Clone()).ToList(),
                shields: this.shields.Select(s => s.Clone()).ToList()
            );
            return clone;
        }

        #endregion
    }
}