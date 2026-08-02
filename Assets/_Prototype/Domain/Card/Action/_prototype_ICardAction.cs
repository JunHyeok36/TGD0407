using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    public interface _prototype_ICardAction
    {
        public UniTask ExecuteCardAction(
            _prototype_EntityData source, 
            IEnumerable<_prototype_EntityData> targets,
            _prototype_CardActionParams @params);

    }

}
