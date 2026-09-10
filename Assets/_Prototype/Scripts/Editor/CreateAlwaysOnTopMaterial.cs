using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CreateAlwaysOnTopMaterial {
    public static void Create() {
        string matPath = "Assets/_Prototype/Resources/UIAlwaysOnTop.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null) {
            Shader shader = Shader.Find("UI/Default");
            mat = new Material(shader);
            // Unity UI/Default uses UnityEngine.Rendering.CompareFunction for unity_GUIZTestMode
            // 8 = Always
            mat.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
            AssetDatabase.CreateAsset(mat, matPath);
        } else {
            mat.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
            EditorUtility.SetDirty(mat);
        }
        
        string prefabPath = "Assets/_Prototype/Resources/LifeStatusBarPrefab.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null) {
            Image[] images = prefab.GetComponentsInChildren<Image>(true);
            foreach (var img in images) {
                img.material = mat;
            }
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log("Material applied to prefab.");
        }
    }
}
