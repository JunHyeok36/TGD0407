using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TDG0407.Domain.Archive
{

    using View.Map;
    
    [CreateAssetMenu(fileName = "PointDocument", menuName = "Archive/Map/PointDocument", order = 3)]
    public sealed class PointDocument : ScriptableObject
    {
        #region Fields

        public AssetReferenceGameObject prefab;

        public string id = string.Empty;

        #endregion
        #region Methods

        public async UniTask<PointView> InstantiatePointView()
        {
            var handle = prefab.InstantiateAsync();
            var pointViewObject = await handle.Task;
            if(pointViewObject == null)
            {
                Debug.LogError($"Failed to instantiate PointView prefab for point ID '{id}'.");
                return null;
            }
            if (!pointViewObject.TryGetComponent<PointView>(out var pointView))
            {
                Debug.LogError($"PointView prefab for point ID '{id}' is missing the PointView component.");
                return null;
            }

            return pointView;
            // Destory with 'Addressables.ReleaseInstance(pointView.gameObject)' when the point is no longer needed.
        }

        #endregion
    }

}