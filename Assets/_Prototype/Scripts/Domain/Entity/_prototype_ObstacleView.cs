using System;

namespace TDG0407._prototype
{

    public class _prototype_ObstacleView : _prototype_EntityView
    {
        public _prototype_ObstacleData Data => _entityData as _prototype_ObstacleData;

        public override void Initialize(
            _prototype_EntityData entityData,
            _prototype_PointView pointView)
        {
            base.Initialize(entityData, pointView);

            if (Data != null)
            {
                Data.health.OnValueChanged += CheckDeath;
            }
        }

        protected override void OnDestroy()
        {
            if (Data != null)
            {
                Data.health.OnValueChanged -= CheckDeath;
            }
        }

        private async void CheckDeath()
        {
            if (Data != null && Data.isDestructible && Data.health.Current <= 0)
            {
                Data.health.OnValueChanged -= CheckDeath;

                var pointView = _prototype_GridManager.Instance.GetPointView(Data.point);
                if (pointView != null) pointView.RemoveEntity(this);

                if (gameObject != null) Destroy(gameObject);
            }
        }
    }

}
