using UnityEngine;

namespace TDG0407.Domain.Archive
{
    
    [CreateAssetMenu(fileName = "PointCollection", menuName = "Archive/Map/PointCollection", order = 2)]
    public sealed class PointCollection : ScriptableObject
    {
        #region Fields

        public string id = string.Empty; // can use Level ID
        public PointDocument[] pointDocuments;

        #endregion
        #region Methods

        public PointDocument GetPointDocument(string pointId)
        {
            if (pointDocuments == null)
                return null;

            foreach (var pointDocument in pointDocuments)
            {
                if (pointDocument.id == pointId)
                    return pointDocument;
            }

            return null;
        }

        public PointDocument ChoiceOne(System.Random random = null)
        {
            if (pointDocuments == null || pointDocuments.Length == 0)
                return null;

            random ??= new System.Random();

            int index = random.Next(pointDocuments.Length);
            return pointDocuments[index];
        }

        #endregion
    }

}