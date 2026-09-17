using System;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_LifeStat
    {
        // 공격력
        public int redPower = 15;
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
        public int staminaRecoverAmount = 3;

        // 기타
        public int knockbackResist = 0;
        public float deathResistProp = .0f;
        public float dodgeProb = .0f;
        public float criticalProb = .0f;
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
            this.staminaRecoverAmount = other.staminaRecoverAmount;
            this.knockbackResist = other.knockbackResist;
            this.deathResistProp = other.deathResistProp;
            this.dodgeProb = other.dodgeProb;
            this.criticalProb = other.criticalProb;
            this.criticalWeight = other.criticalWeight;
        }

    }
}
