using System;
using System.Collections.Generic;
using System.Text;

namespace TDG0407._prototype
{

    public enum _prototype_CostType : sbyte
    {
        NULL = -1,
        None = 0,
        FixedHealth,
        CurrentHealthRatio,
        LostHealthRatio,
        MaxHealthRatio,
        FixedStamina,
        CurrentStaminaRatio,
        LostStaminaRatio,
        MaxStaminaRatio,
        Coin
    }

}
