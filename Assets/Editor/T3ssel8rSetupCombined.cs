using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Reflection;
using System.Linq;

public class T3ssel8rSetupCombined
{
    [MenuItem("Tools/TGD/Setup Combined Pixel Art Shader")]
    public static void Setup()
    {
        string path = "Assets/Settings/URP/PixelArtPostProcessMat.mat";
        string shaderName = "Hidden/Custom/URP/PixelArtPostProcess";
        
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader != null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
                Debug.Log($"Created Material: {path}");
            }
            else
            {
                Debug.LogError($"Shader not found: {shaderName}");
                return;
            }
        }

        UpdateOutlineFeature(mat);
    }

    private static void UpdateOutlineFeature(Material newMat)
    {
        UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null) return;

        ScriptableRendererData rendererData = GetRendererData(urpAsset);
        if (rendererData != null)
        {
            var feature = rendererData.rendererFeatures.FirstOrDefault(f => f.name == "SobelOutlineFeature" || f.name == "PixelArtFeature");
            
            if (feature != null)
            {
                feature.name = "PixelArtFeature";
                System.Type featureType = feature.GetType();
                FieldInfo materialField = featureType.GetField("passMaterial", BindingFlags.Public | BindingFlags.Instance);
                if (materialField != null)
                {
                    materialField.SetValue(feature, newMat);
                    EditorUtility.SetDirty(rendererData);
                    AssetDatabase.SaveAssets();
                    Debug.Log("Successfully updated URP Feature to use the combined Pixel Art Material.");
                }
            }
            else
            {
                Debug.LogWarning("Could not find the existing Outline Feature. Please manually add a FullScreenPassRendererFeature and assign PixelArtPostProcessMat.mat");
            }
        }
    }

    private static ScriptableRendererData GetRendererData(UniversalRenderPipelineAsset urpAsset)
    {
        FieldInfo propertyInfo = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        if (propertyInfo != null)
        {
            ScriptableRendererData[] rendererDatas = propertyInfo.GetValue(urpAsset) as ScriptableRendererData[];
            if (rendererDatas != null && rendererDatas.Length > 0)
            {
                return rendererDatas[0];
            }
        }
        return null;
    }
}
