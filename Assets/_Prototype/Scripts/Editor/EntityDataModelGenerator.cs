using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TDG0407._prototype.Editor
{
    public static class EntityDataModelGenerator
    {
        [MenuItem("_Prototype/Generate Entity and Deck Models")]
        public static void GenerateEntitiesAndDecks()
        {
            string deckFolderPath = "Assets/_Prototype/DataModel/DeckDataModel";
            string entityFolderPath = "Assets/_Prototype/DataModel/EntityDataModel";

            CreateFolderIfNotExists(deckFolderPath);
            CreateFolderIfNotExists(entityFolderPath);

            // 1. Load available cards
            string[] cardGuids = AssetDatabase.FindAssets("t:_prototype_CardDataModel", new[] { "Assets/_Prototype/DataModel" });
            List<_prototype_CardDataModel> allCards = new List<_prototype_CardDataModel>();
            foreach (string guid in cardGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                _prototype_CardDataModel card = AssetDatabase.LoadAssetAtPath<_prototype_CardDataModel>(path);
                if (card != null) allCards.Add(card);
            }

            if (allCards.Count == 0)
            {
                Debug.LogWarning("No cards found! Please run 'Generate Test Cards' first.");
                return;
            }

            // 2. Create Decks
            var basicCard = allCards.FirstOrDefault(c => c.id == "BasicAttack");
            var splashCard = allCards.FirstOrDefault(c => c.id == "SplashAttack");
            var bloodCard = allCards.FirstOrDefault(c => c.id == "BloodStrike");
            var snipeCard = allCards.FirstOrDefault(c => c.id == "Snipe");
            var thrustCard = allCards.FirstOrDefault(c => c.id == "PiercingThrust");
            var novaCard = allCards.FirstOrDefault(c => c.id == "HolyNova");
            var quakeCard = allCards.FirstOrDefault(c => c.id == "Earthquake");

            _prototype_CardDeckModel playerDeck = CreateDeck("PlayerDeck", new List<_prototype_CardDataModel> 
            { 
                basicCard, basicCard, basicCard, 
                splashCard, splashCard, 
                bloodCard, snipeCard, thrustCard, novaCard, quakeCard 
            });
            
            _prototype_CardDeckModel enemyDeck = CreateDeck("EnemyDeck", new List<_prototype_CardDataModel> { basicCard, basicCard });

            // 3. Create Entities
            CreateLifeData("Player", 100, 10, playerDeck, null);
            CreateLifeData("Slime", 30, 5, enemyDeck, new _prototype_MeleeChaseAI { attackDamage = 5 });
            CreateLifeData("Goblin", 50, 8, enemyDeck, new _prototype_MeleeChaseAI { attackDamage = 15 });

            // 4. Create Obstacles
            CreateObstacleData("WoodenBox", 20);
            CreateObstacleData("Rock", 200);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Entities and Decks generated successfully in {entityFolderPath} and {deckFolderPath}");
        }

        private static void CreateFolderIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = path.Substring(0, path.LastIndexOf('/'));
                string newFolder = path.Substring(path.LastIndexOf('/') + 1);
                AssetDatabase.CreateFolder(parent, newFolder);
            }
        }

        private static _prototype_CardDeckModel CreateDeck(string name, List<_prototype_CardDataModel> cards)
        {
            _prototype_CardDeckModel deck = ScriptableObject.CreateInstance<_prototype_CardDeckModel>();
            deck.allCardDatas = cards.Where(c => c != null).ToList();
            
            string path = $"Assets/_Prototype/DataModel/DeckDataModel/{name}.asset";
            AssetDatabase.CreateAsset(deck, path);
            return deck;
        }

        private static _prototype_LifeDataModel CreateLifeData(string ename, int maxHealth, int maxStamina, _prototype_CardDeckModel deck, _prototype_EnemyAILogic aiLogic)
        {
            _prototype_LifeDataModel life = ScriptableObject.CreateInstance<_prototype_LifeDataModel>();
            life.ename = ename;
            life.health = new _prototype_BoundedValue<int>(0, maxHealth, maxHealth);
            life.stamina = new _prototype_BoundedValue<int>(0, maxStamina, maxStamina);
            life.cardDeck = deck;
            life.aiLogic = aiLogic;

            string path = $"Assets/_Prototype/DataModel/EntityDataModel/{ename}.asset";
            AssetDatabase.CreateAsset(life, path);
            return life;
        }

        private static _prototype_ObstacleDataModel CreateObstacleData(string ename, int maxHealth)
        {
            _prototype_ObstacleDataModel obstacle = ScriptableObject.CreateInstance<_prototype_ObstacleDataModel>();
            obstacle.ename = ename;
            obstacle.health = new _prototype_BoundedValue<int>(0, maxHealth, maxHealth);
            obstacle.stamina = new _prototype_BoundedValue<int>(0, 0, 0);

            string path = $"Assets/_Prototype/DataModel/EntityDataModel/{ename}.asset";
            AssetDatabase.CreateAsset(obstacle, path);
            return obstacle;
        }
    }
}
