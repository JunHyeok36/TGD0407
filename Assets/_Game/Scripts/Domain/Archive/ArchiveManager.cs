using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TDG0407.Domain.Archive
{
    
    public static class ArchiveManager
    {
        #region Fields

        public static LevelCollection levelCollection = null;

        #endregion
        #region Methods

        public static async Task Initialize()
        {
            levelCollection = await Addressables.LoadAssetAsync<LevelCollection>("Assets/_Game/Data/Map/_LevelCollection.asset").Task;
        }

        #endregion
    }

}