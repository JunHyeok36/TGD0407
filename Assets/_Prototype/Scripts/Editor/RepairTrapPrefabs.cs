using UnityEditor;
using UnityEngine;
using TDG0407._prototype;

public class RepairTrapPrefabs {
    [MenuItem("Tools/Repair Trap Prefabs")]
    public static void Run() {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Prototype/Entities/Trap" });
        foreach(var guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            var components = instance.GetComponents<Component>();
            bool modified = false;
            
            for (int i = 0; i < components.Length; i++) {
                if (components[i] == null) {
                    Debug.Log("Found missing script on " + path + ". Replacing with ObstacleView...");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(instance);
                    var newView = instance.AddComponent<_prototype_ObstacleView>();
                    
                    // We need to reassign DEV_entityDataModel
                    var modelPath = path.Replace("Entities", "Scripts/Domain/Entity").Replace(".prefab", ".asset");
                    var model = AssetDatabase.LoadAssetAtPath<_prototype_EntityDataModel>(modelPath);
                    if (model != null) {
                        var field = typeof(_prototype_EntityView).GetField("DEV_entityDataModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (field != null) field.SetValue(newView, model);
                    } else {
                        Debug.LogWarning("Could not find model for " + path + " at " + modelPath);
                    }
                    
                    modified = true;
                    break;
                }
            }
            
            if (modified) {
                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Debug.Log("Saved prefab " + path);
            }
            Object.DestroyImmediate(instance);
        }
    }
}
