using UnityEngine;
using UnityEditor;
using TDG0407._prototype;

public class CreateDummyEntity
{
    [MenuItem("Tools/Create Dummy Entity")]
    public static void Execute()
    {
        // 1. Create Data Model
        var dataModel = ScriptableObject.CreateInstance<_prototype_LifeDataModel>();
        dataModel.ename = "Dummy";
        dataModel.health = new _prototype_BoundedValue<int>(0, 10000, 10000);
        dataModel.stamina = new _prototype_BoundedValue<int>(0, 10, 10);
        dataModel.side = _prototype_Side.B; // Enemy side so it can be targeted by Player
        dataModel.aiLogic = new _prototype_StandStillAI();
        dataModel.lifeStat = new _prototype_LifeStat();
        
        AssetDatabase.CreateAsset(dataModel, "Assets/_Prototype/DataModel/EntityDataModel/Life/Dummy.asset");

        // 2. Create Prefab
        var go = new GameObject("Dummy");
        var view = go.AddComponent<_prototype_LifeView>();
        
        var so = new SerializedObject(view);
        so.FindProperty("entityDataModel").objectReferenceValue = dataModel;
        so.ApplyModifiedProperties();
        
        // Add a visual representation (e.g. a simple Cylinder)
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.SetParent(go.transform);
        cylinder.transform.localPosition = new Vector3(0, 0.5f, 0);
        cylinder.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
        
        // No custom material to avoid directory errors

        // Save Prefab
        PrefabUtility.SaveAsPrefabAsset(go, "Assets/_Prototype/Prefabs/Dummy.prefab");
        GameObject.DestroyImmediate(go);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Dummy Entity DataModel and Prefab created successfully.");
    }
}
