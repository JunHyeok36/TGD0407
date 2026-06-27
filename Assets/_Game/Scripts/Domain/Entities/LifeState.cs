using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Entities
{

    using Core.Grid;
    using Domain.Cards;
    using Domain.Effects;
    using TDG0407.Core.Value;

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

        public LifeState(
            string entityId,
            int entityInstanceId,
            Point pos,
            int health,
            int stamina,
            Stat stat,
            CardDeck deck,
            IEnumerable<StatusEffect> statusEffects = null,
            IEnumerable<Shield> shields = null) 
            : base(entityId, entityInstanceId, pos, health, stamina, shields)
        {
            this.stat = stat;
            this.deck = deck;
            this.statusEffects.AddRange(statusEffects ?? new List<StatusEffect>());
        }

        #endregion
        #region Methods

        public override void Initialize()
        {
            base.Initialize();
        }

        #endregion
    }
}