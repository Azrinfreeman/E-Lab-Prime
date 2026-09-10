#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor.SceneManagement;
public static class ScienceNotebookPreview
{
    public static string Capture(int lab,bool angled=false,int width=1200,int height=900)
    {
        var s=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Lab"+lab+"_3D.unity");
        try{
            var book=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Single(t=>t.name=="LeftPageText").parent;
            var go=new GameObject("BookReviewCamera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,s);var cam=go.AddComponent<Camera>();
            cam.transform.position=book.position+book.up*.65f-book.forward*(angled?.38f:0);cam.transform.LookAt(book.position,book.forward);cam.orthographic=true;cam.orthographicSize=.27f;cam.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(s);
            var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);var old=RenderTexture.active;
            try{cam.targetTexture=rt;cam.Render();foreach(var fit in book.GetComponentsInChildren<ScienceNotebookPageFit>())fit.Fit();cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();string file="output/science-notebook/Lab"+lab+(angled?"-angled":"-top")+".jpg";System.IO.File.WriteAllBytes(file,tex.EncodeToJPG(85));return file;}
            finally{cam.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
        }finally{EditorSceneManager.ClosePreviewScene(s);}
    }
}
#endif
