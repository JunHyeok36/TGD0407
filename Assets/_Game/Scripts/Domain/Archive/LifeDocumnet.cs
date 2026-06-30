using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Domain.Archive
{

    using Domain.Cards;
    using Domain.Entities;
    using Domain.Effects;
    
    [CreateAssetMenu(fileName = "LifeDocument", menuName = "Archive/Entities/LifeDocument")]
    public sealed class LifeDocument : EntityDocument
    {
        #region Fields

        public Stat stat = new();
        public CardDeck deck = null;
        public List<StatusEffect> statusEffects = new();

        #endregion
    }

}