using System;

namespace TDG0407.Core.Value
{
    
    /// <summary>
    /// Entity의 능력치를 나타냅니다.
    /// </summary>
    [Serializable]
    public class Stat
    {
        #region Fields

        // Attribute Power
        public int redPower = 1;
        public int bluePower = 0;
        public int yellowPower = 0;
        public int whitePower = 0;

        // Attribute Resistance
        public int redResist = 0;
        public int blueResist = 0;
        public int yellowResist = 0;

        // Cards related
        public int cardSlotCount = 4;
        public float drawQuickness = 1.0f;

        // Action per ticks related
        public int speed = 1;

        // StatusEffects related
        public int bleedingResist = 1;
        public int burningResist = 1;
        public int poisoningResist = 1;
        public int stunResist = 1;
        public int freezeResist = 1;
        public int silenceResist = 1;
        public int fearResist = 1;
        public int knockbackResist = 1;
        public int curseResist = 1;
        public int knockdownResist = 1;

        // Others
        public float healthRecoveryAmount = 1.0f;
        public float staminaRecoveryAmount = 1.0f;
        public float dodgeProb = 0.01f;
        public int deathResist = 10;
        public float criticalProb = 0.05f;
        public float criticalWeight = 2.0f;

        #endregion

    }

}