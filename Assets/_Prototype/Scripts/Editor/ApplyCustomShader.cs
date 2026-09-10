using UnityEngine;
using UnityEditor;

public class ApplyCustomShader {
    public static void Apply() {
        string shaderPath = "Assets/_Prototype/Resources/UIAlwaysOnTop.shader";
        AssetDatabase.ImportAsset(shaderPath);
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
        
        string matPath = "Assets/_Prototype/Resources/UIAlwaysOnTop.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null && shader != null) {
            mat.shader = shader;
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log("Applied custom shader to material");
        } else {
            Debug.LogError("Failed to load material or shader");
        }
    }
}
