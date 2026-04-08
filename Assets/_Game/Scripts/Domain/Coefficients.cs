using System;

namespace TDG0407.Domain
{
    
    /// <summary>
    /// 계수값 목록을 나타냅니다.
    /// </summary>
    [Serializable]
    public sealed class Coefficients
    {
        #region Fields

        public float[] values = new float[0];

        #endregion
        #region Constructors

        public Coefficients(params float[] values)
        {
            this.values = values;
        }
        public Coefficients(Coefficients other)
        {
            this.values = new float[other.values.Length];
            for(int i = 0; i < values.Length; i++)
                this.values[i] = other.values[i];
        }

        #endregion
    }

}
