using System;
using System.Collections.Generic;

namespace TDG0407.Systems.Data
{

    using Core.Value;
    using Domain.Entities;

    /// <summary>
    /// Resources/Data/life_entities.json 파일을 읽고 초기 생명체 데이터를 반환합니다.
    /// </summary>
    public static class LifeEntityInitialDataLoader
    {
        private const string ResourcePath = "Data/life_entities";

        public static LifeEntityCollection Load()
        {
            LifeEntityCollection collection = JsonInitialDataLoader.Load<LifeEntityCollection>(ResourcePath);
            return collection ?? new LifeEntityCollection();
        }

        public static bool TryGetEntity(string entityId, out LifeEntityDefinition entity)
        {
            return JsonInitialDataLoader.TryGetById(Load().entities, entityId, definition => definition.entityId, out entity);
        }

        public static Stat ToStat(StatDefinition statDefinition)
        {
            return JsonInitialDataLoader.ToStat(statDefinition);
        }

        public static Shield ToShield(ShieldDefinition shieldDefinition)
        {
            return JsonInitialDataLoader.ToShield(shieldDefinition);
        }

        public static List<Shield> ToShields(ShieldDefinition[] shieldDefinitions)
        {
            return JsonInitialDataLoader.ToShields(shieldDefinitions);
        }
    }

    [Serializable]
    public class LifeEntityCollection
    {
        public LifeEntityDefinition[] entities = Array.Empty<LifeEntityDefinition>();
    }

    [Serializable]
    public class LifeEntityDefinition : EntityInitialDataDefinition
    {
        public StatDefinition stat = new();
        public CardDeckDefinition deck = new();
        public string[] statusEffects = Array.Empty<string>();
    }
}