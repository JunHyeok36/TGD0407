using System;

namespace TDG0407._prototype
{

    public sealed class _prototype_LifeView : _prototype_EntityView
    {

        public _prototype_LifeData Data => _entityData as _prototype_LifeData;

        public override void Initialize(_prototype_PointView pointView)
        {
            base.Initialize(pointView);

            // Create Status Bar
            UnityEngine.GameObject prefab = UnityEngine.Resources.Load<UnityEngine.GameObject>("LifeStatusBarPrefab");
            if (prefab != null)
            {
                var statusBarObj = UnityEngine.Object.Instantiate(prefab);
                var statusBar = statusBarObj.GetComponent<_prototype_LifeStatusBar>();
                if (statusBar != null)
                {
                    statusBar.Initialize(this);
                }
            }
        }
    }
}