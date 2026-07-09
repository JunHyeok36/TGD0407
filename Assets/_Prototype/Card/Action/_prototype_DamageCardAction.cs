using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_DamageCardAction : _prototype_ICardAction
    {

        public int damage = 0;

        public UniTask ExecuteCardAction(
            _prototype_EntityData source, 
            IEnumerable<_prototype_EntityData> targets,
            _prototype_CardActionParams @params)
        {
            throw new NotImplementedException();
        }

    }

}