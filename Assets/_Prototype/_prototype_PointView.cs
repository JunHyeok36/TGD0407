using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{

    public class _prototype_PointView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, ReadOnly] private _prototype_Point point;
        [SerializeField, ReadOnly] private List<_prototype_EntityView> placedEntityViews;

        public _prototype_Point Point => point;

        public void Initialize(_prototype_Point point)
        {
            this.point = point;
            this.placedEntityViews = GetComponentsInChildren<_prototype_EntityView>().ToList();
            foreach (var entityView in placedEntityViews)
            {
                entityView.Initialize();
            }
        }
    }

}
