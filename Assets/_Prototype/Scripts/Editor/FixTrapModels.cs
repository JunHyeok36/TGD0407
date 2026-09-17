using UnityEditor;
using UnityEngine;
using TDG0407._prototype;

public class FixTrapModels {
    [MenuItem("Tools/Fix Trap Models")]
    public static void Run() {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Prototype/Entities/Trap" });
        foreach(var guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            var view = instance.GetComponent<_prototype_ObstacleView>();
            if (view != null) {
                var modelPath = path.Replace(".prefab", ".asset");
                var model = AssetDatabase.LoadAssetAtPath<_prototype_EntityDataModel>(modelPath);
                if (model != null) {
                    var field = typeof(_prototype_EntityView).GetField("DEV_entityDataModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null) field.SetValue(view, model);
                    PrefabUtility.SaveAsPrefabAsset(instance, path);
                    Debug.Log("Fixed model for " + path);
                } else {
                    Debug.Log("Model not found at " + modelPath);
                }
            }
            Object.DestroyImmediate(instance);
        }
    }
}
