using UnityEngine;
using UnityEditor;
using TDG0407._prototype;

public class CreateLaserPrefab
{
    public static void Create()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "LaserProjectile";
        go.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);

        var view = go.AddComponent<_prototype_LaserProjectileView>();

        if (!System.IO.Directory.Exists("Assets/_Prototype/Resources/Prefabs"))
        {
            System.IO.Directory.CreateDirectory("Assets/_Prototype/Resources/Prefabs");
        }

        PrefabUtility.SaveAsPrefabAsset(go, "Assets/_Prototype/Resources/Prefabs/LaserProjectile.prefab");
        GameObject.DestroyImmediate(go);
    }
}
