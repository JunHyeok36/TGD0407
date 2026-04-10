using System;
using System.Drawing;

namespace TDG0407.Domain.Map
{

    /// <summary>
    /// Room에 대한 데이터입니다.
    /// </summary>
    [Serializable]
    public class RoomData
    {
        #region Fields

        public string roomId = null;
        public Point size = new(5, 5);
        public RoomType type = RoomType.Normal;


        #endregion
        #region Properties

        public string RoomId { get => roomId; }

        #endregion
        #region Constructors

        public RoomData(string roomId)
        {
            this.roomId = roomId;
        }

        #endregion
    }
    
}