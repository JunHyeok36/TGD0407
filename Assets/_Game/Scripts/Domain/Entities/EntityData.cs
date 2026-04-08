using System;

namespace TDG0407.Domain.Entities
{
    
    using Core.Value;
    using Core.Grid;
    using Domain.Cards;
    using Domain.Exceptions;
    using Domain.Interfaces;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class EntityData : IDataValidatable
    {
        #region Fields

        public string id = null;
        public Point pos = Point.zero;
        public BoundedValue<int> health = new(0, 50);
        public BoundedValue<int> stamina = new(0, 10);
        public CardDeck deck = new();

        

        #endregion
        #region Constructors

        

        #endregion
        #region Methods

        public void ValidateData()
        {
            
        }

        #endregion
        
    }

}