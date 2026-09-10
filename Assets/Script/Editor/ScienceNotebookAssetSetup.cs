#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static class ScienceNotebookAssetSetup
{
    const string Folder="Assets/Art/ScienceNotebook";
    const string PrefabPath="Assets/Prefabs/ScienceNotebook/ScienceNotebookOpen.prefab";
    class Shape
    {
        public List<Vector3> vertices=new List<Vector3>(); public List<int> triangles=new List<int>();
        public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=vertices.Count;vertices.AddRange(new[]{a,b,c,d});triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
        public void Tri(Vector3 a,Vector3 b,Vector3 c){int n=vertices.Count;vertices.AddRange(new[]{a,b,c});triangles.AddRange(new[]{n,n+1,n+2});}
        public void Block(float cx,float cz,float w,float h,float y,float depth,float radius)
        {
            var ring=new List<Vector2>();
            for(int corner=0;corner<4;corner++)for(int k=0;k<=6;k++)
            {float angle=(corner*90+k*15)*Mathf.Deg2Rad;float sx=corner==0||corner==3?1:-1;float sz=corner<2?1:-1;ring.Add(new Vector2(cx+sx*(w/2-radius)+Mathf.Cos(angle)*radius,cz+sz*(h/2-radius)+Mathf.Sin(angle)*radius));}
            for(int j=0;j<ring.Count;j++){var p=ring[j];var q=ring[(j+1)%ring.Count];var a=new Vector3(p.x,y,p.y);var b=new Vector3(q.x,y,q.y);var c=b+Vector3.up*depth;var d=a+Vector3.up*depth;Quad(a,d,c,b);Tri(new Vector3(cx,y+depth,cz),c,d);Tri(new Vector3(cx,y,cz),a,b);}
        }
        public Mesh Save(string name)
        {var m=new Mesh{name=name};m.SetVertices(vertices);m.SetTriangles(triangles,0);m.SetUVs(0,vertices.Select(v=>new Vector2((v.x+.19f)/.38f,(v.z+.22f)/.44f)).ToList());m.RecalculateNormals();m.RecalculateBounds();string p=Folder+"/Meshes/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(old!=null){EditorUtility.CopySerialized(m,old);UnityEngine.Object.DestroyImmediate(m);m=old;EditorUtility.SetDirty(m);}else AssetDatabase.CreateAsset(m,p);AssetDatabase.SaveAssetIfDirty(m);return m;}
    }
    static void Directory(string path){if(AssetDatabase.IsValidFolder(path))return;var parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');Directory(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));}
    static Material Mat(string name,Color color)
    {string p=Folder+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.12f);EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);return m;}
    static Transform Part(Transform root,string name,Mesh mesh,Material material){var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(root,false);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=material;return g.transform;}
    public static void Build()
    {
        Directory(Folder+"/Meshes");Directory(Folder+"/Materials");Directory("Assets/Prefabs/ScienceNotebook");
        var cover=new Shape();cover.Block(-.092f,0,.19f,.438f,0,.008f,.012f);cover.Block(.092f,0,.19f,.438f,0,.008f,.012f);
        // Rounded exposed spine beneath the recessed gutter.
        for(int n=0;n<16;n++){float a=Mathf.PI*n/16,b=Mathf.PI*(n+1)/16;cover.Quad(new Vector3(.016f*Mathf.Cos(a),.012f+.009f*Mathf.Sin(a),-.211f),new Vector3(.016f*Mathf.Cos(b),.012f+.009f*Mathf.Sin(b),-.211f),new Vector3(.016f*Mathf.Cos(b),.012f+.009f*Mathf.Sin(b),.211f),new Vector3(.016f*Mathf.Cos(a),.012f+.009f*Mathf.Sin(a),.211f));}
        var stacks=new Shape();foreach(int sign in new[]{-1,1})stacks.Block(sign*.091f,0,.16f,.413f,.008f,.015f,.007f);
        var paper=new Shape();foreach(int sign in new[]{-1,1})for(int n=0;n<24;n++)
        {float x=.008f+n*.163f/24,x2=.008f+(n+1)*.163f/24;float y=.033f-.011f*Mathf.Exp(-(x-.008f)*100),y2=.033f-.011f*Mathf.Exp(-(x2-.008f)*100);var a=new Vector3(sign*x,y,-.201f);var b=new Vector3(sign*x,y,.201f);var c=new Vector3(sign*x2,y2,.201f);var d=new Vector3(sign*x2,y2,-.201f);if(sign>0)paper.Quad(a,b,c,d);else paper.Quad(d,c,b,a);}
                // Close each page block along the front, back and outside edges.
        foreach(int sign in new[]{-1,1}){
            foreach(int end in new[]{-1,1})for(int n=0;n<24;n++){
                float x=.008f+n*.163f/24,x2=.008f+(n+1)*.163f/24;
                float y=.033f-.011f*Mathf.Exp(-(x-.008f)*100),y2=.033f-.011f*Mathf.Exp(-(x2-.008f)*100);
                paper.Quad(new Vector3(sign*x,.023f,end*.201f),new Vector3(sign*x,y,end*.201f),new Vector3(sign*x2,y2,end*.201f),new Vector3(sign*x2,.023f,end*.201f));
            }
            paper.Quad(new Vector3(sign*.171f,.023f,-.201f),new Vector3(sign*.171f,.033f,-.201f),new Vector3(sign*.171f,.033f,.201f),new Vector3(sign*.171f,.023f,.201f));
        }
        var edges=new Shape();foreach(int sign in new[]{-1,1})for(int j=0;j<11;j++)
        {float y=.009f+j*.00135f;edges.Quad(new Vector3(sign*.1711f,y,-.194f),new Vector3(sign*.1711f,y,.194f),new Vector3(sign*.1711f,y+.0002f,.194f),new Vector3(sign*.1711f,y+.0002f,-.194f));foreach(int end in new[]{-1,1})edges.Quad(new Vector3(sign*.017f,y,end*.2066f),new Vector3(sign*.165f,y,end*.2066f),new Vector3(sign*.165f,y+.0002f,end*.2066f),new Vector3(sign*.017f,y+.0002f,end*.2066f));}
        var detail=new Shape();detail.Block(.015f,-.212f,.009f,.058f,.009f,.001f,.001f);
        for(int j=0;j<12;j++)detail.Block(0,-.17f+j*.03f,.006f,.0006f,.0225f,.0004f,.0002f);
        var root=new GameObject("ScienceNotebookOpen");try{
            Part(root.transform,"Cover",cover.Save("CoverAndSpine"),Mat("ClothCover",new Color(.28f,.24f,.49f)));
            Part(root.transform,"PageStacks",stacks.Save("PageStacks"),Mat("PageEdges",new Color(.86f,.82f,.7f)));
            Part(root.transform,"OpenPages",paper.Save("CurvedOpenPages"),Mat("IvoryPaper",new Color(1f,.975f,.89f)));
            Part(root.transform,"PageEdgeLines",edges.Save("PageEdgeLines"),AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/PageEdges.mat"));
            Part(root.transform,"BindingAndRibbon",detail.Save("BindingAndRibbon"),Mat("Ribbon",new Color(.92f,.63f,.24f)));
            foreach(var name in new[]{"LeftContentAnchor","RightContentAnchor","ControlsAnchor"}){var g=new GameObject(name);g.transform.SetParent(root.transform,false);g.transform.localPosition=new Vector3(name.StartsWith("Left")?-.094f:.094f,.034f,0);}
                        var paperMat=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/IvoryPaper.mat");
            paperMat.shader=Shader.Find("Universal Render Pipeline/Unlit");paperMat.SetColor("_BaseColor",Color.white);
            Directory(Folder+"/Textures");
            string texturePath=Folder+"/Textures/PaperGrain.asset";
            var grain=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if(grain==null){grain=new Texture2D(256,256,TextureFormat.RGB24,false);grain.name="IvoryPaperGrain";AssetDatabase.CreateAsset(grain,texturePath);}
            var random=new System.Random(24);var pixels=new Color[256*256];
            for(int y=0;y<256;y++)for(int x=0;x<256;x++){float u=x/255f;float shade=1-.22f*Mathf.Exp(-Mathf.Abs(u-.5f)*70);float noise=(float)random.NextDouble()*.025f;pixels[y*256+x]=new Color(.985f-noise,.955f-noise,.86f-noise)*shade;}
            grain.SetPixels(pixels);grain.Apply();EditorUtility.SetDirty(grain);AssetDatabase.SaveAssetIfDirty(grain);paperMat.SetTexture("_BaseMap",grain);EditorUtility.SetDirty(paperMat);AssetDatabase.SaveAssetIfDirty(paperMat);
            PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
        }finally{UnityEngine.Object.DestroyImmediate(root);}
    }
    public static void Apply(Scene scene)
    {
        if(EditorApplication.isPlaying || !new[]{"Lab2_3D","Lab3_3D","Lab4_3D","Lab5_3D"}.Contains(scene.name))throw new InvalidOperationException("Approved notebook scene in edit mode required");
        var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
        var left=all.Single(t=>t.name=="LeftPageText");var book=left.parent;var right=book.Find("RightPageText");
        var old=book.Find("ScienceNotebookOpen");if(old!=null)Undo.DestroyObjectImmediate(old.gameObject);
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);if(prefab==null)throw new InvalidOperationException("Build notebook assets first");
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab,book);visual.name="ScienceNotebookOpen";
        // Keep the original cover renderer as the visible hover-highlight target.
        var cover=book.Find("NotebookCover");var source=visual.transform.Find("Cover");
        Undo.RecordObject(cover,"Notebook cover");cover.localPosition=Vector3.zero;cover.localRotation=Quaternion.identity;cover.localScale=Vector3.one;
        cover.GetComponent<MeshFilter>().sharedMesh=source.GetComponent<MeshFilter>().sharedMesh;cover.GetComponent<MeshRenderer>().sharedMaterial=source.GetComponent<MeshRenderer>().sharedMaterial;source.gameObject.SetActive(false);
        foreach(string name in new[]{"LeftPage","RightPage"})book.Find(name).GetComponent<MeshRenderer>().enabled=false;
                Label(visual.transform,"LeftHeader","MAKMAL SAINS  /  "+scene.name.Substring(0,4).ToUpper(),new Vector3(-.094f,.0345f,.174f),.00072f);
        Label(visual.transform,"RightHeader","PEMERHATIAN & HASIL",new Vector3(.094f,.0345f,.174f),.00072f);
        Label(visual.transform,"LeftNumber","01",new Vector3(-.094f,.0345f,-.188f),.0007f);
        Label(visual.transform,"RightNumber","02",new Vector3(.094f,.0345f,-.188f),.0007f);
        bool graph=book.Find("GraphContainer")!=null;
        LayoutPage(left,-.094f,0,.14f,.255f);LayoutPage(right,.094f,graph?.092f:0,.14f,graph?.17f:.265f);
        if(graph){var g=book.Find("GraphContainer");g.localPosition=new Vector3(.094f,.034f,-.105f);g.localRotation=Quaternion.identity;g.localScale=Vector3.one;var e=g.GetComponent<EvaporationGraph3D>();if(e!=null){e.zMin=-.055f;e.zMax=.025f;EditorUtility.SetDirty(e);}var q=g.GetComponent<SolubilityGraph3D>();if(q!=null){q.zMin=-.055f;q.zMax=.025f;EditorUtility.SetDirty(q);}}
        var pen=book.Find("Pen");if(pen!=null){pen.localPosition=new Vector3(.202f,.018f,0);pen.localRotation=Quaternion.Euler(90,0,0);foreach(Transform part in pen){part.localRotation=Quaternion.identity;}}
        foreach(var canvas in book.GetComponentsInChildren<Canvas>(true)){var t=canvas.transform;t.localPosition=new Vector3(-.094f,.036f,-.15f);t.localRotation=Quaternion.Euler(90,0,0);}
        var reading=book.GetComponent<ScienceNotebookReadingLayout>()??book.gameObject.AddComponent<ScienceNotebookReadingLayout>();reading.voiceCard=all.FirstOrDefault(t=>t!=null && t.name==scene.name.Substring(0,4)+"VoiceCard") as RectTransform;EditorUtility.SetDirty(reading);
        var collider=book.GetComponent<BoxCollider>();if(collider!=null){collider.center=new Vector3(0,.018f,0);collider.size=new Vector3(.376f,.036f,.438f);}
        EditorSceneManager.MarkSceneDirty(scene);
    }
        static void Label(Transform parent,string name,string words,Vector3 position,float scale)
    {var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localRotation=Quaternion.Euler(90,0,0);g.transform.localScale=Vector3.one*scale;var text=g.AddComponent<TextMesh>();text.fontSize=48;text.text=words;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=new Color(.22f,.25f,.28f);}
    static void LayoutPage(Transform t,float x,float z,float w,float h)
    {var text=t.GetComponent<TextMesh>();t.localPosition=new Vector3(x,.034f,z);t.localRotation=Quaternion.Euler(90,0,0);text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Left;text.fontSize=64;text.color=new Color(.08f,.12f,.17f);var fit=t.GetComponent<ScienceNotebookPageFit>()??t.gameObject.AddComponent<ScienceNotebookPageFit>();fit.page=text;fit.area=new Vector2(w,h);fit.Fit();EditorUtility.SetDirty(text);EditorUtility.SetDirty(fit);}
}
#endif






