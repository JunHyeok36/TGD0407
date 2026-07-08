using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Domain.Archive
{
    
    [CreateAssetMenu(fileName = "LevelCollection", menuName = "Archive/Map/LevelCollection", order = 0)]
    public class LevelCollection : ScriptableObject
    {
        #region Fields

        public LevelDocument[] levelDocuments;
        private Dictionary<string, LevelDocument> levelDocumentById;

        #endregion
        #region Methods

        private void OnEnable()
        {
            RebuildIndex();
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            RebuildIndex();
        }
        #endif

        public bool TryGetLevelDocument(string levelId, out LevelDocument levelDocument)
        {
            EnsureIndex();
            if (string.IsNullOrWhiteSpace(levelId))
            {
                levelDocument = null;
                return false;
            }

            return levelDocumentById.TryGetValue(levelId, out levelDocument);
        }

        public LevelDocument GetLevelDocument(string levelId)
        {
            return TryGetLevelDocument(levelId, out LevelDocument levelDocument)
                ? levelDocument
                : null;
        }

        private void EnsureIndex()
        {
            if (levelDocumentById == null)
                RebuildIndex();
        }

        private void RebuildIndex()
        {
            levelDocumentById = new Dictionary<string, LevelDocument>();

            if (levelDocuments == null)
                return;

            foreach (var levelDocument in levelDocuments)
            {
                if (levelDocument == null || string.IsNullOrWhiteSpace(levelDocument.id))
                    continue;

                if (levelDocumentById.ContainsKey(levelDocument.id))
                {
                    Debug.LogWarning($"Duplicated LevelDocument id '{levelDocument.id}' detected in {name}. Last one is ignored.");
                    continue;
                }

                levelDocumentById.Add(levelDocument.id, levelDocument);
            }
        }

        #endregion
    }

}
