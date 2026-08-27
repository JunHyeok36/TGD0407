using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_DamageCardAction : _prototype_CardAction
    {
        
        public _prototype_CoefficientValue[] damageCoefficients;

        public override async UniTask ExecuteCardAction(
            _prototype_EntityData source, 
            IEnumerable<_prototype_EntityData> targets,
            _prototype_CardActionParams @params)
        {
            int flatDamage = damageCoefficients != null && damageCoefficients.Length > 0 
                ? (int)(damageCoefficients[0].coefficient * 10f) : 10;
            
            foreach (var target in targets)
            {
                var damageContext = new _prototype_DamageContext(
                    source,
                    target,
                    _prototype_DamageType.Physical,
                    flatDamage,
                    flatDamage
                );
                await _prototype_InteractionManager.ApplyDamage(damageContext);
            }
        }

    }

}