using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDG0407.Domain.Archive
{
    
    [CreateAssetMenu(fileName = "LevelDocument", menuName = "Archive/Map/LevelDocument", order = 1)]
    public class LevelDocument : ScriptableObject
    {
        #region Fields

        public string id = string.Empty;

        public RoomCollection roomCollection = null;
        //public EntityCollection entityCollection = null;

        #endregion
    }

}