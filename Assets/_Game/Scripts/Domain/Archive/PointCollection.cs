using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Domain.Archive
{
    
    [CreateAssetMenu(fileName = "PointCollection", menuName = "Archive/Map/PointCollection", order = 2)]
    public sealed class PointCollection : ScriptableObject
    {
        #region Fields

        public string id = string.Empty; // can use Level ID
        public PointDocument[] pointDocuments;
        private Dictionary<string, PointDocument> pointDocumentById;

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

        public bool TryGetPointDocument(string pointId, out PointDocument pointDocument)
        {
            EnsureIndex();
            if (string.IsNullOrWhiteSpace(pointId))
            {
                pointDocument = null;
                return false;
            }

            return pointDocumentById.TryGetValue(pointId, out pointDocument);
        }

        public PointDocument GetPointDocument(string pointId)
        {
            return TryGetPointDocument(pointId, out PointDocument pointDocument)
                ? pointDocument
                : null;
        }

        public PointDocument ChoiceOne(System.Random random = null)
        {
            if (pointDocuments == null || pointDocuments.Length == 0)
                return null;

            random ??= new System.Random();

            int index = random.Next(pointDocuments.Length);
            return pointDocuments[index];
        }

        private void EnsureIndex()
        {
            if (pointDocumentById == null)
                RebuildIndex();
        }

        private void RebuildIndex()
        {
            pointDocumentById = new Dictionary<string, PointDocument>();

            if (pointDocuments == null)
                return;

            foreach (var pointDocument in pointDocuments)
            {
                if (pointDocument == null || string.IsNullOrWhiteSpace(pointDocument.id))
                    continue;

                if (pointDocumentById.ContainsKey(pointDocument.id))
                {
                    Debug.LogWarning($"Duplicated PointDocument id '{pointDocument.id}' detected in {name}. Last one is ignored.");
                    continue;
                }

                pointDocumentById.Add(pointDocument.id, pointDocument);
            }
        }

        #endregion
    }

}