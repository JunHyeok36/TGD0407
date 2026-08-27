using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace TDG0407._prototype.Editor
{
    public static class CardDataModelGenerator
    {
        [MenuItem("_Prototype/Generate Test Cards")]
        public static void GenerateCards()
        {
            string folderPath = "Assets/_Prototype/DataModel/CardDataModel";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/_Prototype", "DataModel");
            }

            CreateCard("BasicAttack", "가장 기본이 되는 단일 타겟 근접 공격입니다.", _prototype_CardType.Attack, _prototype_CostType.FixedStamina, 1f, 
                new _prototype_AroundRectCastSelector(), 1, 
                new _prototype_SingleTargetSelector(), 0, 
                CreateDamageAction(_prototype_Stat.Attack, 1.0f));

            CreateCard("SplashAttack", "주변 범위를 타격하는 광역 공격입니다.", _prototype_CardType.Attack, _prototype_CostType.FixedStamina, 2f, 
                new _prototype_AroundRectCastSelector(), 2, 
                new _prototype_RectSplashTargetSelector(), 1, 
                CreateDamageAction(_prototype_Stat.Attack, 0.5f));

            CreateCard("BloodStrike", "체력을 소모하여 강력한 일격을 날립니다.", _prototype_CardType.Attack, _prototype_CostType.FixedHealth, 5f, 
                new _prototype_AroundRectCastSelector(), 1, 
                new _prototype_SingleTargetSelector(), 0, 
                CreateDamageAction(_prototype_Stat.Health, 2.0f));

            CreateCard("Snipe", "멀리 떨어진 적 하나를 정밀하게 저격합니다.", _prototype_CardType.Attack, _prototype_CostType.FixedStamina, 3f, 
                new _prototype_AroundRectCastSelector(), 5, 
                new _prototype_SingleTargetSelector(), 0, 
                CreateDamageAction(_prototype_Stat.Attack, 2.5f));

            // New Combination Cards
            CreateCard("PiercingThrust", "직선으로 길게 뻗어나가는 찌르기 공격입니다.", _prototype_CardType.Attack, _prototype_CostType.FixedStamina, 2f, 
                new _prototype_CrossCastSelector(), 3, 
                new _prototype_SingleTargetSelector(), 0, 
                CreateDamageAction(_prototype_Stat.Attack, 1.5f));

            CreateCard("HolyNova", "자신을 중심으로 십자 형태의 폭발을 일으킵니다.", _prototype_CardType.Attack, _prototype_CostType.FixedStamina, 4f, 
                new _prototype_SelfCastSelector(), 0, 
                new _prototype_CrossSplashTargetSelector(), 2, 
                CreateDamageAction(_prototype_Stat.Attack, 1.2f));

            CreateCard("Earthquake", "자신 주변의 고리 범위에 지진을 일으켜 타격합니다.", _prototype_CardType.Attack, _prototype_CostType.FixedStamina, 5f, 
                new _prototype_SelfCastSelector(), 0, 
                new _prototype_RingTargetSelector(), 2, 
                CreateDamageAction(_prototype_Stat.Attack, 3.0f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Test CardDataModels generated successfully in " + folderPath);
        }

        private static void CreateCard(string id, string desc, _prototype_CardType type, _prototype_CostType costType, float costVal, 
            _prototype_ICastRangeSelector castSelector, int castRange, 
            _prototype_ITargetRangeSelector targetSelector, int targetRange, 
            _prototype_CardAction action)
        {
            _prototype_CardDataModel card = ScriptableObject.CreateInstance<_prototype_CardDataModel>();
            card.id = id;
            card.description = desc;
            card.cardType = type;
            card.costValue = new _prototype_CostValue(costType, costVal);
            card.coolTicks = new _prototype_BoundedValue<byte>((byte)0, (byte)3);
            
            // Use reflection to set private _range or _radius fields if they exist
            if (castSelector != null) SetRangeField(castSelector, castRange);
            if (targetSelector != null) SetRangeField(targetSelector, targetRange);

            card.castRange = castSelector;
            card.targetRange = targetSelector;
            card.actionList = new List<_prototype_CardAction> { action };

            AssetDatabase.CreateAsset(card, $"Assets/_Prototype/DataModel/{id}.asset");
        }

        private static void SetRangeField(object obj, int rangeValue)
        {
            FieldInfo field = obj.GetType().GetField("_range", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) field = obj.GetType().GetField("_radius", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, rangeValue);
            }
        }

        private static _prototype_DamageCardAction CreateDamageAction(_prototype_Stat stat, float coeff)
        {
            var action = new _prototype_DamageCardAction();
            action.damageCoefficients = new _prototype_CoefficientValue[]
            {
                new _prototype_CoefficientValue { stat = stat, coefficient = coeff }
            };
            return action;
        }
    }
}
