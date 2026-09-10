using UnityEngine;
// Fits updated notebook prose into its reserved region without changing content or experiment state.
public sealed class ScienceNotebookPageFit : MonoBehaviour
{
    public TextMesh page;
    public Vector2 area = new Vector2(.14f,.23f);
    string previous;
    void LateUpdate(){if(page!=null && previous!=page.text){Fit();previous=page.text;}}
    public void Fit()
    {
        if(page==null)return;
        var bounds=page.GetComponent<MeshRenderer>().localBounds;
        if(bounds.size.x<=0 || bounds.size.y<=0)return;
        float scale=Mathf.Min(area.x/bounds.size.x,area.y/bounds.size.y,.0022f);
        page.transform.localScale=Vector3.one*scale;
    }
}
