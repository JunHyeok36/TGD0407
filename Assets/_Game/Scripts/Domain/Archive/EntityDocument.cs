using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TDG0407.Domain.Archive
{
    
    using Core.Grid;
    using Core.Value;
    using Domain.Entities;

    public class EntityDocument : ScriptableObject
    {
        #region Fields
    
        public AssetReferenceGameObject prefab;

        public string id = string.Empty;
        public Point position = new(0, 0);
        public BoundedValue<int> health = new(0, 50);
        public BoundedValue<int> stamina = new(0, 10);
        public List<Shield> shields = new();

        #endregion
    }

}