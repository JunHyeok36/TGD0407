using UnityEngine;

namespace TDG0407._prototype
{

    public class _prototype_InteractableView : _prototype_ObstacleView
    {
        public new _prototype_InteractableData Data => _entityData as _prototype_InteractableData;

        public override void Initialize(_prototype_PointView pointView)
        {
            base.Initialize(pointView);
            _prototype_TickManager.RegisterPostTick(ProcessInteractionTick);
        }

        private void OnDestroy()
        {
            _prototype_TickManager.UnregisterPostTick(ProcessInteractionTick);
        }

        private async Cysharp.Threading.Tasks.UniTask ProcessInteractionTick()
        {
            if (Data == null || !Data.canInteract)
                return;

            Data.AdvanceInteractionCooldowns();

            var lifeViews = FindObjectsByType<_prototype_LifeView>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            foreach (var lifeView in lifeViews)
            {
                if (lifeView == null || lifeView.Data == null)
                    continue;

                if (await _prototype_InteractionManager.Interact(lifeView.Data, Data))
                    break;
            }
        }
    }

}