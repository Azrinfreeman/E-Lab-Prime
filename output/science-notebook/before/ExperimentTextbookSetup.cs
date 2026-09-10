#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
public static class ExperimentTextbookSetup
{
    public static void Apply(Scene scene)
    {
        int lab=scene.name=="Lab3_3D"?3:scene.name=="Lab4_3D"?4:scene.name=="Lab5_3D"?5:0;
        if(lab==0 || EditorApplication.isPlaying)throw new System.InvalidOperationException("Lab3/4/5 edit mode required");
        var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
        foreach(string name in new[]{"LeftPageText","RightPageText"})
        {
            var page=all.Single(t=>t.name==name).GetComponent<TextMesh>();
            page.text=name=="LeftPageText"?ExperimentTextbookContent.Left(lab):ExperimentTextbookContent.Right(lab);
            page.anchor=TextAnchor.MiddleCenter;page.alignment=TextAlignment.Left;
            var left=all.Single(t=>t.name=="LeftPageText");var pos=page.transform.localPosition;pos.z=left.localPosition.z;page.transform.localPosition=pos;
            page.fontSize=48;page.color=Color.black;page.transform.localScale=Vector3.one*.0016f;
            EditorUtility.SetDirty(page);
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    }
}
#endif

