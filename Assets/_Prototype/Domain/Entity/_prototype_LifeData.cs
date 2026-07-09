using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_LifeData : _prototype_EntityData
    {
        public List<_prototype_CardData> allCardDatas = new();
        public List<_prototype_CardData> remainedCardDatas = new();
        public List<_prototype_CardData> handedCardDatas = new();
        public List<_prototype_CardData> discardedCardDatas = new();
        public List<_prototype_CardData> destroyedCardDatas = new();

        public _prototype_LifeData() : base() { }
        public _prototype_LifeData(
            string name,
            _prototype_BoundedValue<int> health, 
            _prototype_BoundedValue<int> stamina, 
            _prototype_Point position, 
            IEnumerable<_prototype_CardData> allCardDatas) 
            : base(name, health, stamina, position)
        {
            this.allCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in allCardDatas) this.allCardDatas.Add(new(cardData));
            remainedCardDatas = new List<_prototype_CardData>();
            handedCardDatas = new List<_prototype_CardData>();
            discardedCardDatas = new List<_prototype_CardData>();
            destroyedCardDatas = new List<_prototype_CardData>();
        }
        public _prototype_LifeData(_prototype_LifeData other) 
            : base(other)
        {
            this.allCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.allCardDatas) this.allCardDatas.Add(new(cardData));
            this.remainedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.remainedCardDatas) this.remainedCardDatas.Add(new(cardData));
            this.handedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.handedCardDatas) this.handedCardDatas.Add(new(cardData));
            this.discardedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.discardedCardDatas) this.discardedCardDatas.Add(new(cardData));
            this.destroyedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.destroyedCardDatas) this.destroyedCardDatas.Add(new(cardData));
        }
    }

}
