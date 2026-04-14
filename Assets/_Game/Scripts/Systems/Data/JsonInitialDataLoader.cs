using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Systems.Data
{
    using Core.Value;
    using Domain;
    using Domain.Entities;

    /// <summary>
    /// Resources 기반 초기 JSON을 읽고 공통 DTO를 변환하는 도우미입니다.
    /// </summary>
    public static class JsonInitialDataLoader
    {
        public static TCollection Load<TCollection>(string resourcePath) where TCollection : class
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if(jsonAsset == null || string.IsNullOrWhiteSpace(jsonAsset.text))
                return null;

            return JsonUtility.FromJson<TCollection>(jsonAsset.text);
        }

        public static bool TryGetById<TItem>(IEnumerable<TItem> items, string id, Func<TItem, string> idSelector, out TItem item)
        {
            item = default;

            if(items == null || string.IsNullOrWhiteSpace(id) || idSelector == null)
                return false;

            foreach(TItem candidate in items)
            {
                if(candidate != null && idSelector(candidate) == id)
                {
                    item = candidate;
                    return true;
                }
            }

            return false;
        }

        public static Stat ToStat(StatDefinition statDefinition)
        {
            if(statDefinition == null)
                return new Stat();

            return new Stat
            {
                redPower = statDefinition.redPower,
                bluePower = statDefinition.bluePower,
                yellowPower = statDefinition.yellowPower,
                whitePower = statDefinition.whitePower,
                redResist = statDefinition.redResist,
                blueResist = statDefinition.blueResist,
                yellowResist = statDefinition.yellowResist,
                cardSlotCount = statDefinition.cardSlotCount,
                drawQuickness = statDefinition.drawQuickness,
                speed = statDefinition.speed,
                bleedingResist = statDefinition.bleedingResist,
                burningResist = statDefinition.burningResist,
                poisoningResist = statDefinition.poisoningResist,
                stunResist = statDefinition.stunResist,
                freezeResist = statDefinition.freezeResist,
                silenceResist = statDefinition.silenceResist,
                fearResist = statDefinition.fearResist,
                knockbackResist = statDefinition.knockbackResist,
                curseResist = statDefinition.curseResist,
                knockdownResist = statDefinition.knockdownResist,
                healthRecoveryAmount = statDefinition.healthRecoveryAmount,
                staminaRecoveryAmount = statDefinition.staminaRecoveryAmount,
                dodgeProb = statDefinition.dodgeProb,
                deathResist = statDefinition.deathResist,
                criticalProb = statDefinition.criticalProb,
                criticalWeight = statDefinition.criticalWeight,
            };
        }

        public static Shield ToShield(ShieldDefinition shieldDefinition)
        {
            if(shieldDefinition == null)
                return null;

            TickDurationType tickDurationType = TickDurationType.Forever;
            if(string.IsNullOrWhiteSpace(shieldDefinition.duration?.type) == false)
                Enum.TryParse(shieldDefinition.duration.type, true, out tickDurationType);

            int durationTicks = shieldDefinition.duration?.value?.current ?? -1;
            return new Shield(shieldDefinition.durability, tickDurationType, durationTicks, shieldDefinition.performedEntityInstanceId);
        }

        public static List<Shield> ToShields(ShieldDefinition[] shieldDefinitions)
        {
            List<Shield> shields = new();
            if(shieldDefinitions == null)
                return shields;

            foreach(ShieldDefinition shieldDefinition in shieldDefinitions)
            {
                Shield shield = ToShield(shieldDefinition);
                if(shield != null)
                    shields.Add(shield);
            }

            return shields;
        }
    }

    [Serializable]
    public abstract class EntityInitialDataDefinition
    {
        public string entityId = null;
        public string entityType = null;
        public VitalDefinition health = new();
        public VitalDefinition stamina = new();
        public ShieldDefinition[] shields = Array.Empty<ShieldDefinition>();
    }

    [Serializable]
    public class VitalDefinition
    {
        public int max = 0;
        public int current = 0;
    }

    [Serializable]
    public class ShieldDefinition
    {
        public int durability = 0;
        public TickDurationDefinition duration = new();
        public int performedEntityInstanceId = -1;
    }

    [Serializable]
    public class TickDurationDefinition
    {
        public string type = TickDurationType.Forever.ToString();
        public VitalDefinition value = new();
    }

    [Serializable]
    public class StatDefinition
    {
        public int redPower = 1;
        public int bluePower = 0;
        public int yellowPower = 0;
        public int whitePower = 0;

        public int redResist = 0;
        public int blueResist = 0;
        public int yellowResist = 0;

        public int cardSlotCount = 4;
        public float drawQuickness = 1.0f;
        public int speed = 1;

        public int bleedingResist = 1;
        public int burningResist = 1;
        public int poisoningResist = 1;
        public int stunResist = 1;
        public int freezeResist = 1;
        public int silenceResist = 1;
        public int fearResist = 1;
        public int knockbackResist = 1;
        public int curseResist = 1;
        public int knockdownResist = 1;

        public float healthRecoveryAmount = 1.0f;
        public float staminaRecoveryAmount = 1.0f;
        public float dodgeProb = 0.01f;
        public int deathResist = 10;
        public float criticalProb = 0.05f;
        public float criticalWeight = 2.0f;
    }

    [Serializable]
    public class CardDeckDefinition
    {
        public string[] allCards = Array.Empty<string>();
    }
}