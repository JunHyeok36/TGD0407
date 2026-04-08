namespace TDG0407.Domain.Interfaces
{
    
    /// <summary>
    /// 데이터 검증이 필요한 경우 사용하는 인터페이스입니다.
    /// </summary>
    public interface IDataValidatable
    { 

        /// <summary>
        /// 데이터를 검증합니다.
        /// </summary>
        public void ValidateData();
        
    }

}