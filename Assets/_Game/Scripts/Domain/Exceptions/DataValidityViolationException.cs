using System;

namespace TDG0407.Domain.Exceptions
{
    
    public class DataValidityViolationException : Exception
    {
        #region Constructors

        public DataValidityViolationException() { }
        public DataValidityViolationException(string message) : base(message) { }

        #endregion
    }

}