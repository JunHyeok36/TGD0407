using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_ChestComponentData : _prototype_EntityComponentData, _prototype_IInteractable
    {
        public List<_prototype_CardData> storedCards = new();
        public List<_prototype_ItemDataModel> storedItems = new();
        public bool isOpened = false;

        [NonSerialized]
        private _prototype_EntityView _owner;
        public _prototype_EntityView Owner => _owner;

        public override void Initialize(_prototype_EntityView owner)
        {
            _owner = owner;
        }

        public async UniTask Interact(_prototype_EntityData caster, string interactionKey)
        {
            if (!string.Equals(interactionKey, "OpenChest", StringComparison.OrdinalIgnoreCase))
                return;

            if (isOpened) return;
            isOpened = true;

            var targetView = _owner;
            if (targetView == null && _prototype_GridManager.Instance != null)
            {
                foreach (var ev in _prototype_GridManager.Instance.GetAllEntityViews())
                {
                    if (ev != null && ev.EntityData?.components != null && ev.EntityData.components.Contains(this))
                    {
                        targetView = ev;
                        _owner = ev;
                        break;
                    }
                }
            }

            // 1. 상자 연출 (살짝 튀어오르는 바운스 트윈)
            if (targetView != null)
            {
                targetView.transform.DOPunchScale(new Vector3(0.2f, 0.35f, 0.2f), 0.4f, 8, 0.5f);
                _prototype_FloatingText.SpawnOnEntity(targetView, "상자 개봉!", new Color(1f, 0.85f, 0.2f));
            }

            // 2. 카드 및 아이템 지급
            if (caster is _prototype_LifeData lifeCaster && lifeCaster.cardDeck != null)
            {
                if (storedCards != null && storedCards.Count > 0)
                {
                    foreach (var card in storedCards)
                    {
                        if (card == null) continue;
                        var clonedCard = card.Clone();
                        lifeCaster.cardDeck.allCardDatas.Add(clonedCard);
                        lifeCaster.cardDeck.remainedCardDatas.Add(clonedCard);

                        // 이벤트 발행
                        _prototype_EventBus.Fire(new EntityCardAcquiredEvent(caster, clonedCard, targetView != null ? targetView.name : "상자"));

                        // UI 모달 팝업
                        if (_prototype_PlayerUIView.Instance != null)
                        {
                            _prototype_PlayerUIView.Instance.ShowCardAcquiredModal(clonedCard, targetView != null ? targetView.name : "상자");
                        }
                    }
                }

                if (storedItems != null && storedItems.Count > 0)
                {
                    foreach (var item in storedItems)
                    {
                        if (item == null) continue;
                        lifeCaster.AddItem(item, 1);
                        _prototype_EventBus.Fire(new EntityItemAcquiredEvent(caster, item, 1, targetView != null ? targetView.name : "상자"));

                        if (_prototype_PlayerUIView.Instance != null && (storedCards == null || storedCards.Count == 0))
                        {
                            _prototype_PlayerUIView.Instance.ShowItemAcquiredModal(item, 1, targetView != null ? targetView.name : "상자");
                        }
                    }
                }
            }

            // 3. 덱 UI 갱신 (상자 열기 카드가 손패에서 즉시 갱신/제거되도록)
            if (_prototype_PlayerUIView.Instance != null)
            {
                _prototype_PlayerUIView.Instance.UpdatePlayerCardDeck();
            }

            await UniTask.Yield();
        }

        public override _prototype_EntityComponentData Clone()
        {
            var clone = new _prototype_ChestComponentData();
            clone.isOpened = this.isOpened;
            if (storedCards != null)
            {
                foreach (var c in storedCards)
                {
                    if (c != null) clone.storedCards.Add(c.Clone());
                }
            }
            if (storedItems != null)
            {
                foreach (var item in storedItems)
                {
                    if (item != null) clone.storedItems.Add(item);
                }
            }
            return clone;
        }
    }
}
