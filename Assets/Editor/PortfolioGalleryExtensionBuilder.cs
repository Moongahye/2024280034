// Portfolio Gallery Extension Builder — Unity 2021.3 LTS / 2022 LTS / Unity 6
// Editor-only generator. Put this script anywhere under Assets/Editor.
// Select an EMPTY GameObject at the old gallery entrance, its local +Z axis pointing
// INTO the old gallery, then use Tools > Portfolio Gallery > Create Connected Wing.
// The new wing extends along local -Z. The anchor's Y must match the existing floor TOP.
// Generated wall/floor/props are ordinary editable GameObjects with native colliders.

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.IO;

public static class PortfolioGalleryExtensionBuilder
{
    private const string GeneratedName = "NEW_WING__LibraryGallery";
    private const string AssetDir = "Assets/PortfolioGallery_Kit";
    private const float WallH = 4.3f;

    private static Material wall, wood, woodLight, darkWood, metal, gold, burgundy, plum, artBlue, artTan, artGreen, stone, ceiling;
    private static Transform architecture, floors, trim, props, lights, roofs;

    [MenuItem("Tools/Portfolio Gallery/Create Connected Wing", true)]
    private static bool CanCreate() { return Selection.activeTransform != null; }

    [MenuItem("Tools/Portfolio Gallery/Create Connected Wing")]
    public static void CreateConnectedWing()
    {
        Transform anchor = Selection.activeTransform;
        if (anchor == null) { EditorUtility.DisplayDialog("Choose anchor", "Select an empty anchor object first.", "OK"); return; }
        if (anchor.lossyScale != Vector3.one)
        {
            Debug.LogWarning("Gallery Builder: Anchor scale should be (1,1,1). Its world scale is " + anchor.lossyScale);
        }
        Transform previous = anchor.Find(GeneratedName);
        if (previous != null)
        {
            if (!EditorUtility.DisplayDialog("Replace previous extension?", "Existing extension found below the selected anchor. Replace it? (Undo supported)", "Replace", "Cancel")) return;
            Undo.DestroyObjectImmediate(previous.gameObject);
        }

        CreateMaterialAssets();
        GameObject root = new GameObject(GeneratedName);
        Undo.RegisterCreatedObjectUndo(root, "Create connected portfolio gallery");
        root.transform.SetParent(anchor, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        architecture = NewGroup(root.transform, "01_ARCHITECTURE__Walls");
        floors = NewGroup(root.transform, "02_FLOORS__WoodAndRugs");
        trim = NewGroup(root.transform, "03_TRIMS__DoorwaysAndMoulding");
        props = NewGroup(root.transform, "04_EXHIBITS__MoveOrReplace");
        lights = NewGroup(root.transform, "05_LIGHTS__WarmCeilingLights");
        roofs = NewGroup(root.transform, "06_CEILING__Enable_When_Ready");
        NewGroup(root.transform, "07_NOTES__Select_STAIRS_SLOT_to_Place_Model");

        BuildFloor();
        BuildWalls();
        BuildMouldingsAndDoorways();
        BuildExhibitionProps(root.transform);
        BuildLighting();
        BuildCeiling();

        // Off while editing so the top-down Scene view stays unobstructed.
        // Tick this GameObject in Inspector before filming/playing the final interior.
        roofs.gameObject.SetActive(false);
        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
        EditorSceneManagerMarkDirty();
        Debug.Log("PORTFOLIO GALLERY CREATED. +Z of anchor points into original room; wing extends toward -Z. Turn on 06_CEILING before final presentation. The existing room is NOT modified.");
    }

    private static void EditorSceneManagerMarkDirty()
    {
        if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private static Transform NewGroup(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static Material Mat(string file, Color color, float metallic = 0f, float smooth = 0.25f)
    {
        string path = AssetDir + "/Materials/" + file + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;
        RenderPipelineAsset rp = GraphicsSettings.currentRenderPipeline;
        Shader shader = null;
        if (rp != null && rp.GetType().Name.Contains("HDRenderPipeline")) shader = Shader.Find("HDRP/Lit");
        else if (rp != null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        Material mat = new Material(shader);
        mat.name = file;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smooth);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smooth);
        mat.enableInstancing = true;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static void CreateMaterialAssets()
    {
        if (!AssetDatabase.IsValidFolder(AssetDir)) AssetDatabase.CreateFolder("Assets", "PortfolioGallery_Kit");
        if (!AssetDatabase.IsValidFolder(AssetDir + "/Materials")) AssetDatabase.CreateFolder(AssetDir, "Materials");
        wall = Mat("01_Dark_Charcoal_Wallpaper__REPLACE_With_Your_Wall", new Color(.115f,.119f,.142f), 0f, .14f);
        wood = Mat("02_Parquet_Walnut_Floor__REPLACE_With_Your_Floor", new Color(.255f,.157f,.108f), 0f, .32f);
        woodLight = Mat("03_Oak_Border", new Color(.38f,.225f,.131f), 0f, .3f);
        darkWood = Mat("04_Ebony_Wood", new Color(.105f,.064f,.052f), 0f, .27f);
        metal = Mat("05_Black_Iron", new Color(.072f,.073f,.082f), .65f, .36f);
        gold = Mat("06_Antique_Brass", new Color(.61f,.397f,.172f), .76f, .52f);
        burgundy = Mat("07_Burgundy_Carpet", new Color(.245f,.052f,.082f), 0f, .18f);
        plum = Mat("08_Plum_Carpet", new Color(.25f,.15f,.249f), 0f, .18f);
        artBlue = Mat("09_Art_Placeholder_Blue", new Color(.28f,.43f,.53f), 0f, .5f);
        artTan = Mat("10_Art_Placeholder_Sepia", new Color(.58f,.41f,.27f), 0f, .38f);
        artGreen = Mat("11_Art_Placeholder_Sage", new Color(.31f,.43f,.34f), 0f, .38f);
        stone = Mat("12_Stone_Step", new Color(.32f,.315f,.297f), 0f, .3f);
        ceiling = Mat("13_Ceiling", new Color(.17f,.147f,.141f), 0f, .17f);
        AssetDatabase.SaveAssets();
    }

    private static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material material, bool collider = true)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = center;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    private static GameObject Cylinder(Transform parent, string name, Vector3 center, float diameter, float height, Material material, bool collider = true)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = center;
        go.transform.localScale = new Vector3(diameter, height * .5f, diameter); // native cylinder height is 2
        go.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    private static void BuildFloor()
    {
        // Ground elevation Y=0 corresponds to the old gallery's floor TOP.
        Box(floors, "LOBBY_FLOOR__22m_x_9m", new Vector3(0,-.15f,-14.5f), new Vector3(22,.30f,9), wood);
        Box(floors, "CENTRAL_CORRIDOR_FLOOR__4m_x_10m", new Vector3(0,-.15f,-5), new Vector3(4,.30f,10), wood);
        Box(floors, "LEFT_GALLERY_FLOOR", new Vector3(-6.5f,-.15f,-6), new Vector3(9,.30f,8), wood);
        Box(floors, "RIGHT_GALLERY_FLOOR", new Vector3(6.5f,-.15f,-6), new Vector3(9,.30f,8), wood);
        Box(floors, "ENTRANCE_PORCH_STONE", new Vector3(0,-.16f,-20.5f), new Vector3(5,.28f,3), stone);
        // Decorative floor bands that visually connect old corridor to the new lobby.
        Box(floors, "Runner_Burgundy_Corridor", new Vector3(0,.018f,-5), new Vector3(1.8f,.035f,9.3f), burgundy, false);
        Box(floors, "Runner_Burgundy_Lobby", new Vector3(0,.018f,-16.5f), new Vector3(2.1f,.035f,4.0f), burgundy, false);
        Box(floors, "LEFT_Rug_Plum", new Vector3(-6.5f,.018f,-6.3f), new Vector3(3.5f,.035f,2.6f), plum, false);
        Box(floors, "RIGHT_Rug_Plum", new Vector3(6.5f,.018f,-6.3f), new Vector3(3.5f,.035f,2.6f), plum, false);
        Box(floors, "LOBBY_Left_Rug", new Vector3(-6.8f,.018f,-14.6f), new Vector3(3.8f,.035f,3.0f), plum, false);
        Box(floors, "LOBBY_Right_Rug", new Vector3(6.8f,.018f,-14.6f), new Vector3(3.8f,.035f,3.0f), plum, false);
        // Subtle parquet joint strips; cheap but visibly structured with no textures required.
        for (int i = 0; i < 8; i++)
        {
            float z = -10.4f - i * 1.05f;
            Box(floors, "Lobby_Parquet_Seam_" + i, new Vector3(0,.004f,z), new Vector3(21.6f,.008f,.032f), darkWood, false);
        }
    }

    private static void WallX(string name, float x, float za, float zb)
    {
        float len = Mathf.Abs(zb - za);
        if (len <= .005f) return;
        Box(architecture, name, new Vector3(x,WallH*.5f,(za+zb)*.5f), new Vector3(.25f,WallH,len), wall);
        Box(trim, name+"_Baseboard", new Vector3(x,.13f,(za+zb)*.5f), new Vector3(.35f,.26f,len), darkWood, false);
        Box(trim, name+"_Crown", new Vector3(x,4.05f,(za+zb)*.5f), new Vector3(.39f,.16f,len), darkWood, false);
    }
    private static void WallZ(string name, float z, float xa, float xb)
    {
        float len = Mathf.Abs(xb - xa);
        if (len <= .005f) return;
        Box(architecture, name, new Vector3((xa+xb)*.5f,WallH*.5f,z), new Vector3(len,WallH,.25f), wall);
        Box(trim, name+"_Baseboard", new Vector3((xa+xb)*.5f,.13f,z), new Vector3(len,.26f,.35f), darkWood, false);
        Box(trim, name+"_Crown", new Vector3((xa+xb)*.5f,4.05f,z), new Vector3(len,.16f,.39f), darkWood, false);
    }

    private static void BuildWalls()
    {
        // Map: original room lies at local +Z, centered at Z=0.
        // Hall: x [-2,2], z [-10,0] with 3m doorway gaps into two rooms.
        WallX("Hall_Left_NearOldRoom", -2,-4.5f,0);
        WallX("Hall_Left_AfterDoor", -2,-10,-7.5f);
        WallX("Hall_Right_NearOldRoom", 2,-4.5f,0);
        WallX("Hall_Right_AfterDoor", 2,-10,-7.5f);
        // L/R themed gallery walls: x [-11,-2] and [2,11], z [-10,-2].
        WallX("LeftGallery_OuterWall", -11,-10,-2);
        WallX("RightGallery_OuterWall", 11,-10,-2);
        WallZ("LeftGallery_EndWall_ForPaintings",-2,-11,-2);
        WallZ("RightGallery_EndWall_ForPaintings",-2,2,11);
        WallZ("LeftGallery_BackWall",-10,-11,-2);
        WallZ("RightGallery_BackWall",-10,2,11);
        // Lobby connected directly at z=-10; gallery backwalls create separation.
        WallX("Lobby_Left_OuterWall",-11,-19,-10);
        WallX("Lobby_Right_OuterWall",11,-19,-10);
        // Main doorway is x[-2.5,2.5] in lobby front wall z=-19.
        WallZ("Lobby_Entrance_Left",-19,-11,-2.5f);
        WallZ("Lobby_Entrance_Right",-19,2.5f,11);
        // Keep actual holes unobstructed all the way to walking height.
        Box(architecture,"Gallery_Left_Door_Header",new Vector3(-2,3.82f,-6),new Vector3(.25f,.96f,3.0f),wall);
        Box(architecture,"Gallery_Right_Door_Header",new Vector3(2,3.82f,-6),new Vector3(.25f,.96f,3.0f),wall);
        Box(architecture,"Lobby_Entrance_Header",new Vector3(0,3.82f,-19),new Vector3(5,.96f,.25f),wall);
    }

    private static void DoorColumn(Transform p, string name, Vector3 c)
    {
        Box(p,name+"_Foot",new Vector3(c.x,.18f,c.z),new Vector3(.48f,.36f,.48f),gold,false);
        Box(p,name+"_Pillar",new Vector3(c.x,1.7f,c.z),new Vector3(.35f,3.1f,.35f),darkWood,false);
        Box(p,name+"_Head",new Vector3(c.x,3.28f,c.z),new Vector3(.53f,.19f,.53f),gold,false);
    }
    private static void BuildMouldingsAndDoorways()
    {
        // Decorative opening to the existing room: DOES NOT alter its front wall.
        DoorColumn(trim,"OldConnection_Left",new Vector3(-1.9f,0,-.08f));
        DoorColumn(trim,"OldConnection_Right",new Vector3(1.9f,0,-.08f));
        Box(trim,"OldConnection_TopBeam",new Vector3(0,3.55f,-.08f),new Vector3(4.05f,.26f,.48f),darkWood,false);
        Box(trim,"OldConnection_BrassLine",new Vector3(0,3.47f,-.33f),new Vector3(3.8f,.055f,.05f),gold,false);
        // Doors left/right lead into side rooms from the corridor.
        foreach (float x in new float[]{-2, 2})
        {
            DoorColumn(trim,(x<0?"Left":"Right")+"Door_North",new Vector3(x,0,-4.5f));
            DoorColumn(trim,(x<0?"Left":"Right")+"Door_South",new Vector3(x,0,-7.5f));
            Box(trim,(x<0?"Left":"Right")+"Door_Top",new Vector3(x,3.25f,-6),new Vector3(.51f,.18f,3.05f),gold,false);
        }
        // Lobby grand entrance frame / passage out to exterior (no actual door blocking passage).
        DoorColumn(trim,"Entrance_Left",new Vector3(-2.48f,0,-19));
        DoorColumn(trim,"Entrance_Right",new Vector3(2.48f,0,-19));
        Box(trim,"Entrance_Lintel",new Vector3(0,3.22f,-19),new Vector3(5.3f,.24f,.56f),gold,false);
        Box(trim,"Entry_StoneStep",new Vector3(0,.045f,-20.9f),new Vector3(5,.09f,1.2f),stone);
    }

    private static void Art(Transform parent, string name, Vector3 pos, Quaternion facing, Material picture)
    {
        Transform a = NewGroup(parent,name+"__ArtworkReplaceMe");
        a.localPosition=pos;
        a.localRotation=facing;
        Box(a,"Carved_Wood_Frame",Vector3.zero,new Vector3(1.95f,1.50f,.12f),darkWood,false);
        Box(a,"Antique_Brass_Trim",new Vector3(0,0,-.068f),new Vector3(1.84f,1.40f,.025f),gold,false);
        Box(a,"Artwork_Placeholder__ReplaceWithYourImage",new Vector3(0,0,-.09f),new Vector3(1.65f,1.20f,.025f),picture,false);
        Box(a,"Small_Title_Plaque",new Vector3(0,-.94f,-.045f),new Vector3(1.3f,.16f,.08f),gold,false);
    }

    private static void Bookcase(Transform parent, string name, Vector3 pos, bool faceRight)
    {
        Transform b=NewGroup(parent,name);
        b.localPosition=pos;
        if (faceRight) b.localRotation=Quaternion.Euler(0,180,0);
        Box(b,"Back",new Vector3(0,1.22f,0),new Vector3(2.25f,2.44f,.33f),darkWood);
        Box(b,"LeftPost",new Vector3(-1.06f,1.25f,-.11f),new Vector3(.16f,2.5f,.5f),woodLight);
        Box(b,"RightPost",new Vector3(1.06f,1.25f,-.11f),new Vector3(.16f,2.5f,.5f),woodLight);
        for(int i=0;i<4;i++)
        {
            float h=.46f+i*.53f;
            Box(b,"Shelf_"+i,new Vector3(0,h,-.27f),new Vector3(2.1f,.1f,.52f),woodLight);
            for(int j=0;j<11;j++)
            {
                float xx = -.88f+j*.17f;
                Material m = ((i+j)%5==0)?plum:((i+j)%3==0?burgundy:((i+j)%4==0?artGreen:darkWood));
                Box(b,"Book_"+i+"_"+j,new Vector3(xx,h+.25f,-.38f),new Vector3(.12f,.37f+((i+j)%3)*.045f,.22f),m,false);
            }
        }
        Box(b,"Cap",new Vector3(0,2.54f,-.12f),new Vector3(2.5f,.14f,.55f),gold,false);
    }

    private static void Bench(Transform parent,string name,float x,float z)
    {
        Transform b=NewGroup(parent,name);
        b.localPosition=new Vector3(x,0,z);
        Box(b,"Seat",new Vector3(0,.48f,0),new Vector3(2.5f,.15f,.62f),woodLight);
        for(int k=-1;k<=1;k+=2)
            Box(b,"Leg_"+k,new Vector3(k*1.0f,.23f,0),new Vector3(.15f,.46f,.52f),darkWood);
    }

    private static void BuildExhibitionProps(Transform extensionRoot)
    {
        // Gallery works facing corridor direction, on the wall z=-2.
        Art(props,"LEFT_Wall_Artwork_01",new Vector3(-8.45f,2.40f,-2.22f),Quaternion.identity,artTan);
        Art(props,"LEFT_Wall_Artwork_02",new Vector3(-4.7f,2.40f,-2.22f),Quaternion.identity,artBlue);
        Art(props,"RIGHT_Wall_Artwork_01",new Vector3(4.7f,2.40f,-2.22f),Quaternion.identity,artGreen);
        Art(props,"RIGHT_Wall_Artwork_02",new Vector3(8.45f,2.40f,-2.22f),Quaternion.identity,artTan);
        // Outer walls have secondary large artworks facing toward the middle of the rooms.
        Art(props,"LEFT_Side_Artwork",new Vector3(-10.79f,2.43f,-5.6f),Quaternion.Euler(0,-90,0),artGreen);
        Art(props,"RIGHT_Side_Artwork",new Vector3(10.79f,2.43f,-5.6f),Quaternion.Euler(0,90,0),artBlue);
        // Freestanding display panels against gallery back walls.
        Art(props,"LEFT_Back_Artwork",new Vector3(-7.3f,2.35f,-9.78f),Quaternion.Euler(0,180,0),artBlue);
        Art(props,"RIGHT_Back_Artwork",new Vector3(7.3f,2.35f,-9.78f),Quaternion.Euler(0,180,0),artGreen);
        Bookcase(props,"LEFT_Bookshelf_Near_Back",new Vector3(-4.6f,0,-9.50f),false);
        Bookcase(props,"RIGHT_Bookshelf_Near_Back",new Vector3(4.6f,0,-9.50f),false);
        Bench(props,"LEFT_Bench",-6.5f,-6.6f);
        Bench(props,"RIGHT_Bench",6.5f,-6.6f);
        // Lobby side feature displays, away from the circular stair slot.
        Art(props,"LOBBY_LEFT_01",new Vector3(-10.78f,2.4f,-15.0f),Quaternion.Euler(0,-90,0),artTan);
        Art(props,"LOBBY_RIGHT_01",new Vector3(10.78f,2.4f,-15.0f),Quaternion.Euler(0,90,0),artBlue);
        Art(props,"LOBBY_LEFT_02",new Vector3(-7.1f,2.4f,-18.77f),Quaternion.Euler(0,180,0),artGreen);
        Art(props,"LOBBY_RIGHT_02",new Vector3(7.1f,2.4f,-18.77f),Quaternion.Euler(0,180,0),artTan);
        // Small modular exhibition tables left/right of the lobby.
        foreach(float x in new float[]{-6.8f,6.8f})
        {
            Box(props,"Lobby_DisplayTable_Top_"+x,new Vector3(x,.85f,-13.6f),new Vector3(2.1f,.16f,1.1f),woodLight);
            Box(props,"Lobby_DisplayTable_Leg_"+x,new Vector3(x,.40f,-13.6f),new Vector3(.65f,.8f,.65f),darkWood);
        }
        // THIS IS A SLOT FOR THE ORNATE 3D STAIRCASE ASSET GENERATED EARLIER.
        // Drag the staircase model under this transform and reset its local Position.
        Transform slot = NewGroup(props,"STAIRS_SLOT__DRAG_YOUR_STAIRCASE_HERE");
        slot.localPosition = new Vector3(0,.14f,-14.45f);
        Cylinder(props,"Staircase_Round_Platform",new Vector3(0,.12f,-14.45f),4.7f,.24f,darkWood);
        Cylinder(props,"Staircase_Raised_Inner_Disk",new Vector3(0,.248f,-14.45f),4.3f,.03f,woodLight);
        // Central temporary centerpiece: can be hidden after placing staircase.
        Transform sample=NewGroup(props,"HIDE_AFTER_IMPORT__Staircase_Placement_Guide");
        Cylinder(sample,"Central_Guide_Pillar",new Vector3(0,1.0f,-14.45f),.22f,1.7f,gold,false);
        Cylinder(sample,"Top_Cap",new Vector3(0,1.88f,-14.45f),.46f,.12f,gold,false);
        // Location hint at original entrance, empty and nonblocking.
        Transform connector=NewGroup(extensionRoot,"ALIGN_NOTE__Anchor_Origin_Is_Existing_Door_Floor");
        connector.localPosition=Vector3.zero;
    }

    private static void CreatePointLight(Transform p,string name,Vector3 localPos,float range,float intensity)
    {
        GameObject go=new GameObject(name);
        go.transform.SetParent(p,false);
        go.transform.localPosition=localPos;
        Light l=go.AddComponent<Light>();
        l.type=LightType.Point;
        l.color=new Color(1.0f,.72f,.43f);
        l.intensity=intensity;
        l.range=range;
        l.shadows=LightShadows.None;
        // Small visual metal and brass ceiling housing.
        Cylinder(go.transform,"Lamp_Casing",new Vector3(0,.08f,0),.37f,.18f,metal,false);
        Cylinder(go.transform,"Lamp_Brass_Band",new Vector3(0,-.03f,0),.42f,.05f,gold,false);
    }

    private static void BuildLighting()
    {
        CreatePointLight(lights,"Corridor_WarmLamp",new Vector3(0,3.5f,-3.1f),7.0f,1.3f);
        CreatePointLight(lights,"Corridor_WarmLamp_2",new Vector3(0,3.5f,-8.4f),7.0f,1.3f);
        CreatePointLight(lights,"Left_Exhibition_WarmLamp",new Vector3(-6.6f,3.5f,-5.6f),8f,1.6f);
        CreatePointLight(lights,"Right_Exhibition_WarmLamp",new Vector3(6.6f,3.5f,-5.6f),8f,1.6f);
        CreatePointLight(lights,"Lobby_Left_WarmLamp",new Vector3(-6.2f,3.5f,-14.4f),9f,1.7f);
        CreatePointLight(lights,"Lobby_Right_WarmLamp",new Vector3(6.2f,3.5f,-14.4f),9f,1.7f);
        // Lighting is intentionally modest; adjust exposure to your existing room.
    }

    private static void BuildCeiling()
    {
        Box(roofs,"Hall_Ceiling",new Vector3(0,4.37f,-5f),new Vector3(4.1f,.2f,10.1f),ceiling);
        Box(roofs,"Left_Gallery_Ceiling",new Vector3(-6.5f,4.37f,-6f),new Vector3(9.1f,.2f,8.1f),ceiling);
        Box(roofs,"Right_Gallery_Ceiling",new Vector3(6.5f,4.37f,-6f),new Vector3(9.1f,.2f,8.1f),ceiling);
        Box(roofs,"Lobby_Ceiling",new Vector3(0,4.37f,-14.5f),new Vector3(22.1f,.2f,9.1f),ceiling);
        for(float x=-9.0f;x<=9f;x+=3.0f)
            Box(roofs,"Lobby_Ceiling_Beam_"+x,new Vector3(x,4.18f,-14.5f),new Vector3(.22f,.18f,8.8f),darkWood,false);
    }
}
#endif
