using UnityEngine;

namespace TDG0407.View.Map
{
    
    public static class MapPrefabLoader
    {
        public static RoomView LoadRoomView(string roomId)
        {
            // TODO: json 파일을 읽어서 roomId에 해당하는 프리팹을 로드하도록 수정
            return Resources.Load<GameObject>("Prefabs/Map/Room").GetComponent<RoomView>();
        }

        
    }

}