#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
public static class Lab2TextbookSetup
{
    public static void Apply(Scene scene)
    {
        if(scene.path!="Assets/Scenes/Lab2_3D.unity" || EditorApplication.isPlaying)throw new System.InvalidOperationException("Lab2 edit mode required");
        var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
        var book=all.Single(t=>t.name=="Lab2_Notebook");
        var result=book.GetComponent<Lab2TextbookResults>()??book.gameObject.AddComponent<Lab2TextbookResults>();
        result.leftPage=book.Find("LeftPageText").GetComponent<TextMesh>();
        result.rightPage=book.Find("RightPageText").GetComponent<TextMesh>();
        result.ShowResults(false);
        foreach(var text in new[]{result.leftPage,result.rightPage})
        {
            text.fontSize=48;text.color=Color.black;
            text.transform.localScale=Vector3.one*.0018f;
        }
        var manager=all.Select(t=>t.GetComponent<ElectrolysisManager3D>()).Single(m=>m!=null);
        manager.textbookResults=result;EditorUtility.SetDirty(manager);EditorUtility.SetDirty(result);
        ScienceNotebookAssetSetup.Apply(scene);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    }
}
#endif

