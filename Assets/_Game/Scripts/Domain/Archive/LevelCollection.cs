using UnityEngine;

namespace TDG0407.Domain.Archive
{
    
    [CreateAssetMenu(fileName = "LevelCollection", menuName = "Archive/Map/LevelCollection", order = 0)]
    public class LevelCollection : ScriptableObject
    {
        #region Fields

        public LevelDocument[] levelDocuments;

        #endregion
        #region Methods

        public LevelDocument GetLevelDocument(string levelId)
        {
            foreach (var levelDocument in levelDocuments)
            {
                if (levelDocument.id == levelId)
                    return levelDocument;
            }
            
            return null;
        }

        #endregion
    }

}
