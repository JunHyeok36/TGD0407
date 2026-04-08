namespace TDG0407.Domain.Interfaces
{

    /// <summary>
    /// Tick에 영향을 받는 요소에 대한 인터페이스입니다.
    /// </summary>
    public interface IOnTick
    {
        
        /// <summary>
        /// Tick이 한 번 지날 때마다 호출됩니다.
        /// </summary>
        public void OnTick();

    }

}