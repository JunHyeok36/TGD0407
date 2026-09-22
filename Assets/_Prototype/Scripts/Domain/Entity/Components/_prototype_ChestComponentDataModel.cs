using UnityEngine;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [System.Serializable]
    public class _prototype_ChestComponentDataModel : _prototype_EntityComponentDataModel
    {
        [Tooltip("상자를 열었을 때 획득할 카드 목록 (BattleCard 또는 InteractionCard)")]
        public List<_prototype_CardDataModel> storedCards = new();

        [Tooltip("상자를 열었을 때 획득할 아이템 목록 (선택 사항)")]
        public List<_prototype_ItemDataModel> storedItems = new();

        [Tooltip("상자 시작 시 이미 열려있는지 여부")]
        public bool isOpened = false;

        public override _prototype_EntityComponentData CreateComponentData()
        {
            var component = new _prototype_ChestComponentData();
            component.isOpened = this.isOpened;
            if (storedCards != null)
            {
                foreach (var cardModel in storedCards)
                {
                    if (cardModel != null)
                    {
                        component.storedCards.Add(cardModel.CreateCardData());
                    }
                }
            }
            if (storedItems != null)
            {
                foreach (var itemModel in storedItems)
                {
                    if (itemModel != null)
                    {
                        component.storedItems.Add(itemModel);
                    }
                }
            }
            return component;
        }
    }
}
