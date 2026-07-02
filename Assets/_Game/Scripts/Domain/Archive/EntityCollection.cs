using UnityEngine;

namespace TDG0407.Domain.Archive
{
    
    [CreateAssetMenu(fileName = "EntityCollection", menuName = "Archive/Entities/EntityCollection", order = 0)]
    public class EntityCollection : ScriptableObject
    {
        #region Fields

        public EntityDocument[] entityDocuments;

        #endregion
        #region Methods

        public EntityDocument GetEntityDocument(string entityId)
        {
            foreach (var entityDocument in entityDocuments)
            {
                if (entityDocument.id == entityId)
                    return entityDocument;
            }
            
            return null;
        }

        #endregion
    }

}