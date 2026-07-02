using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TDG0407.Domain.Archive
{
    
    using Core.Value;
    using Domain.Entities;
    using View.Entities;
    using View.Map;

    public abstract class EntityDocument : ScriptableObject
    {
        #region Fields
    
        public AssetReferenceGameObject prefab;

        public string id = string.Empty;
        //public Point position = new(0, 0);
        public BoundedValue<int> health = new(0, 50);
        public BoundedValue<int> stamina = new(0, 10);
        public List<Shield> shields = new();

        #endregion
        #region Methods

        public abstract UniTask<EntityView> InstantiateEntityView(
            string levelId,
            int entityInstanceId,
            PointView pointView);

        public abstract EntityState CreateEntityState();

        #endregion
    }

}