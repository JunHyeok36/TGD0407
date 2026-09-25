using System;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 선택 패시브: 잔영 (Afterimage)
    /// - Dash 유형의 카드를 사용할 때마다 핸드에 있는 모든 카드의 쿨타임을 1틱씩 감소
    /// </summary>
    [Serializable]
    public class _prototype_AfterimagPassiveData : _prototype_LifePassiveData
    {
        public override string PassiveId => "passive_afterimage";

        static _prototype_AfterimagPassiveData()
            => _prototype_LifePassiveRegistry.Register("passive_afterimage", () => new _prototype_AfterimagPassiveData());

        public override void OnCardUsed(_prototype_LifeData owner, _prototype_BattleCardData card)
        {
            // Skill 유형 카드 중 이름/ID에 "dash"/"windwalk"/"phantom"/"flank"/"rush"/"shadowrush" 포함
            // 또는 cardType 기반 판별: life_ren.md에서 Dash 유형은 Attack이 아닌 별도 타입이지만
            // 현재 CardType enum에 Dash가 없으므로 카드 ID prefix로 판별
            if (!IsDashCard(card)) return;

            // 핸드의 모든 카드 쿨타임 -1
            if (owner.cardDeck == null) return;
            int count = ReduceHandCooldowns(owner.cardDeck);

            if (count > 0)
                SpawnFloating(owner, $"잔영: 쿨타임 -{count}장!", new Color(0.8f, 0.7f, 1f));
        }

        /// <summary>카드가 Dash 유형인지 판별합니다 (CardType 및 ID 폴백).</summary>
        private bool IsDashCard(_prototype_BattleCardData card)
        {
            if (card == null) return false;
            if (card.cardType == _prototype_CardType.Dash) return true;
            string id = card.id?.ToLowerInvariant() ?? "";
            return id.Contains("windwalk") || id.Contains("phantom") || id.Contains("flank")
                || id.Contains("rush") || id.Contains("shadowrush") || id.Contains("dash");
        }

        /// <summary>핸드에 있는 모든 카드의 쿨타임을 1씩 감소. 감소된 카드 수 반환.</summary>
        private int ReduceHandCooldowns(_prototype_CardDeck deck)
        {
            int count = 0;
            if (deck?.handedCardDatas == null) return count;
            foreach (var c in deck.handedCardDatas)
            {
                if (c is _prototype_BattleCardData bc)
                {
                    bool reduced = false;
                    if (bc.currentCoolTicks > 0)
                    {
                        bc.currentCoolTicks = Mathf.Max(0, bc.currentCoolTicks - 1);
                        reduced = true;
                    }
                    if (bc.coolTicks != null && bc.coolTicks.Current > 0)
                    {
                        bc.coolTicks.Current = (byte)Mathf.Max(0, bc.coolTicks.Current - 1);
                        reduced = true;
                    }
                    if (reduced) count++;
                }
            }
            return count;
        }

        private void SpawnFloating(_prototype_LifeData owner, string text, Color color)
        {
            if (_prototype_GridManager.Instance == null) return;
            var pv = _prototype_GridManager.Instance.GetPointView(owner.point);
            if (pv == null) return;
            var ev = pv.PlacedEntityViews.FirstOrDefault(v => v?.EntityData == owner);
            if (ev != null) _prototype_FloatingText.SpawnOnEntity(ev, text, color);
        }

        public override _prototype_LifePassiveData Clone() => new _prototype_AfterimagPassiveData();
    }
}
