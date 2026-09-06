using UnityEngine;
using UnityEditor;
using TDG0407._prototype;

public class UpdateShootArrowCard
{
    [MenuItem("Tools/Update Shoot Arrow Card 2")]
    public static void Execute()
    {
        var shootCard = AssetDatabase.LoadAssetAtPath<_prototype_CardDataModel>("Assets/_Prototype/DataModel/CardDataModel/ShootArrow.asset");
        if (shootCard != null && shootCard.actionList != null && shootCard.actionList.Count > 0)
        {
            var shootAction = shootCard.actionList[0] as _prototype_SpawnDirectionalProjectileCardAction;
            if (shootAction != null)
            {
                var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prototype/Prefabs/ArrowProjectile.prefab");
                var newModel = AssetDatabase.LoadAssetAtPath<_prototype_ProjectileDataModel>("Assets/_Prototype/DataModel/EntityDataModel/Projectile/ArrowProjectile.asset");
                shootAction.projectilePrefab = newPrefab;
                shootAction.projectileModel = newModel;
                EditorUtility.SetDirty(shootCard);
                AssetDatabase.SaveAssets();
                Debug.Log("ShootArrow card updated with ArrowProjectile prefab and model.");
            }
        }
    }
}
