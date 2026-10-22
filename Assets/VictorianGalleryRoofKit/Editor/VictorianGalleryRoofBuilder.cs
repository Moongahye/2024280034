// Victorian Gallery Roof Kit — Unity editor helper
// Compatible with Unity 2021.3, 2022 LTS, and Unity 6 (Built-in/URP/HDRP material fallbacks).
// Uses OBJ model files, not a screenshot. Roof geometry is baked in LOCAL coordinates
// relative to the same GALLERY_ANCHOR used by PortfolioGalleryExtensionBuilder.
// Does not change or destroy the existing gallery or previous exhibit prefabs.
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class VictorianGalleryRoofBuilder
{
    const string RootName = "VICTORIAN_ROOF__Toggle_Off_For_Edit";
    const string AssetRoot = "Assets/VictorianGalleryRoofKit";

    [MenuItem("Tools/Portfolio Gallery/Add Victorian Roof (4 sections)", true)]
    static bool CanAdd() { return Selection.activeTransform != null; }

    [MenuItem("Tools/Portfolio Gallery/Add Victorian Roof (4 sections)")]
    static void AddRoof()
    {
        Transform anchor = Selection.activeTransform;
        if (anchor == null) return;
        if (anchor.name != "GALLERY_ANCHOR" && anchor.name != "NEW_WING__LibraryGallery")
        {
            if (!EditorUtility.DisplayDialog("Check the roof anchor",
                "Select your GALLERY_ANCHOR (the same one used when creating the gallery), or its NEW_WING__LibraryGallery child.\n\nSelected: " + anchor.name + "\n\nCreate anyway using the selected object as the roof anchor?",
                "Create Anyway", "Cancel")) return;
        }
        if ((anchor.lossyScale - Vector3.one).sqrMagnitude > .0001f)
            Debug.LogWarning("Victorian Roof: Use an anchor with world scale (1,1,1); current world scale: " + anchor.lossyScale);

        Transform old = anchor.Find(RootName);
        if (old != null)
        {
            if (!EditorUtility.DisplayDialog("Existing roof found", "Replace the existing ROOF ONLY? No rooms or art will be changed.", "Replace roof", "Cancel")) return;
            Undo.DestroyObjectImmediate(old.gameObject);
        }

        Material slate = FindOrMakeMaterial("Roof_Slate", new Color(.25f,.27f,.31f),
            "Slate_Roof_Tiles.png", .0f, .20f);
        Material walnut = FindOrMakeMaterial("Roof_Walnut", new Color(.56f,.50f,.47f),
            "Walnut_Gable.png", .0f, .25f);
        Material iron = FindOrMakeMaterial("Roof_Iron", new Color(.11f,.12f,.14f),
            null, .62f, .34f);
        Material brass = FindOrMakeMaterial("Roof_Brass", new Color(.51f,.34f,.17f),
            null, .72f, .45f);

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Add Victorian Gallery Roof");
        root.transform.SetParent(anchor, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        string[] pieces = {
            "01_GrandHall", "02_TwinGalleryWing", "03_CorridorLink", "04_OriginalEntrance_OPTIONAL"
        };
        foreach (string key in pieces)
        {
            GameObject section = new GameObject(key + (key.Contains("OPTIONAL") ? "__ADJUST_TO_OLD_ROOM" : ""));
            section.transform.SetParent(root.transform, false);
            section.transform.localPosition = Vector3.zero;
            section.transform.localRotation = Quaternion.identity;
            section.transform.localScale = Vector3.one;
            AddPiece(section.transform, key, "Slate", slate);
            AddPiece(section.transform, key, "Wood", walnut);
            AddPiece(section.transform, key, "Iron", iron);
            AddPiece(section.transform, key, "Brass", brass);
            if (key.Contains("OPTIONAL")) section.SetActive(false);
        }

        EditorSceneManager.MarkSceneDirty(anchor.gameObject.scene);
        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
        Debug.Log("Victorian Roof created! 01-03 match the original generated wing. 04 is an OPTIONAL estimate for the earlier entrance room: position/scale manually then enable. Disable VICTORIAN_ROOF while editing from the top.");
    }

    [MenuItem("Tools/Portfolio Gallery/Toggle Victorian Roof Visibility")]
    static void ToggleRoof()
    {
        GameObject roof = null;
        if (Selection.activeTransform != null)
        {
            Transform tr = Selection.activeTransform;
            Transform current = tr;
            while (current != null)
            {
                if (current.name == RootName) { roof = current.gameObject; break; }
                Transform child = current.Find(RootName);
                if (child != null) { roof = child.gameObject; break; }
                current = current.parent;
            }
        }
        if (roof == null) roof = GameObject.Find("GALLERY_ANCHOR/" + RootName);
        if (roof == null) { EditorUtility.DisplayDialog("Roof not found", "Select the roof group or GALLERY_ANCHOR and try again.", "OK"); return; }
        Undo.RecordObject(roof, "Toggle Victorian Roof");
        roof.SetActive(!roof.activeSelf);
        EditorSceneManager.MarkSceneDirty(roof.scene);
    }

    static Material FindOrMakeMaterial(string name, Color tint, string textureName, float metal, float smooth)
    {
        string folder = AssetRoot + "/GeneratedMaterials";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(AssetRoot, "GeneratedMaterials");
        string path = folder + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = null;
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline != null && pipeline.GetType().Name.Contains("HDRenderPipeline")) shader = Shader.Find("HDRP/Lit");
            else if (pipeline != null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            mat = new Material(shader);
            mat.name = name;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metal);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smooth);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smooth);
            if (textureName != null)
            {
                string texPath = AssetRoot + "/Textures/" + textureName;
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null)
                {
                    // UVs intentionally exceed 0..1 to repeat shingles/wood.
                    TextureImporter imp = AssetImporter.GetAtPath(texPath) as TextureImporter;
                    if (imp != null && imp.wrapMode != TextureWrapMode.Repeat)
                    {
                        imp.wrapMode = TextureWrapMode.Repeat;
                        imp.SaveAndReimport();
                    }
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                }
                else Debug.LogWarning("Texture not found: " + texPath);
            }
            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();
        }
        return mat;
    }

    static void AddPiece(Transform parent, string key, string type, Material material)
    {
        string path = AssetRoot + "/Models/" + key + "_" + type + ".obj";
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (model == null) { Debug.LogError("Missing roof OBJ: " + path); return; }
        GameObject part = (GameObject)PrefabUtility.InstantiatePrefab(model);
        part.name = key + "_" + type;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = Vector3.zero;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = Vector3.one;
        foreach (Renderer r in part.GetComponentsInChildren<Renderer>(true))
        {
            var arr = r.sharedMaterials;
            for (int i=0;i<arr.Length;i++) arr[i] = material;
            r.sharedMaterials = arr;
            r.shadowCastingMode = ShadowCastingMode.On;
            r.receiveShadows = true;
        }
        // Cosmetic roof: no physical collision; existing gallery ceilings provide collision if enabled.
    }
}
#endif
