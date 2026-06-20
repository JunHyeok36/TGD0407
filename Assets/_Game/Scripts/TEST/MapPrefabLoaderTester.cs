using UnityEngine;

namespace TDG0407.TEST
{
    using View.Map;

    public class MapPrefabLoaderTester : MonoBehaviour
    {
        public RoomView roomViewInstance;

        [ContextMenu("Test LoadRoomView")]
        public async void TestLoadRoomView()
        {
            if (roomViewInstance != null)
            {
                Debug.LogWarning("A RoomView instance is already loaded. Please release it before loading a new one.");
                return;
            }

            RoomView roomView = await MapPrefabLoader.LoadRoomViewAsync("room_1");
            if (roomView != null)
            {
                Debug.Log("Successfully loaded RoomView for room_1.");
                roomView.transform.SetParent(this.transform); // 예시로 현재 오브젝트의 자식으로 설정
                roomView.transform.position = new Vector3(0, 0, 0); // 예시로 위치를 설정

                roomViewInstance = roomView; // 인스턴스 저장 (테스트용)
            }
            else
            {
                Debug.LogError("Failed to load RoomView for room_1.");
            }
            
        }

        [ContextMenu("Test ReleaseRoomView")]
        public void TestReleaseRoomView()
        {
            if (roomViewInstance != null)
            {
                MapPrefabLoader.ReleaseRoomView(roomViewInstance);
                roomViewInstance = null;
            }
        }
    }

}