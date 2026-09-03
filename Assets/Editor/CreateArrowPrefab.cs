using UnityEngine;
using UnityEditor;

public class CreateArrowPrefab
{
    [MenuItem("Tools/Create Arrow Prefab")]
    public static void Execute()
    {
        // 1. Create Root GameObject
        GameObject arrowRoot = new GameObject("ArrowProjectile");
        
        // Add ProjectileView script (Assuming it exists and is TDG0407._prototype._prototype_ProjectileView)
        var projView = arrowRoot.AddComponent<TDG0407._prototype._prototype_ProjectileView>();

        // 2. Create Visual Root (To allow rotation independent of logical movement)
        GameObject visualRoot = new GameObject("Visual");
        visualRoot.transform.SetParent(arrowRoot.transform, false);
        // Arrow flies along Z-axis. Let's orient it so that Z is forward.
        // Wait, standard Unity primitives.
        
        // --- Shaft ---
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        Object.DestroyImmediate(shaft.GetComponent<Collider>());
        shaft.transform.SetParent(visualRoot.transform, false);
        shaft.transform.localScale = new Vector3(0.05f, 0.4f, 0.05f); // 0.8 units long
        shaft.transform.localRotation = Quaternion.Euler(90, 0, 0); // Point along Z
        
        // --- Arrowhead ---
        // Cone is not a default primitive. We can use a stretched cube or just a smaller cylinder, or a sphere.
        // A diamond shape made from a rotated cube is good.
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        Object.DestroyImmediate(head.GetComponent<Collider>());
        head.transform.SetParent(visualRoot.transform, false);
        head.transform.localScale = new Vector3(0.12f, 0.12f, 0.2f);
        head.transform.localRotation = Quaternion.Euler(0, 45, 0); 
        head.transform.localPosition = new Vector3(0, 0, 0.4f); // Front of shaft

        // --- Fletching (Feathers) ---
        GameObject feather1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        feather1.name = "Feather1";
        Object.DestroyImmediate(feather1.GetComponent<Collider>());
        feather1.transform.SetParent(visualRoot.transform, false);
        feather1.transform.localScale = new Vector3(0.02f, 0.15f, 0.15f);
        feather1.transform.localPosition = new Vector3(0, 0, -0.35f);

        GameObject feather2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        feather2.name = "Feather2";
        Object.DestroyImmediate(feather2.GetComponent<Collider>());
        feather2.transform.SetParent(visualRoot.transform, false);
        feather2.transform.localScale = new Vector3(0.15f, 0.02f, 0.15f);
        feather2.transform.localPosition = new Vector3(0, 0, -0.35f);

        // Materials
        Material woodMat = new Material(Shader.Find("Standard"));
        woodMat.color = new Color(0.4f, 0.2f, 0.05f);
        shaft.GetComponent<Renderer>().sharedMaterial = woodMat;

        Material metalMat = new Material(Shader.Find("Standard"));
        metalMat.color = new Color(0.8f, 0.8f, 0.8f);
        metalMat.SetFloat("_Metallic", 0.8f);
        metalMat.SetFloat("_Glossiness", 0.8f);
        head.GetComponent<Renderer>().sharedMaterial = metalMat;

        Material featherMat = new Material(Shader.Find("Standard"));
        featherMat.color = Color.white;
        feather1.GetComponent<Renderer>().sharedMaterial = featherMat;
        feather2.GetComponent<Renderer>().sharedMaterial = featherMat;

        // --- Trail Renderer ---
        GameObject trailObj = new GameObject("Trail");
        trailObj.transform.SetParent(arrowRoot.transform, false);
        trailObj.transform.localPosition = new Vector3(0, 0, -0.4f);
        
        TrailRenderer trail = trailObj.AddComponent<TrailRenderer>();
        trail.time = 0.2f;
        trail.startWidth = 0.1f;
        trail.endWidth = 0.0f;
        
        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        trailMat.color = new Color(1, 1, 1, 0.5f);
        trail.sharedMaterial = trailMat;

        // Save Prefab
        string localPath = "Assets/_Prototype/Prefabs/ArrowProjectile.prefab";
        PrefabUtility.SaveAsPrefabAsset(arrowRoot, localPath);
        Object.DestroyImmediate(arrowRoot);

        Debug.Log("ArrowProjectile Prefab created at " + localPath);
    }
}
