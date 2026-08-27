using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;
using System.Linq;

public class T3ssel8rSetup
{
    [MenuItem("Tools/TGD/Setup T3ssel8r Materials and Feature")]
    public static void Setup()
    {
        // 1. Create Materials
        CreateMaterial("Assets/Settings/URP/ToonMaterial.mat", "Custom/URP/ToonShader");
        CreateMaterial("Assets/Settings/URP/SobelOutlineMaterial.mat", "Hidden/Custom/URP/SobelOutline");

        // 2. Add FullScreenPassRendererFeature to URP Asset
        AddOutlineFeature();
    }

    private static void CreateMaterial(string path, string shaderName)
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(path) == null)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader != null)
            {
                Material mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
                Debug.Log($"Created Material: {path}");
            }
            else
            {
                Debug.LogError($"Shader not found: {shaderName}");
            }
        }
        else
        {
            Debug.Log($"Material already exists: {path}");
        }
    }

    private static void AddOutlineFeature()
    {
        UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogError("Current Render Pipeline is not URP.");
            return;
        }

        // We need to modify the renderer data. 
        // In URP 12+, we can get renderer data via reflection or assume the first one.
        ScriptableRendererData rendererData = GetRendererData(urpAsset);

        if (rendererData != null)
        {
            // Check if feature already exists
            if (rendererData.rendererFeatures.Any(f => f.name == "SobelOutlineFeature"))
            {
                Debug.Log("Outline feature already exists.");
                return;
            }

            // Create FullScreenPassRendererFeature
            // Note: FullScreenPassRendererFeature is in UnityEngine.Rendering.Universal
            // It might be named differently depending on URP version. Let's try to add it.
            // FullScreenPassRendererFeature was added in URP 14 (Unity 2022.2).
            System.Type featureType = System.Type.GetType("UnityEngine.Rendering.Universal.FullScreenPassRendererFeature, Unity.RenderPipelines.Universal.Runtime");
            
            if (featureType != null)
            {
                ScriptableRendererFeature feature = ScriptableObject.CreateInstance(featureType) as ScriptableRendererFeature;
                feature.name = "SobelOutlineFeature";
                
                // Set the material
                Material outlineMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/URP/SobelOutlineMaterial.mat");
                
                // Use reflection to set passMaterial field
                FieldInfo materialField = featureType.GetField("passMaterial", BindingFlags.Public | BindingFlags.Instance);
                if (materialField != null && outlineMat != null)
                {
                    materialField.SetValue(feature, outlineMat);
                }

                // Use reflection to set injection point (AfterRenderingPostProcessing)
                FieldInfo injectionField = featureType.GetField("injectionPoint", BindingFlags.Public | BindingFlags.Instance);
                if (injectionField != null)
                {
                    // FullScreenPassInjectionPoint.AfterRenderingPostProcessing = 2
                    injectionField.SetValue(feature, 2); 
                }

                AssetDatabase.AddObjectToAsset(feature, rendererData);
                rendererData.rendererFeatures.Add(feature);
                
                // Force save
                EditorUtility.SetDirty(rendererData);
                AssetDatabase.SaveAssets();
                Debug.Log("Successfully added SobelOutlineFeature to URP Renderer.");
            }
            else
            {
                Debug.LogWarning("FullScreenPassRendererFeature not found. You may need to add it manually or your URP version is older.");
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
