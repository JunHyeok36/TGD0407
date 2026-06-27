using UnityEngine;

namespace TDG0407.View.Entities
{

    using Domain.Entities;
    
    public class EntityView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private EntityState _state;

        #endregion
        #region Properties

        public EntityState State { get => _state; set => _state = value; }

        #endregion
    }

}