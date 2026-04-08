using UnityEngine;
using System;

namespace TDG0407.Core.Value
{
    
    [Serializable]
    public sealed class Stat
    {
        #region Fields

        [SerializeField] private BoundedValue<float> _value;

        #endregion

    }

}