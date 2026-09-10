using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [CreateAssetMenu(fileName = "_prototype_InteractableDataModel", menuName = "_Prototype/InteractableData Model")]
    public class _prototype_InteractableDataModel : _prototype_EntityDataModel
    {
        public bool canInteract = true;
        public bool singleUse = true;
        [UnityEngine.SerializeReference, SubclassSelector]
        public List<_prototype_InteractionDefinition> interactions = new();

        public _prototype_InteractableData CreateInteractableData()
        {
            return new()
            {
                ename = ename,
                health = health.Clone(),
                stamina = stamina.Clone(),
                point = point,
                movementType = movementType,
                canInteract = canInteract,
                singleUse = singleUse,
                interactions = interactions != null
                    ? interactions.ConvertAll(interaction => interaction?.Clone())
                    : new()
            };
        }
    }

}