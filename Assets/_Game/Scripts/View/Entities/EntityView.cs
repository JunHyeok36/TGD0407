using UnityEngine;

namespace TDG0407.View.Entities
{

    using Domain.Entities;
    using Domain.Map;
    
    public class EntityView : MonoBehaviour
    {
        #region Fields

        [SerializeReference] private EntityState _state;

        #endregion
        #region Properties

        public EntityState State { get => _state; set => _state = value; }

        #endregion
        #region Methods

        public void Initialize(EntityState state, PointState pointState)
        {
            _state = state;
            _state.position = pointState.position;
            transform.localPosition = Vector3.zero;
        }

        #endregion
    }

}