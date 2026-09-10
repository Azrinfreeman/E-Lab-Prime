using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using P = KidFriendlyPalette;

public static class KidFriendlySceneColourSetup
{
    public static string Path(Transform t) => t.parent == null ? t.name : Path(t.parent) + "/" + t.name;
    private static Transform[] Nodes(Scene s) => s.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
    private static bool Protected(string path)
    {
        string p = path.ToLowerInvariant();
        return p.Contains("/water") || p.Contains("/air_visual") || p.Contains("/bikar") || p.Contains("/basin") ||
            p.Contains("/pemanas_visual") || p.Contains("/bunsen") || p.Contains("/lampu_") || p.Contains("/battery") ||
            p.Contains("/testtube") || p.Contains("/candle") || p.Contains("/glasses") || p.Contains("/laserbox") ||
            p.Contains("/measurer") || p.Contains("/targetpoint") || p.Contains("/retort") || p.Contains("/protractor") ||
            p.Contains("/jamrandik/") || p.Contains("/graphplotarea/gameobject/") || p.EndsWith("/leftimage");
    }
    private static Color Accent(string scene) => scene.Contains("2") || scene.Contains("6") ? P.Mint : scene.Contains("3") || scene.Contains("7") ? P.Lavender : scene.Contains("4") || scene.Contains("8") ? P.Yellow : P.Blue;
    private static bool SurfaceSprite(Image i)
    {
        string p = AssetDatabase.GetAssetPath(i.sprite);
        return i.sprite == null || p.Contains("/btn_") && !p.Contains("close") || p.EndsWith("pb_rect_rounded.png");
    }
    private static Color? ImageRole(Image i, Scene s)
    {
        string p = Path(i.transform), n = i.name.ToLowerInvariant();
        if (Protected(p)) return null;
        if (s.name == "Boot")
        {
            if (n.Contains("logoimage") || n.Contains("logosheen")) return null;
            if (n == "background" || n == "fadecurtain") return P.Sky;
            Color c = n.Contains("progress") ? P.Mint : n.Contains("particle") ? P.Lavender : P.Blue;
            c.a = i.color.a; return c;
        }
        if (p == "Canvas/bg" || p == "Canvas/background") return Color.white;
        if (p.Contains("LoadingSpinner")) { var c=P.Blue; c.a=i.color.a; return c; }
        if (n == "panel_loadingsreen") return P.Sky;
        if (n == "overlay") return new Color(P.Sky.r,P.Sky.g,P.Sky.b,0.35f);
        if (n == "handle") return P.Ink;
        if (n == "fill" || n == "progressfill") return P.Mint;
        if (n == "background" && i.GetComponentInParent<Slider>() != null) return P.Neutral;
        if (n == "outline") return new Color(P.Ink.r,P.Ink.g,P.Ink.b,0.12f);
        var button = i.GetComponent<Button>();
        if (button != null)
        {
            if (AssetDatabase.GetAssetPath(i.sprite).Contains("close")) return null; // keep illustrated close glyph
            string label=n+" "+string.Join(" ",button.GetComponentsInChildren<TMP_Text>(true).Select(t=>t.text)).ToLowerInvariant();
            if (label.Contains("reset") || label.Contains("padam") || label.Contains("henti") || label.Contains("close") || label.Contains("keluar")) return P.Coral;
            if (label.Contains("graph") || label.Contains("graf") || label.Contains("jadual")) return P.Blue;
            if (label.Contains("view") || label.Contains("camera")) return P.Lavender;
            return P.Yellow;
        }
        if (i.GetComponent<TMP_InputField>() != null || i.GetComponent<InputField>() != null) return P.Slot;
        if (!SurfaceSprite(i)) return null;
        if (i.color.a == 0) return null; // transparent raycast/layout surface
        if (p.Contains("/Title") || n == "header") return Accent(s.name);
        if (n.StartsWith("ans") || n.Contains("input")) return P.Slot;
        if (p.Contains("/Jadual/")) return n=="image (3)" || n=="image (5)" ? P.Slot : P.Surface;
        if (p.Contains("/Graph") && (n == "image" || n.Contains("line"))) return P.Ink;
        return P.Surface;
    }

    public static JObject Apply(Scene scene, bool dryRun = true)
    {
        if (!KidFriendlySceneTheme.Includes(scene)) throw new InvalidOperationException("Scene outside approved allowlist: " + scene.path);
        if (Application.isPlaying) throw new InvalidOperationException("Apply in Edit mode only");
        if (scene.name == "MainMenu") return new JObject { ["scene"]=scene.name,["status"]="baseline preserved" };
        var all=Nodes(scene); var manifest=new JArray();
        if (!all.Any(t=>t.GetComponent<Canvas>()!=null)) throw new InvalidOperationException("Expected Canvas missing: "+scene.name);
        var rounded=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/pb_rect_rounded.png");
        if (rounded==null) throw new InvalidOperationException("Rounded UI sprite missing");
        if (!dryRun) foreach(var root in scene.GetRootGameObjects()) Undo.RegisterFullObjectHierarchyUndo(root,"Linked scene palette");
        foreach(var t in all)
        {
            string path=Path(t);
            var img=t.GetComponent<Image>();
            if (img!=null)
            {
                var role=ImageRole(img,scene);
                manifest.Add(new JObject { ["path"]=path,["id"]=t.GetSiblingIndex(),["kind"]="Image",["action"]=role.HasValue?"colour":"preserve",["colour"]=role.HasValue?ColorUtility.ToHtmlStringRGBA(role.Value):ColorUtility.ToHtmlStringRGBA(img.color) });
                if (!dryRun && role.HasValue)
                {
                    img.color=role.Value;
                    if (img.name=="Panel_LoadingSreen") { img.sprite=null; img.type=Image.Type.Simple; }
                    else if (SurfaceSprite(img) && img.sprite!=null && !AssetDatabase.GetAssetPath(img.sprite).Contains("close"))
                    { img.sprite=rounded; img.type=Image.Type.Sliced; img.pixelsPerUnitMultiplier=2; }
                    if(img.name=="Handle") { img.sprite=rounded; img.type=Image.Type.Simple; img.preserveAspect=true; }
                    EditorUtility.SetDirty(img);
                }
            }
            var tmp=t.GetComponent<TMP_Text>();
            bool uiText=tmp is TextMeshProUGUI || path.Contains("NotebookPanel");
            if(tmp!=null && uiText && !Protected(path))
            {
                manifest.Add(new JObject { ["path"]=path,["kind"]="Text",["action"]="colour" });
                if(!dryRun)
                {
                    tmp.color=P.Ink; tmp.enableVertexGradient=false;
                    tmp.outlineWidth=0; tmp.overrideColorTags=true;
                    if(t.name=="Placeholder")tmp.color=P.Muted;
                    EditorUtility.SetDirty(tmp);
                }
            }
            if(!dryRun && !Protected(path))
            {
                var legacy=t.GetComponent<UnityEngine.UI.Text>(); if(legacy!=null){legacy.color=P.Ink;EditorUtility.SetDirty(legacy);}
                var selectable=t.GetComponent<Selectable>();
                if(selectable!=null)
                {
                    var colors=selectable.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(.94f,.97f,1);
                    colors.pressedColor=new Color(.76f,.84f,.92f); colors.selectedColor=new Color(.84f,.91f,1);
                    colors.disabledColor=new Color(.72f,.76f,.8f,1); colors.colorMultiplier=1; selectable.colors=colors; EditorUtility.SetDirty(selectable);
                }
                foreach(var shadow in t.GetComponents<Shadow>()){shadow.effectColor=new Color(P.Ink.r,P.Ink.g,P.Ink.b,.12f);EditorUtility.SetDirty(shadow);}
            }
        }
        if(!dryRun)
        {
            StyleDecor(scene,all,manifest);
            foreach(var c in all.Select(t=>t.GetComponent<StudioSplashController>()).Where(c=>c!=null))
            {
                if(c.profile==null)throw new InvalidOperationException("Boot profile missing");
                const string profilePath="Assets/Branding/StudioSplash/Profiles/KidFriendlySplashProfile.asset";
                var copy=AssetDatabase.LoadAssetAtPath<StudioSplashProfile>(profilePath);
                if(copy==null){copy=UnityEngine.Object.Instantiate(c.profile);AssetDatabase.CreateAsset(copy,profilePath);}
                copy.backgroundColor=P.Sky;copy.accentColor=P.Blue;c.profile=copy;
                EditorUtility.SetDirty(copy);EditorUtility.SetDirty(c);
            }
            EditorSceneManager.MarkSceneDirty(scene);
        }
        Directory.CreateDirectory("output/linked-scene-colours");
        File.WriteAllText("output/linked-scene-colours/"+scene.name+(dryRun?"-dry-run":"-applied")+".json",manifest.ToString());
        return new JObject { ["scene"]=scene.name,["dryRun"]=dryRun,["mapped"]=manifest.Count(x=>(string)x["action"]=="colour"),["preserved"]=manifest.Count(x=>(string)x["action"]=="preserve"),["manifestRows"]=manifest.Count };
    }

    private static void StyleDecor(Scene scene,Transform[] all,JArray manifest)
    {
        const string folder="Assets/Materials/KidFriendlyScenes";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Materials","KidFriendlyScenes");
        foreach(var t in all)
        {
            string path=Path(t);Color? colour=null;string role="";
            if(path.StartsWith("Classroom Environment/Wall_")){colour=P.Sky;role="Sky";}
            else if(path=="Classroom Environment/Floor"){colour=P.Neutral;role="Neutral";}
            else if(path=="Classroom Environment/Desk_1" || path=="Classroom Environment/Desk_2" || path=="Classroom Environment/TeacherDeskTop"){colour=P.Surface;role="Surface";}
            else if(path=="Classroom Environment/Cabinet_1" || path=="Classroom Environment/Cabinet_2"){colour=Accent(scene.name);role=scene.name+"Accent";}
            else if(t.name=="TabletCasing"){colour=P.Blue;role="Blue";}
            else if(t.name=="TabletScreen"){colour=P.Surface;role="Surface";}
            else if(t.name=="NotebookCover"){colour=P.Lavender;role="Lavender";}
            var r=t.GetComponent<MeshRenderer>(); if(!colour.HasValue || r==null)continue;
            Undo.RecordObject(r,"Decorative palette variant");
            var mats=r.sharedMaterials;
            for(int i=0;i<mats.Length;i++)
            {
                var original=mats[i]; if(original==null)throw new InvalidOperationException("Missing decorative material: "+path);
                string source=AssetDatabase.GetAssetPath(original);
                string target=source.StartsWith(folder+"/") ? source : folder+"/"+AssetDatabase.AssetPathToGUID(source)+"_"+role+".mat";
                if(string.IsNullOrEmpty(source))throw new InvalidOperationException("Unsaved decorative material: "+path);
                var clone=AssetDatabase.LoadAssetAtPath<Material>(target);
                if(clone==null){clone=new Material(original);AssetDatabase.CreateAsset(clone,target);}
                var authored=KidFriendlySceneTheme.MaterialColor(colour.Value);
                if(clone.HasProperty("_BaseColor"))clone.SetColor("_BaseColor",authored);
                if(clone.HasProperty("_Color"))clone.SetColor("_Color",authored);
                EditorUtility.SetDirty(clone); mats[i]=clone;
            }
            r.sharedMaterials=mats;EditorUtility.SetDirty(r);
            manifest.Add(new JObject{["path"]=path,["kind"]="DecorativeMaterialVariant",["action"]="colour"});
        }
    }

    // Render-only preview: operates on an isolated preview scene; never saves temporary Canvas changes.
    public static string Preview(string scenePath,string label,int width=1280,int height=720)
    {
        var s=EditorSceneManager.OpenPreviewScene(scenePath);RenderTexture rt=null;Texture2D tex=null;Camera camera=null;
        try
        {
            var all=Nodes(s);camera=all.Select(t=>t.GetComponent<Camera>()).FirstOrDefault(c=>c!=null);
            if(camera==null){var go=new GameObject("ReviewCamera");SceneManager.MoveGameObjectToScene(go,s);camera=go.AddComponent<Camera>();camera.backgroundColor=P.Sky;camera.clearFlags=CameraClearFlags.SolidColor;}
            if(label.Contains("lab-view")) foreach(var t in all.Where(t=>t.name=="Panel_LoadingSreen"))t.gameObject.SetActive(false);
            foreach(var canvas in all.Select(t=>t.GetComponent<Canvas>()).Where(c=>c!=null&&c.renderMode==RenderMode.ScreenSpaceOverlay))
            {canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=camera.nearClipPlane+1;}
            camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(s);
            rt=new RenderTexture(width,height,24);camera.targetTexture=rt;
            Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;
            try{RenderTexture.active=rt;tex=new Texture2D(width,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();}
            finally{RenderTexture.active=previous;}
            string file="output/linked-scene-colours/"+s.name+"-"+label+".png";File.WriteAllBytes(file,tex.EncodeToPNG());return file;
        }
        finally{if(camera!=null)camera.targetTexture=null;if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);if(rt!=null)UnityEngine.Object.DestroyImmediate(rt);EditorSceneManager.ClosePreviewScene(s);}
    }
}
