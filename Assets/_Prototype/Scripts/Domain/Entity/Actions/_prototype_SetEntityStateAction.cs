using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    public enum _prototype_StateActionTarget
    {
        SourceProvider,
        Targets,
        Actor
    }

    [Serializable]
    public class _prototype_SetEntityStateAction : _prototype_EntityAction
    {
        public _prototype_StateActionTarget actionTarget = _prototype_StateActionTarget.SourceProvider;
        public string stateKey;
        public string newValue;
        public bool toggleBool = false;

        public override UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            List<_prototype_EntityData> targetEntities = new();

            switch (actionTarget)
            {
                case _prototype_StateActionTarget.SourceProvider:
                    if (@params is _prototype_CardActionParams cardParams && cardParams.SourceProvider != null)
                    {
                        targetEntities.Add(cardParams.SourceProvider);
                    }
                    else if (targets != null)
                    {
                        targetEntities.AddRange(targets);
                    }
                    break;
                case _prototype_StateActionTarget.Targets:
                    if (targets != null) targetEntities.AddRange(targets);
                    break;
                case _prototype_StateActionTarget.Actor:
                    if (source != null) targetEntities.Add(source);
                    break;
            }

            foreach (var entity in targetEntities)
            {
                if (entity == null || entity.components == null) continue;

                _prototype_EntityView entityView = null;
                if (_prototype_GridManager.Instance != null)
                {
                    var ptView = _prototype_GridManager.Instance.GetPointView(entity.point);
                    if (ptView != null && ptView.PlacedEntityViews != null)
                    {
                        entityView = ptView.PlacedEntityViews.Find(v => v.EntityData == entity);
                    }
                }

                foreach (var comp in entity.components)
                {
                    if (comp is _prototype_StateComponentData stateComp)
                    {
                        if (stateComp.Owner == null && entityView != null)
                        {
                            stateComp.Initialize(entityView);
                        }

                        if (toggleBool)
                        {
                            stateComp.ToggleBoolState(stateKey);
                        }
                        else
                        {
                            stateComp.SetState(stateKey, newValue);
                        }
                        break;
                    }
                }
            }

            return UniTask.CompletedTask;
        }
    }
}
