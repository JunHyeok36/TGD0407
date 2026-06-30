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

        #endregion
        #region Methods
        
        public RoomDocument GetRoomDocument(string roomId)
        {
            foreach (var roomDocument in roomDocuments)
            {
                if (roomDocument.id == roomId)
                    return roomDocument;
            }

            return null;
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

        #endregion
    }

}