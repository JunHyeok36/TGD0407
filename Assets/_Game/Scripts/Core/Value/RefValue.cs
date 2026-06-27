namespace TDG0407.Core.Value
{

    public class Ref<T> where T : struct
    {
        #region Fields

        private T value;

        #endregion
        #region Properties

        public T Value { get => value; set => this.value = value; }

        #endregion
        #region Constructors

        public Ref(T value) => this.value = value;

        #endregion
    }

}