using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Domain.Archive
{

    using Domain.Map;
    
    [CreateAssetMenu(fileName = "RoomCollection", menuName = "Archive/Map/RoomCollection", order = 4)]
    public sealed class RoomCollection : ScriptableObject
    {
        #region Fields

        public string id = string.Empty; // can use Level ID
        public RoomDocument[] roomDocuments;
        private Dictionary<string, RoomDocument> roomDocumentById;

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

        public bool TryGetRoomDocument(string roomId, out RoomDocument roomDocument)
        {
            EnsureIndex();
            if (string.IsNullOrWhiteSpace(roomId))
            {
                roomDocument = null;
                return false;
            }

            return roomDocumentById.TryGetValue(roomId, out roomDocument);
        }
        
        public RoomDocument GetRoomDocument(string roomId)
        {
            return TryGetRoomDocument(roomId, out RoomDocument roomDocument)
                ? roomDocument
                : null;
        }

        public RoomDocument ChoiceOne(RoomScale scale = RoomScale.NULL, RoomType type = RoomType.NULL, System.Random random = null)
        {
            if (roomDocuments == null || roomDocuments.Length == 0)
                return null;

            random ??= new System.Random();

            var filteredRooms = new List<RoomDocument>();
            Predicate<RoomDocument> filter = null;
            if (scale < 0 && type < 0)
                filter = roomDocument => true;
            else if (scale < 0)
                filter = roomDocument => roomDocument.type == type;
            else if (type < 0)
                filter = roomDocument => roomDocument.scale == scale;
            else 
                filter = roomDocument => roomDocument.scale == scale && roomDocument.type == type;

            int totalWeight = 0;
            foreach (var roomDocument in roomDocuments)
            {
                if (filter(roomDocument))
                {
                    filteredRooms.Add(roomDocument);
                    totalWeight += roomDocument.appearanceWeight;
                }
            }

            if (filteredRooms.Count == 0)
                return null;

            int randomValue = random.Next(totalWeight);
            int cumulativeWeight = 0;

            for (int i = 0; i < filteredRooms.Count; i++)
            {
                cumulativeWeight += filteredRooms[i].appearanceWeight;
                if (randomValue < cumulativeWeight)
                    return filteredRooms[i];
            }

            return null;
        }

        private void EnsureIndex()
        {
            if (roomDocumentById == null)
                RebuildIndex();
        }

        private void RebuildIndex()
        {
            roomDocumentById = new Dictionary<string, RoomDocument>();

            if (roomDocuments == null)
                return;

            foreach (var roomDocument in roomDocuments)
            {
                if (roomDocument == null || string.IsNullOrWhiteSpace(roomDocument.id))
                    continue;

                if (roomDocumentById.ContainsKey(roomDocument.id))
                {
                    Debug.LogWarning($"Duplicated RoomDocument id '{roomDocument.id}' detected in {name}. Last one is ignored.");
                    continue;
                }

                roomDocumentById.Add(roomDocument.id, roomDocument);
            }
        }

        #endregion
    }

}