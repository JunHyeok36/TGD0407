using UnityEditor;
using UnityEngine;

public class ChangeArrowMaterialShader
{
    public static void Run()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Prototype/Resources/Materials/ArrowMaterial.mat");
        if (mat != null)
        {
            mat.shader = Shader.Find("Unlit/Transparent");
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log("Changed ArrowMaterial shader to Unlit/Transparent");
        }
        else
        {
            Debug.LogError("ArrowMaterial not found");
        }
    }
}
