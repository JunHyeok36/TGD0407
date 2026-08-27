using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_LifeData : _prototype_EntityData
    {
        
        public _prototype_CardDeck cardDeck = new();
        [SerializeReference, SubclassSelector] public _prototype_EnemyAILogic aiLogic;

        public _prototype_LifeData() : base() { }
        public _prototype_LifeData(
            string name,
            _prototype_BoundedValue<int> health, 
            _prototype_BoundedValue<int> stamina, 
            _prototype_Point point, 
            _prototype_CardDeck cardDeck) 
            : base(name, health, stamina, point)
        {
            this.cardDeck = cardDeck;
        }
        public _prototype_LifeData(_prototype_LifeData other) 
            : base(other)
        {
            this.cardDeck = new _prototype_CardDeck(other.cardDeck);
        }
    }

}
