using System;
using UnityEngine.Serialization;

namespace TDG0407.Domain.Entities
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
        public int poise = 0; // 강인도 (SP 데미지 경감)

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

        [FormerlySerializedAs("knockdownResist")]
        public int groggyResist = 1;
        public int provocationResist = 1;
        public int airborneResist = 1;

        [Obsolete("Use groggyResist instead.")]
        public int knockdownResist { get => groggyResist; set => groggyResist = value; }

        // Others
        public float healthRecoveryAmount = 1.0f;
        public float staminaRecoveryAmount = 1.0f;
        public float dodgeProb = 0.01f;
        public int deathResist = 10;
        public float criticalProb = 0.05f;
        public float criticalWeight = 2.0f;

        #endregion
        #region Methods

        public Stat Clone()
        {
            return new Stat
            {
                redPower = this.redPower,
                bluePower = this.bluePower,
                yellowPower = this.yellowPower,
                whitePower = this.whitePower,

                redResist = this.redResist,
                blueResist = this.blueResist,
                yellowResist = this.yellowResist,

                cardSlotCount = this.cardSlotCount,
                drawQuickness = this.drawQuickness,

                speed = this.speed,

                bleedingResist = this.bleedingResist,
                burningResist = this.burningResist,
                poisoningResist = this.poisoningResist,
                stunResist = this.stunResist,
                freezeResist = this.freezeResist,
                silenceResist = this.silenceResist,
                fearResist = this.fearResist,
                knockbackResist = this.knockbackResist,
                curseResist = this.curseResist,
                groggyResist = this.groggyResist,
                provocationResist = this.provocationResist,
                airborneResist = this.airborneResist,

                healthRecoveryAmount = this.healthRecoveryAmount,
                staminaRecoveryAmount = this.staminaRecoveryAmount,
                dodgeProb = this.dodgeProb,
                deathResist = this.deathResist,
                criticalProb = this.criticalProb,
                criticalWeight = this.criticalWeight
            };
        }

        #endregion
    }

}