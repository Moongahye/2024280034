#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

public static class CharacterGalleryRefinedImporter
{
    [MenuItem("Tools/Character Gallery/Build 3D Exhibit Prefabs (Refined)")]
    public static void BuildPrefabs()
    {
        string folder = FindFolder();
        if (string.IsNullOrEmpty(folder)) { Debug.LogError("Character Gallery: 'Models' folder not found. Put entire Character_Gallery_3D_Props_REFINED inside Assets first."); return; }
        string models = folder + "/Models";
        string textures = folder + "/Textures";
        string output = folder + "/Prefabs";
        if (!AssetDatabase.IsValidFolder(output)) AssetDatabase.CreateFolder(folder, "Prefabs");
        bool urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
        int made = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Model", new[]{models}))
        {
            string modelPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!modelPath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase)) continue;
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (asset == null) continue;
            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null) continue;
            instance.name = Path.GetFileNameWithoutExtension(modelPath);
            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material[] existing = renderer.sharedMaterials;
                for (int i = 0; i < existing.Length; i++)
                {
                    string n = existing[i] != null ? existing[i].name.ToLowerInvariant().Replace(" (instance)", "") : "";
                    string stem = instance.name.ToLowerInvariant();
                    string image = null;
                    if (stem.StartsWith("standee_"))
                    {
                        string id = stem.Substring(8);
                        if (id == "shark_main") id = "shark_main";
                        image = id + "_standee.png";
                        if (!n.Contains("standee_tex")) image = null;
                    }
                    else if (stem.StartsWith("board_") && n.Contains("sheet_tex")) image = stem + "_sheet.png";
                    else if (stem.StartsWith("info_") && n.Contains("board_tex")) image = stem + "_board.png";
                    else if (n.Contains("cloth_tex")) image = stem + "_banner.png";
                    else if (n.Contains("plaque_tex")) image = stem + "_sheet.png";
                    if (!string.IsNullOrEmpty(image))
                    {
                        string imagePath = textures + "/" + image;
                        Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
                        if (t == null) t = AssetDatabase.LoadAssetAtPath<Texture2D>(models + "/" + image);
                        if (t != null)
                        {
                            var mat = new Material(Shader.Find(urp ? "Universal Render Pipeline/Lit" : "Standard"));
                            mat.name = instance.name + "_" + n + "_AutoMat";
                            if (urp) mat.SetTexture("_BaseMap", t); else mat.SetTexture("_MainTex", t);
                            if (stem.StartsWith("standee_"))
                            {
                                // Alpha Clipping, avoids ugly white border from transparent images.
                                if (urp) { mat.SetFloat("_Surface", 0); mat.SetFloat("_AlphaClip", 1); mat.SetFloat("_Cutoff", 0.28f); mat.EnableKeyword("_ALPHATEST_ON"); }
                                else { mat.SetFloat("_Mode", 1f); mat.SetFloat("_Cutoff", .28f); mat.SetOverrideTag("RenderType", "TransparentCutout"); mat.EnableKeyword("_ALPHATEST_ON"); mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest; }
                                mat.SetInt("_Cull", 0);
                            }
                            var matPath = folder + "/" + instance.name + "_" + n + ".mat";
                            AssetDatabase.CreateAsset(mat, AssetDatabase.GenerateUniqueAssetPath(matPath));
                            existing[i] = mat;
                        }
                    }
                }
                renderer.sharedMaterials = existing;
            }
            string prefabPath = output + "/" + instance.name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            UnityEngine.Object.DestroyImmediate(instance);
            made++;
        }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("Character Gallery: Created " + made + " editable prefabs at " + output);
    }
    static string FindFolder()
    {
        foreach (var d in AssetDatabase.GetSubFolders("Assets"))
        {
            if (d.EndsWith("Character_Gallery_3D_Props_REFINED")) return d;
        }
        string[] paths = AssetDatabase.FindAssets("t:DefaultAsset")
            .Select(AssetDatabase.GUIDToAssetPath).ToArray();
        return paths.FirstOrDefault(p => p.EndsWith("/Character_Gallery_3D_Props_REFINED"));
    }
}
#endif
