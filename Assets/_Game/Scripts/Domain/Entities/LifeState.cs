using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Entities
{
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

        public LifeState() : base()
        {
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