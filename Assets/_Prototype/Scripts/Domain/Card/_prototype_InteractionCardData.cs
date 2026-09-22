using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_InteractionCardData : _prototype_CardData
    {
        public string interactionKey;
        public List<_prototype_Condition> visibilityConditions = new();

        public _prototype_InteractionCardData() : base() { }

        public _prototype_InteractionCardData(
            string id,
            string interactionKey,
            List<_prototype_Condition> visibilityConditions = null,
            string descriptionLocalizationKey = ""
        ) : base(id, descriptionLocalizationKey)
        {
            this.interactionKey = interactionKey;
            if (visibilityConditions != null)
            {
                this.visibilityConditions = new List<_prototype_Condition>(visibilityConditions);
            }
        }

        public _prototype_InteractionCardData(_prototype_InteractionCardData other) : base(other)
        {
            if (other == null) return;
            this.interactionKey = other.interactionKey;
            if (other.visibilityConditions != null)
            {
                this.visibilityConditions = new List<_prototype_Condition>(other.visibilityConditions);
            }
        }

        public override _prototype_CardData Clone()
        {
            return new _prototype_InteractionCardData(this);
        }

        public override bool IsVisible(_prototype_ConditionContext context)
        {
            if (visibilityConditions != null)
            {
                foreach (var cond in visibilityConditions)
                {
                    if (cond == null) continue;
                    if (!cond.Evaluate(context).IsSatisfied)
                        return false;
                }
            }
            return true;
        }

        public async UniTask ExecuteInteraction(_prototype_EntityData caster)
        {
            if (sourceProvider == null)
            {
                Debug.LogWarning($"[InteractionCard] SourceProvider is null on card '{id}'.");
                return;
            }

            bool handled = false;

            // 1. 엔티티 자체가 _prototype_IInteractable을 구현한 경우
            if (sourceProvider is _prototype_IInteractable interactableEntity)
            {
                await interactableEntity.Interact(caster, interactionKey);
                handled = true;
            }

            // 2. 엔티티의 컴포넌트 중 _prototype_IInteractable을 구현한 경우
            if (sourceProvider.components != null)
            {
                foreach (var comp in sourceProvider.components)
                {
                    if (comp is _prototype_IInteractable interactableComp)
                    {
                        await interactableComp.Interact(caster, interactionKey);
                        handled = true;
                    }
                }
            }

            if (!handled)
            {
                Debug.LogWarning($"[InteractionCard] No IInteractable found on entity '{sourceProvider.ename}' for key '{interactionKey}'.");
            }
        }
    }
}
