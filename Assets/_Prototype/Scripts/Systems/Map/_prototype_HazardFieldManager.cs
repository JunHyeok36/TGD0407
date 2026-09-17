using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    public class _prototype_HazardFieldManager : MonoBehaviour
    {
        public static _prototype_HazardFieldManager Instance { get; private set; }

        private List<_prototype_HazardFieldInstance> _activeFields = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            _prototype_TickManager.RegisterPostTick(OnPostTick);
        }

        private void OnDestroy()
        {
            _prototype_TickManager.UnregisterPostTick(OnPostTick);
        }

        public void SpawnHazardField(List<_prototype_Point> points, int durationTicks, int damage, _prototype_StatusType statusType, _prototype_EntityView owner)
        {
            var instance = new _prototype_HazardFieldInstance(points, durationTicks, damage, statusType, owner);
            _activeFields.Add(instance);
        }

        private async UniTask OnPostTick()
        {
            for (int i = _activeFields.Count - 1; i >= 0; i--)
            {
                var field = _activeFields[i];
                await field.ProcessTick();
                if (field.IsExpired)
                {
                    field.Cleanup();
                    _activeFields.RemoveAt(i);
                }
            }
        }
    }

    public class _prototype_HazardFieldInstance
    {
        public List<_prototype_Point> Points { get; private set; }
        public int RemainingTicks { get; private set; }
        public int Damage { get; private set; }
        public _prototype_StatusType StatusType { get; private set; }
        public _prototype_EntityView Owner { get; private set; }
        public bool IsExpired => RemainingTicks <= 0;

        private List<GameObject> _visualPieces = new();

        public _prototype_HazardFieldInstance(List<_prototype_Point> points, int durationTicks, int damage, _prototype_StatusType statusType, _prototype_EntityView owner)
        {
            Points = points;
            RemainingTicks = durationTicks;
            Damage = damage;
            StatusType = statusType;
            Owner = owner;
        }

        public async UniTask ProcessTick()
        {
            RemainingTicks--;

            // Deal damage to entities on these points
            foreach (var pt in Points)
            {
                var pv = _prototype_GridManager.Instance?.GetPointView(pt);
                if (pv != null)
                {
                    var targets = new List<_prototype_EntityView>(pv.PlacedEntityViews);
                    foreach (var ev in targets)
                    {
                        if (ev != null && ev.EntityData is _prototype_LifeData life)
                        {
                            var ctx = new _prototype_DamageContext(Owner != null ? Owner.EntityData : null, life, _prototype_DamageType.Magical, Damage, Damage);
                            await life.TakeDamage(ctx);
                        }
                    }
                }
            }
        }

        public void Cleanup()
        {
            foreach (var go in _visualPieces)
            {
                if (go != null) Object.Destroy(go);
            }
            _visualPieces.Clear();
        }
    }
}
