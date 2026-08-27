using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public static class CreateStatusBarPrefab
{
    public static void Create()
    {
        string path = "Assets/_Prototype/Resources/LifeStatusBarPrefab.prefab";
        
        GameObject root = new GameObject("LifeStatusBar");
        
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(1.2f, 0.2f); // slightly wider than 1 tile
        rootRT.localPosition = Vector3.zero;

        // Health Bar (Top)
        GameObject hpBgObj = new GameObject("HP_BG");
        hpBgObj.transform.SetParent(root.transform, false);
        Image hpBg = hpBgObj.AddComponent<Image>();
        hpBg.color = new Color(0.2f, 0, 0, 0.8f);
        RectTransform hpBgRT = hpBgObj.GetComponent<RectTransform>();
        hpBgRT.anchorMin = new Vector2(0, 1);
        hpBgRT.anchorMax = new Vector2(1, 1);
        hpBgRT.pivot = new Vector2(0.5f, 1);
        hpBgRT.sizeDelta = new Vector2(0, 0.08f);
        hpBgRT.anchoredPosition = new Vector2(0, 0);

        GameObject hpFillObj = new GameObject("HP_Fill");
        hpFillObj.transform.SetParent(hpBgObj.transform, false);
        Image hpFill = hpFillObj.AddComponent<Image>();
        hpFill.color = new Color(1f, 0.2f, 0.2f, 1f);
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        hpFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        hpFill.fillAmount = 1f;
        RectTransform hpFillRT = hpFillObj.GetComponent<RectTransform>();
        hpFillRT.anchorMin = new Vector2(0, 0);
        hpFillRT.anchorMax = new Vector2(1, 1);
        hpFillRT.pivot = new Vector2(0.5f, 0.5f);
        hpFillRT.sizeDelta = new Vector2(0, 0);
        hpFillRT.anchoredPosition = new Vector2(0, 0);

        // Stamina Bar (Bottom, thinner)
        GameObject spBgObj = new GameObject("SP_BG");
        spBgObj.transform.SetParent(root.transform, false);
        Image spBg = spBgObj.AddComponent<Image>();
        spBg.color = new Color(0, 0.2f, 0, 0.8f);
        RectTransform spBgRT = spBgObj.GetComponent<RectTransform>();
        spBgRT.anchorMin = new Vector2(0, 1);
        spBgRT.anchorMax = new Vector2(1, 1);
        spBgRT.pivot = new Vector2(0.5f, 1);
        spBgRT.sizeDelta = new Vector2(0, 0.04f); // Thinner than HP
        spBgRT.anchoredPosition = new Vector2(0, -0.09f); // Below HP

        GameObject spFillObj = new GameObject("SP_Fill");
        spFillObj.transform.SetParent(spBgObj.transform, false);
        Image spFill = spFillObj.AddComponent<Image>();
        spFill.color = new Color(0.2f, 1f, 0.2f, 1f);
        spFill.type = Image.Type.Filled;
        spFill.fillMethod = Image.FillMethod.Horizontal;
        spFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        spFill.fillAmount = 1f;
        RectTransform spFillRT = spFillObj.GetComponent<RectTransform>();
        spFillRT.anchorMin = new Vector2(0, 0);
        spFillRT.anchorMax = new Vector2(1, 1);
        spFillRT.pivot = new Vector2(0.5f, 0.5f);
        spFillRT.sizeDelta = new Vector2(0, 0);
        spFillRT.anchoredPosition = new Vector2(0, 0);

        // Add script
        root.AddComponent<TDG0407._prototype._prototype_LifeStatusBar>();

        if (!System.IO.Directory.Exists("Assets/_Prototype/Resources"))
            System.IO.Directory.CreateDirectory("Assets/_Prototype/Resources");

        PrefabUtility.SaveAsPrefabAsset(root, path);
        GameObject.DestroyImmediate(root);
        Debug.Log("Created " + path);
    }
}
