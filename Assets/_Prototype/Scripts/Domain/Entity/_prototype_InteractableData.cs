using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_InteractableData : _prototype_ObstacleData
    {
        public bool canInteract = true;
        public bool singleUse = true;
        public int interactionCount;
        public List<_prototype_InteractionDefinition> interactions = new();

        public bool CanInteract => canInteract && (!singleUse || interactionCount == 0);

        public _prototype_InteractableData() : base() { }

        public _prototype_InteractableData(_prototype_InteractableData other)
            : base(other)
        {
            canInteract = other.canInteract;
            singleUse = other.singleUse;
            interactionCount = other.interactionCount;
            interactions = other.interactions != null
                ? other.interactions.ConvertAll(interaction => interaction?.Clone())
                : new();
        }

        public bool CanInteractWith(
            _prototype_InteractionContext context,
            out string failureReason)
        {
            if (!CanInteract)
            {
                failureReason = "This interactable is not available.";
                return false;
            }

            failureReason = null;
            return true;
        }

        public void MarkInteracted()
        {
            interactionCount++;
        }

        public void AdvanceInteractionCooldowns()
        {
            if (interactions == null) return;
            foreach (var interaction in interactions)
                interaction?.AdvanceCooldown();
        }
    }

}