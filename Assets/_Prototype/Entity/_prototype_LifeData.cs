using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_LifeData : _prototype_EntityData
    {
        public List<_prototype_CardData> allCardDatas;
        public List<_prototype_CardData> remainedCardDatas;
        public List<_prototype_CardData> handedCardDatas;
        public List<_prototype_CardData> discardedCardDatas;
        public List<_prototype_CardData> destroyedCardDatas;

        public _prototype_LifeData(
            string name,
            _prototype_BoundedValue<int> health, 
            _prototype_BoundedValue<int> stamina, 
            _prototype_Point position, 
            IEnumerable<_prototype_CardData> allCardDatas) 
            : base(name, health, stamina, position)
        {
            this.allCardDatas = new List<_prototype_CardData>(allCardDatas);
            remainedCardDatas = new List<_prototype_CardData>();
            handedCardDatas = new List<_prototype_CardData>();
            discardedCardDatas = new List<_prototype_CardData>();
            destroyedCardDatas = new List<_prototype_CardData>();
        }

        public _prototype_LifeData(_prototype_LifeData other) 
            : base(other)
        {
            this.allCardDatas = new List<_prototype_CardData>(other.allCardDatas);
            this.remainedCardDatas = new List<_prototype_CardData>(other.remainedCardDatas);
            this.handedCardDatas = new List<_prototype_CardData>(other.handedCardDatas);
            this.discardedCardDatas = new List<_prototype_CardData>(other.discardedCardDatas);
            this.destroyedCardDatas = new List<_prototype_CardData>(other.destroyedCardDatas);
        }
    }

}
