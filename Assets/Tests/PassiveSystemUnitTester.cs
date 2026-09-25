using System;
using TDG0407._prototype;
using UnityEngine;

namespace TDG0407.Tests
{
    public static class PassiveSystemUnitTester
    {
        public static void RunTests()
        {
            Debug.Log("[PassiveTest] Testing Passive Registry...");
            // 1. Force static constructors to run
            var resolute = new _prototype_ResolutePassiveData();
            var afterimage = new _prototype_AfterimagPassiveData();
            var bloodDemon = new _prototype_BloodDemonPassiveData();
            var torrent = new _prototype_TorrentGaugePassiveData();
            var formless = new _prototype_FormlessBladePassiveData();
            var immortal = new _prototype_ImmortalPassiveData();

            Assert(_prototype_LifePassiveRegistry.IsRegistered("passive_resolute"), "passive_resolute registered");
            Assert(_prototype_LifePassiveRegistry.IsRegistered("passive_afterimage"), "passive_afterimage registered");
            Assert(_prototype_LifePassiveRegistry.IsRegistered("passive_blood_demon"), "passive_blood_demon registered");
            Assert(_prototype_LifePassiveRegistry.IsRegistered("passive_torrent_gauge"), "passive_torrent_gauge registered");
            Assert(_prototype_LifePassiveRegistry.IsRegistered("passive_formless_blade"), "passive_formless_blade registered");
            Assert(_prototype_LifePassiveRegistry.IsRegistered("passive_immortal"), "passive_immortal registered");

            Debug.Log("[PassiveTest] Testing LifeData AddPassive / HasPassive...");
            var lifeData = new _prototype_LifeData();
            bool added = lifeData.AddPassive("passive_resolute");
            Assert(added, "Add passive_resolute returns true");
            Assert(lifeData.HasPassive("passive_resolute"), "HasPassive passive_resolute true");
            Assert(lifeData.Passives.Count == 1, "Passives count == 1");

            // Duplicate add test
            bool addedAgain = lifeData.AddPassive("passive_resolute");
            Assert(!addedAgain, "Duplicate AddPassive returns false");
            Assert(lifeData.Passives.Count == 1, "Passives count still 1 after duplicate add");

            // Resolute on kill test (20 stamina)
            lifeData.stamina = new _prototype_BoundedValue<int>(0, 100, 50);
            lifeData.FirePassiveOnKill(new _prototype_LifeData());
            Assert(lifeData.stamina.Current == 70, $"Resolute recovered 20 stamina on kill (expected 70, got {lifeData.stamina.Current})");

            Debug.Log("[PassiveTest] Testing Unique FlowGauge & Card Cost Override...");
            var flowPassive = new _prototype_FlowGaugePassiveData();
            lifeData.uniquePassive = flowPassive;

            var attackCard = new _prototype_BattleCardData();
            attackCard.cardType = _prototype_CardType.Attack;
            attackCard.costValue = new _prototype_CostValue(_prototype_CostType.FixedStamina, 20);

            // Biting wind not active -> cost override should be -1
            int override1 = lifeData.QueryPassiveCostOverride(attackCard);
            Assert(override1 == -1, "Cost override is -1 when biting wind is inactive");

            // Deal damage 3 times to trigger Flow Gauge -> Biting Wind
            var dummyTarget = new _prototype_LifeData();
            dummyTarget.side = _prototype_Side.B;
            lifeData.side = _prototype_Side.A;

            for (int i = 0; i < 3; i++)
            {
                var ctx = new _prototype_DamageContext(lifeData, dummyTarget, _prototype_DamageType.Physical, 10, 10);
                ctx.finalDamage = 10;
                lifeData.FirePassiveOnDamageDealt(ctx);
            }

            Assert(flowPassive.BitingWindActive, "Biting Wind is active after 3 hits");

            // Biting wind active -> Attack card cost override should be 0
            int override2 = lifeData.QueryPassiveCostOverride(attackCard);
            Assert(override2 == 0, "Cost override is 0 when biting wind is active for Attack card");

            // Non-attack card should NOT get 0 cost
            var skillCard = new _prototype_BattleCardData();
            skillCard.cardType = _prototype_CardType.Skill;
            skillCard.costValue = new _prototype_CostValue(_prototype_CostType.FixedStamina, 15);
            int override3 = lifeData.QueryPassiveCostOverride(skillCard);
            Assert(override3 == -1, "Skill card does not get cost override from biting wind");

            // Consuming biting wind via OnCardUsed
            lifeData.FirePassiveOnCardUsed(attackCard);
            Assert(!flowPassive.BitingWindActive, "Biting Wind consumed after OnCardUsed");

            Debug.Log("[PassiveTest] Testing Ren Range Selectors & Actions...");
            // 1. AroundCircleCastSelector
            var circleSelector = new _prototype_AroundCircleCastSelector(3, false);
            var circlePoints = circleSelector.GetValidCastPoints(new _prototype_Point(0, 0));
            Assert(circlePoints.Count > 0, "AroundCircleCastSelector returned points");
            Assert(!circlePoints.Contains(new _prototype_Point(0, 0)), "AroundCircleCastSelector excludeSelf works");
            Assert(circlePoints.Contains(new _prototype_Point(0, 3)), "AroundCircleCastSelector contains (0,3)");
            Assert(circlePoints.Contains(new _prototype_Point(2, 2)), "AroundCircleCastSelector contains (2,2) since 4+4=8<=9");

            // 2. FrontBoxTargetSelector
            var frontBox1x2 = new _prototype_FrontBoxTargetSelector(1, 2);
            var frontPoints = frontBox1x2.GetValidTargetPoints(new _prototype_Point(0, 0), new _prototype_Point(1, 0));
            Assert(frontPoints.Count == 2, "FrontBox 1x2 returns exactly 2 points");
            Assert(frontPoints.Contains(new _prototype_Point(1, 0)), "FrontBox 1x2 contains (1,0)");
            Assert(frontPoints.Contains(new _prototype_Point(2, 0)), "FrontBox 1x2 contains (2,0)");

            var frontBox3x3 = new _prototype_FrontBoxTargetSelector(3, 3);
            var frontPoints3x3 = frontBox3x3.GetValidTargetPoints(new _prototype_Point(0, 0), new _prototype_Point(0, 1));
            Assert(frontPoints3x3.Count == 9, "FrontBox 3x3 returns exactly 9 points");

            // 3. StatusType.EnhanceStab and RemoveStatusEffect
            lifeData.ApplyStatusEffect(new _prototype_StatusEffect(_prototype_StatusType.EnhanceStab, 5));
            Assert(lifeData.HasStatusEffect(_prototype_StatusType.EnhanceStab), "HasStatusEffect EnhanceStab true");
            bool removed = lifeData.RemoveStatusEffect(_prototype_StatusType.EnhanceStab);
            Assert(removed, "RemoveStatusEffect returns true");
            Assert(!lifeData.HasStatusEffect(_prototype_StatusType.EnhanceStab), "HasStatusEffect EnhanceStab false after removal");

            // 4. Breathe action (Draw + Flow stack)
            var breatheAction = new _prototype_BreatheEntityAction();
            flowPassive.ResetFlowGauge();
            lifeData.uniquePassive = flowPassive;
            breatheAction.ExecuteAction(lifeData, null, null).GetAwaiter().GetResult();
            Assert(flowPassive.FlowStack == 1, "Breathe action added 1 flow stack to FlowGaugePassiveData");

            // 5. Dash enum & Afterimage Passive with Dash card
            Debug.Log("[PassiveTest] Testing Dash Card Type & Afterimage Passive...");
            var dashCard = new _prototype_BattleCardData();
            dashCard.cardType = _prototype_CardType.Dash;
            Assert(dashCard.cardType == _prototype_CardType.Dash, "CardType.Dash is properly set");

            var afterimageLife = new _prototype_LifeData();
            afterimageLife.AddPassive("passive_afterimage");
            var coolCard = new _prototype_BattleCardData();
            coolCard.coolTicks = new _prototype_BoundedValue<byte>(0, 5, 3);
            coolCard.currentCoolTicks = 3;
            afterimageLife.cardDeck.handedCardDatas.Add(coolCard);
            afterimageLife.FirePassiveOnCardUsed(dashCard);
            Assert(coolCard.coolTicks.Current == 2 && coolCard.currentCoolTicks == 2, "Afterimage reduced coolTicks from 3 to 2 on Dash card usage");

            // 6. AroundLineCastSelector (range 2 with diagonals: 8 directions * 2 = 16 points)
            Debug.Log("[PassiveTest] Testing AroundLineCastSelector (8 directions)...");
            var lineSelector = new _prototype_AroundLineCastSelector(2, true);
            var linePoints = lineSelector.GetValidCastPoints(new _prototype_Point(0, 0));
            Assert(linePoints.Count == 16, $"AroundLineCastSelector returned {linePoints.Count} points (expected 16)");
            Assert(linePoints.Contains(new _prototype_Point(2, 0)), "AroundLine contains (2, 0) - Right 2");
            Assert(linePoints.Contains(new _prototype_Point(-2, 0)), "AroundLine contains (-2, 0) - Left 2");
            Assert(linePoints.Contains(new _prototype_Point(0, 2)), "AroundLine contains (0, 2) - Up 2");
            Assert(linePoints.Contains(new _prototype_Point(0, -2)), "AroundLine contains (0, -2) - Down 2");
            Assert(linePoints.Contains(new _prototype_Point(2, 2)), "AroundLine contains (2, 2) - Diagonal UpRight 2");
            Assert(linePoints.Contains(new _prototype_Point(-2, -2)), "AroundLine contains (-2, -2) - Diagonal DownLeft 2");

            // 7. Windwalk action (Movement + Flow Stack)
            Debug.Log("[PassiveTest] Testing WindwalkEntityAction...");
            var windwalkAction = new _prototype_WindwalkEntityAction();
            flowPassive.ResetFlowGauge();
            lifeData.uniquePassive = flowPassive;
            windwalkAction.ExecuteAction(lifeData, null, null).GetAwaiter().GetResult();
            Assert(flowPassive.FlowStack == 1, "WindwalkEntityAction granted 1 Flow Stack");

            // 8. Windwall action (Shield + Unstoppable)
            Debug.Log("[PassiveTest] Testing WindwallEntityAction...");
            var windwallAction = new _prototype_WindwallEntityAction();
            lifeData.health = new _prototype_BoundedValue<int>(0, 100, 100);
            windwallAction.ExecuteAction(lifeData, null, null).GetAwaiter().GetResult();
            // Expected shield: 10 + 100 * 0.04 = 14
            Assert(lifeData.CurrentShield >= 14, $"WindwallEntityAction granted shield >= 14 (actual: {lifeData.CurrentShield})");
            Assert(lifeData.HasStatusEffect(_prototype_StatusType.Unstoppable), "WindwallEntityAction granted Unstoppable status effect");

            // 9. Stab action (Instant EnhanceStab buff consumption & projectile damage)
            Debug.Log("[PassiveTest] Testing StabAttackEntityAction buff consumption and projectile destruction...");
            var stabAction = new _prototype_StabAttackEntityAction();
            lifeData.ApplyStatusEffect(new _prototype_StatusEffect(_prototype_StatusType.EnhanceStab, 5));
            Assert(lifeData.HasStatusEffect(_prototype_StatusType.EnhanceStab), "Precondition: EnhanceStab active");

            var projData = new _prototype_ProjectileData();
            projData.health = new _prototype_BoundedValue<int>(0, 10, 10);
            projData.side = _prototype_Side.B;

            stabAction.ExecuteAction(lifeData, new[] { projData }, null).GetAwaiter().GetResult();
            Assert(!lifeData.HasStatusEffect(_prototype_StatusType.EnhanceStab), "EnhanceStab consumed immediately on stab cast");
            Assert(projData.health.Current <= 0, "Projectile health reduced to 0 by stab attack");

            Debug.Log("[PassiveTest] Testing Shield System & SP Damage Immunity...");
            var shieldTester = new _prototype_LifeData();
            shieldTester.health = new _prototype_BoundedValue<int>(0, 80);
            shieldTester.stamina = new _prototype_BoundedValue<int>(0, 100);
            shieldTester.lifeStat.poise = 0;

            // 1) 보호막 30 부여
            shieldTester.AddShield(30, 3);
            Assert(shieldTester.CurrentShield == 30, "CurrentShield is 30 after AddShield(30, 3)");

            // 2) 데미지 20 피격 -> 보호막만 20 깎이고 HP 피해 0 -> SP 피해도 0 (면제)
            var ctx1 = new _prototype_DamageContext(null, shieldTester, _prototype_DamageType.Physical, 20, 20, spDamageMultiplier: 1.0f);
            shieldTester.TakeDamage(ctx1).GetAwaiter().GetResult();

            Assert(shieldTester.CurrentShield == 10, "Shield absorbed 20 damage, 10 remains");
            Assert(shieldTester.health.Current == 80, "HP remains intact at 80/80");
            Assert(shieldTester.stamina.Current == 100, "SP damage WAIVED (stamina remains at 100/100) when shield absorbed all damage!");

            // 3) 추가 데미지 25 피격 -> 보호막 10 모두 소진되고 관통 피해 15가 HP와 SP에 적용
            var ctx2 = new _prototype_DamageContext(null, shieldTester, _prototype_DamageType.Physical, 25, 25, spDamageMultiplier: 1.0f);
            shieldTester.TakeDamage(ctx2).GetAwaiter().GetResult();

            Assert(shieldTester.CurrentShield == 0, "Shield fully consumed");
            Assert(shieldTester.health.Current == 65, "Penetrating damage (15) reduced HP to 65");
            Assert(shieldTester.stamina.Current == 85, "Penetrating damage (15) dealt 15 SP damage (stamina reduced to 85)");

            // 4) 틱 만료 테스트
            shieldTester.AddShield(50, 1);
            Assert(shieldTester.CurrentShield == 50, "Added 50 shield with 1 tick duration");
            shieldTester.TickShields();
            Assert(shieldTester.CurrentShield == 0, "Shield expired after 1 tick");

            Debug.Log("[PassiveTest] ALL REN BASIC DECK & PASSIVE SYSTEM TESTS PASSED SUCCESSFULLY!");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Debug.LogError($"[PassiveTest] ASSERTION FAILED: {message}");
                throw new Exception($"Assertion failed: {message}");
            }
            else
            {
                Debug.Log($"[PassiveTest] PASS: {message}");
            }
        }
    }
}
