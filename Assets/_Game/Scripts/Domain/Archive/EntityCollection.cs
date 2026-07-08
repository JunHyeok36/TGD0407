using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Domain.Archive
{
    
    [CreateAssetMenu(fileName = "EntityCollection", menuName = "Archive/Entities/EntityCollection", order = 0)]
    public class EntityCollection : ScriptableObject
    {
        #region Fields

        public EntityDocument[] entityDocuments;
        private Dictionary<string, EntityDocument> entityDocumentById;

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

        public bool TryGetEntityDocument(string entityId, out EntityDocument entityDocument)
        {
            EnsureIndex();
            if (string.IsNullOrWhiteSpace(entityId))
            {
                entityDocument = null; 
                return false;
            }

            return entityDocumentById.TryGetValue(entityId, out entityDocument);
        }

        public EntityDocument GetEntityDocument(string entityId)
        {
            return TryGetEntityDocument(entityId, out EntityDocument entityDocument)
                ? entityDocument
                : null;
        }

        private void EnsureIndex()
        {
            if (entityDocumentById == null)
                RebuildIndex();
        }

        private void RebuildIndex()
        {
            entityDocumentById = new Dictionary<string, EntityDocument>();

            if (entityDocuments == null)
                return;

            foreach (var entityDocument in entityDocuments)
            {
                if (entityDocument == null || string.IsNullOrWhiteSpace(entityDocument.id))
                    continue;

                if (entityDocumentById.ContainsKey(entityDocument.id))
                {
                    Debug.LogWarning($"Duplicated EntityDocument id '{entityDocument.id}' detected in {name}. Last one is ignored.");
                    continue;
                }

                entityDocumentById.Add(entityDocument.id, entityDocument);
            }
        }

        #endregion
    }

}