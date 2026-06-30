using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TDG0407.View.Map
{
    
    public static class MapPrefabLoader
    {
        [Serializable]
        private class MapPrefabJSONElement
        {
            public string id;
            public string path;
        }

        [Serializable]
        private class MapPrefabJSON
        {
            public MapPrefabJSONElement[] prefabs;
        }

        public static async Task<RoomView> LoadRoomViewAsync(string roomId)
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>("Data/Prefabs/Map/test_map_prefabs");
            if (jsonAsset == null)
            {
                Debug.LogError("Failed to load prefab data JSON.");
                return null;
            }

            MapPrefabJSONElement[] prefabDataArray = JsonUtility.FromJson<MapPrefabJSON>(jsonAsset.text).prefabs;
            MapPrefabJSONElement prefabData = System.Array.Find(prefabDataArray, data => data.id == roomId);
            if (prefabData == null)
            {
                Debug.LogError($"Prefab data for room ID '{roomId}' not found.");
                return null;
            }
            //TEST
            for (int i = 0; i < prefabDataArray.Length; i++)
            {
                Debug.Log($"Prefab Data {i}: ID={prefabDataArray[i].id}, Path={prefabDataArray[i].path}");
            }

            // 로드 대신 InstantiateAsync를 사용하여 씬에 바로 생성(인스턴스화)하는 것이 안전합니다.
            // (에셋 원본을 수정하지 않기 위함)
            var handle = Addressables.InstantiateAsync(prefabData.path);
            var instanceObject = await handle.Task;
            
            if (instanceObject == null)
            {
                Debug.LogError($"Failed to instantiate prefab at path/key '{prefabData.path}'. Please check if the Addressable Key is correct.");
                return null;
            }

            RoomView roomView = instanceObject.GetComponent<RoomView>();
            if (roomView == null)
            {
                Debug.LogError($"Prefab '{prefabData.path}' is loaded/instantiated, but missing 'RoomView' component.");
                return null;
            }

            return roomView;
        }

        public static void ReleaseRoomView(RoomView roomView)
        {
            if (roomView != null)
            {
                Addressables.ReleaseInstance(roomView.gameObject);
            }
        }

    }

}