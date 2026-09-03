using UnityEngine;
using UnityEditor;
using TDG0407._prototype;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

public class Script
{
    public static void Main()
    {
        CreateLaserPrefab.Create();
        AssetDatabase.Refresh();

        if (!Directory.Exists("Assets/_Prototype/DataModel/Entity")) Directory.CreateDirectory("Assets/_Prototype/DataModel/Entity");
        if (!Directory.Exists("Assets/_Prototype/DataModel/Card")) Directory.CreateDirectory("Assets/_Prototype/DataModel/Card");

        var laserDataModel = ScriptableObject.CreateInstance<_prototype_LaserProjectileDataModel>();
        laserDataModel.ename = "LaserProjectile";
        laserDataModel.durationTicks = 2;
        laserDataModel.damage = 10;
        AssetDatabase.CreateAsset(laserDataModel, "Assets/_Prototype/DataModel/Entity/LaserProjectileDataModel.asset");

        var laserCard = ScriptableObject.CreateInstance<_prototype_CardDataModel>();
        laserCard.id = "Shoot Laser";
        laserCard.description = "Shoot a laser that stays for a few ticks.";
        laserCard.costValue = new _prototype_CostValue(_prototype_CostType.FixedStamina, 2);

        var castRange = new _prototype_CrossCastSelector();
        FieldInfo rangeField = typeof(_prototype_CrossCastSelector).GetField("_range", BindingFlags.NonPublic | BindingFlags.Instance);
        if (rangeField != null) rangeField.SetValue(castRange, 5);

        laserCard.castRange = castRange;

        var action = new _prototype_ShootLaserCardAction();
        action.laserProjectileDataModel = laserDataModel;
        action.laserProjectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prototype/Resources/Prefabs/LaserProjectile.prefab");
        laserCard.actionList = new List<_prototype_CardAction> { action };

        AssetDatabase.CreateAsset(laserCard, "Assets/_Prototype/DataModel/Card/ShootLaserCard.asset");
        
        AssetDatabase.SaveAssets();
    }
}
