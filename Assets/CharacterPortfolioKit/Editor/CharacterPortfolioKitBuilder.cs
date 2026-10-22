#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.IO;

// Import the entire Assets/CharacterPortfolioKit directory into your Unity project.
// Menu: Tools > Character Gallery > Build Editable Exhibit Prefabs
public static class CharacterPortfolioKitBuilder
{
    const string Root = "Assets/CharacterPortfolioKit";
    const string Out = Root + "/GeneratedPrefabs";
    const string MatPath = Root + "/GeneratedMaterials";

    class Exhibit
    {
        public string id, title; public Color tint; public float height;
        public Exhibit(string i, string t, string c, float h) { id = i; title = t; tint = Parse(c); height = h; }
    }
    static Exhibit[] exhibits = {
        new Exhibit("01_Red", "01 Crimson Talisman", "#862933", 1.87f),
        new Exhibit("02_Thorn", "02 Thorn Archive", "#503B5F", 1.78f),
        new Exhibit("03_Wing", "03 Nocturne Wing", "#6C2A46", 1.95f),
        new Exhibit("04_Agent", "04 Ashen Agent", "#4D5257", 1.85f),
        new Exhibit("05_Shark", "05 Tidal Hunter - MAIN", "#265B65", 2.02f),
    };

    static Color Parse(string hex) { Color c; ColorUtility.TryParseHtmlString(hex, out c); return c; }
    static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/'); string cur = parts[0];
        for(int i=1;i<parts.Length;i++) { string next=cur+"/"+parts[i]; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]); cur = next; }
    }
    static Texture2D Texture(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/" + name);
    static Material Material(string name, Color color, string textureName=null, bool alphaCutout=false)
    {
        var path=MatPath+"/"+name+".mat";
        var old=AssetDatabase.LoadAssetAtPath<Material>(path);
        if (old!=null) return old;
        Shader shader=null;
        if (alphaCutout)
            shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Transparent Cutout") ?? Shader.Find("Standard");
        else shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");
        var m=new Material(shader); m.name=name;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor",color);
        if (m.HasProperty("_Color")) m.SetColor("_Color",color);
        if (textureName!=null)
        {
            var tex=Texture(textureName);
            if (tex==null) Debug.LogWarning("CharacterGallery missing texture: "+textureName);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap",tex);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex",tex);
        }
        if (alphaCutout)
        {
            // Alpha clipping keeps each PNG's silhouette while preserving opaque surface depth.
            if (m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip",1);
            if (m.HasProperty("_Cutoff")) m.SetFloat("_Cutoff",0.42f);
            m.EnableKeyword("_ALPHATEST_ON");
            if (m.HasProperty("_Cull")) m.SetFloat("_Cull",0);  // two-sided
            m.renderQueue=(int)RenderQueue.AlphaTest;
        }
        AssetDatabase.CreateAsset(m,path); return m;
    }
    static GameObject RootGO(string name, Transform parent)
    {
        var o=new GameObject(name);if(parent) o.transform.SetParent(parent,false); return o;
    }
    static GameObject Cube(string name,Transform parent, Vector3 pos, Vector3 scale,Material mat)
    {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;
        o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=scale;
        o.GetComponent<Renderer>().sharedMaterial=mat;return o;
    }
    static GameObject Quad(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, float tilt=0)
    {
        var o=GameObject.CreatePrimitive(PrimitiveType.Quad);o.name=name;
        o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=scale;
        o.transform.localRotation=Quaternion.Euler(tilt,0,0);
        o.GetComponent<Renderer>().sharedMaterial=mat;
        var collider=o.GetComponent<Collider>();if(collider) Object.DestroyImmediate(collider);
        return o;
    }
    static void DisableCollider(GameObject go){var col=go.GetComponent<Collider>();if(col) Object.DestroyImmediate(col);}
    static GameObject Build(Exhibit e)
    {
        var wood=Material("M_WalnutWood",Parse("#34251F"));
        var gold=Material("M_AgedGold",Parse("#C19A5D"));
        var black=Material("M_StandBlack",Parse("#28272B"));
        var tint=Material("M_Accent_"+e.id,e.tint);
        var sheetName=e.id=="05_Shark" ? e.id+"_ProcessSheet.jpg" : e.id+"_FullSheet.jpg";
        var artwork=Material("M_Sheet_"+e.id,Color.white,sheetName);
        var info=Material("M_Info_"+e.id,Color.white,e.id+"_InfoCard.png");
        var cutout=Material("M_StandeeCutout_"+e.id,Color.white,e.id+"_Cutout.png",true);

        var root=RootGO("THEMED_EXHIBIT_"+e.title,null);
        var wall=RootGO("WALL_ARTWORK__Move_to_Wall",root.transform);
        wall.transform.localPosition=new Vector3(0,0,0);
        float panelWidth=e.id=="05_Shark"?1.40f:2.64f;
        float panelHeight=e.id=="05_Shark"?1.98f:1.485f;
        float centerY=1.35f;
        Cube("Dark_Wood_Back",wall.transform,new Vector3(0,centerY,0),new Vector3(panelWidth+.27f,panelHeight+.27f,.085f),wood);
        Cube("Golden_Inner_Trim",wall.transform,new Vector3(0,centerY,.057f),new Vector3(panelWidth+.1f,panelHeight+.1f,.02f),gold);
        Quad("YOUR_ORIGINAL_CHARACTER_SHEET",wall.transform,new Vector3(0,centerY,.075f),new Vector3(panelWidth,panelHeight,1),artwork);
        var placard=RootGO("INFORMATION_STAND__Move_In_Front",root.transform);
        placard.transform.localPosition=new Vector3(1.05f,0,1.0f);
        Cube("Foot",placard.transform,new Vector3(0,.045f,0),new Vector3(.54f,.09f,.42f),black);
        Cube("Pole",placard.transform,new Vector3(0,.52f,0),new Vector3(.055f,.96f,.055f),wood);
        var angled=RootGO("Tilted_Board",placard.transform);angled.transform.localPosition=new Vector3(0,1.03f,.03f);angled.transform.localRotation=Quaternion.Euler(25f,0,0);
        Cube("Accent_Backboard",angled.transform,Vector3.zero,new Vector3(.95f,.65f,.03f),tint);
        Cube("Gold_Trim",angled.transform,new Vector3(0,0,.022f),new Vector3(.89f,.59f,.01f),gold);
        Quad("EDIT_THIS_INFO_TEXTURE_IN_PNG",angled.transform,new Vector3(0,0,.036f),new Vector3(.84f,.54f,1f),info);
        var stand=RootGO("OPTIONAL_STANDEE__Move_to_Desired_Spot",root.transform);
        stand.transform.localPosition=new Vector3(-1.17f,0,1.2f);
        var masktex=Texture(e.id+"_Cutout.png");
        float ratio=masktex ? (float)masktex.width/masktex.height : .75f;
        float standWidth=e.height*ratio;
        Quad("TRANSPARENT_ILLUSTRATION__FRONT",stand.transform,new Vector3(0,.12f+e.height/2f,0),new Vector3(standWidth,e.height,1),cutout);
        Cube("Floor_Base",stand.transform,new Vector3(0,.04f,0),new Vector3(Mathf.Max(.58f,standWidth+.14f),.08f,.48f),black);
        Cube("Base_Brass_Edge",stand.transform,new Vector3(0,.083f,0),new Vector3(Mathf.Max(.59f,standWidth+.16f),.012f,.49f),gold);
        Cube("Rear_Support",stand.transform,new Vector3(0,.55f,-.12f),new Vector3(.052f,1f,.053f),black);
        // Standee collision volume. Remove or disable if you want visitors to walk through it.
        var c=stand.AddComponent<BoxCollider>();c.center=new Vector3(0,.12f+e.height/2f,0);c.size=new Vector3(standWidth,e.height,.14f);
        var rug=Cube("OPTIONAL_THEME_RUG__Disable_If_Unwanted",root.transform,new Vector3(0,.006f,1.0f),new Vector3(3.5f,.015f,2.3f),tint);
        DisableCollider(rug);
        return root;
    }
    [MenuItem("Tools/Character Gallery/1. Build Editable Exhibit Prefabs")]
    public static void MakePrefabs()
    {
        Folder(Out);Folder(MatPath);AssetDatabase.Refresh();
        foreach(var e in exhibits)
        {
            var instance=Build(e);
            string filename=Out+"/"+e.id+"_ExhibitKit.prefab";
            PrefabUtility.SaveAsPrefabAsset(instance,filename);
            Object.DestroyImmediate(instance);
        }
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Debug.Log("Character Gallery: 5 editable exhibit prefabs created in "+Out);
        EditorUtility.DisplayDialog("Character Gallery","5 exhibition prefabs generated! Drag the prefab for each character into your room. Wall board, info stand and optional standee are separate child objects.","OK");
    }
    [MenuItem("Tools/Character Gallery/2. Place All Kits In A Row (Preview)")]
    public static void PlaceStaging()
    {
        if (!AssetDatabase.IsValidFolder(Out)) MakePrefabs();
        var parent=RootGO("CHARACTER_EXHIBITS_STAGING__Move_Each_RoomSet",Selection.activeTransform);
        Undo.RegisterCreatedObjectUndo(parent,"Place character exhibit staging");
        for(int i=0;i<exhibits.Length;i++)
        {
            string p=Out+"/"+exhibits[i].id+"_ExhibitKit.prefab";
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if(!source)continue;
            var inst=(GameObject)PrefabUtility.InstantiatePrefab(source);
            Undo.RegisterCreatedObjectUndo(inst,"Place exhibit kit");
            inst.transform.SetParent(parent.transform,false);
            inst.transform.localPosition=new Vector3((i-2)*4.5f,0,0);
        }
        Selection.activeGameObject=parent;
        Debug.Log("Move each THEMED_EXHIBIT to its room; this preview row is NOT a prealigned layout for your building.");
    }
}
#endif
