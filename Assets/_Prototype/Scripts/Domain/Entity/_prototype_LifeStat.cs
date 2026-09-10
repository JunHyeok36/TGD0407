using System;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_LifeStat
    {
        // 공격력
        public int redPower = 5;
        public int bluePower = 0;

        // 방어력
        public int redResist;
        public int blueResist;

        // 덱 관련
        public int handCardSlotCount = 5;
        public int drawQuickness = 0;
        public int curseResist;

        // 지속효과 저항 관련
        public int bleedingResist;
        public int burningResist;
        public int poisoningResist;
        public int stunResist;
        public int freezeResist;
        public int silenceResist;
        public int fearResist;
        public int knockdownResist;

        // 회복 관련
        public int healthRecoverAmount = 5;
        public int staminaRecoverAmount = 3;

        // 기타
        public int knockbackResist;
        public int deathResist;
        public int dodgeProb;
        public float criticalProb = .0f; // 0 ~ 1
        public int criticalWeight = 50;

        public _prototype_LifeStat() { }

        public _prototype_LifeStat(_prototype_LifeStat other)
        {
            this.redPower = other.redPower;
            this.bluePower = other.bluePower;
            this.redResist = other.redResist;
            this.blueResist = other.blueResist;
            this.handCardSlotCount = other.handCardSlotCount;
            this.drawQuickness = other.drawQuickness;
            this.curseResist = other.curseResist;
            this.bleedingResist = other.bleedingResist;
            this.burningResist = other.burningResist;
            this.poisoningResist = other.poisoningResist;
            this.stunResist = other.stunResist;
            this.freezeResist = other.freezeResist;
            this.silenceResist = other.silenceResist;
            this.fearResist = other.fearResist;
            this.knockdownResist = other.knockdownResist;
            this.healthRecoverAmount = other.healthRecoverAmount;
            this.staminaRecoverAmount = other.staminaRecoverAmount;
            this.knockbackResist = other.knockbackResist;
            this.deathResist = other.deathResist;
            this.dodgeProb = other.dodgeProb;
            this.criticalProb = other.criticalProb;
            this.criticalWeight = other.criticalWeight;
        }

    }
}
