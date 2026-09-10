using System.Collections.Generic;
using UnityEngine;
// Reflows existing voice controls beside the book only while the textbook camera is selected.
public sealed class ScienceNotebookReadingLayout : MonoBehaviour
{
    public RectTransform voiceCard;
    struct RectState {public RectTransform t;public Vector2 min,max,pivot,pos,size;public bool active;}
    readonly List<RectState> saved=new List<RectState>();
        bool reading;
        bool graphLabelsFitted;
    void Start()
    {
        foreach(var canvas in GetComponentsInChildren<Canvas>(true))
        {
            if(canvas.renderMode!=RenderMode.WorldSpace)continue;
            canvas.transform.localPosition=new Vector3(-.094f,.036f,-.15f);
            canvas.transform.localRotation=Quaternion.Euler(90,0,0);
        }
    }
    void LateUpdate()
    {
        if(graphLabelsFitted)return;
        var graph=transform.Find("GraphContainer");if(graph==null){graphLabelsFitted=true;return;}
        var labels=graph.GetComponentsInChildren<TextMesh>();if(labels.Length<3)return;
        foreach(var label in labels){var size=label.GetComponent<MeshRenderer>().localBounds.size;if(size.x<=0)return;float width=label.name=="YLabel"?.065f:.125f;label.transform.localScale=Vector3.one*Mathf.Min(.0018f,width/size.x);}
        graphLabelsFitted=true;
    }
    public void SetReading(bool value)
    {
        if(voiceCard==null || value==reading)return;
        reading=value;
        if(value){
            saved.Clear();foreach(var t in voiceCard.GetComponentsInChildren<RectTransform>(true))saved.Add(new RectState{t=t,min=t.anchorMin,max=t.anchorMax,pivot=t.pivot,pos=t.anchoredPosition,size=t.sizeDelta,active=t.gameObject.activeSelf});
            voiceCard.anchorMin=voiceCard.anchorMax=new Vector2(0,1);voiceCard.pivot=new Vector2(0,1);voiceCard.anchoredPosition=new Vector2(20,-150);voiceCard.sizeDelta=new Vector2(220,340);
            Set("Transcript",new Vector2(0,-12),new Vector2(204,164),true);
            Set("Status",Vector2.zero,Vector2.zero,false);
            Set("Replay",new Vector2(0,-202),new Vector2(200,40),true);
            Set("Stop",new Vector2(0,-252),new Vector2(200,40),true);
            Set("Mute",new Vector2(0,-302),new Vector2(200,40),true);
        }else{foreach(var s in saved)if(s.t!=null){s.t.anchorMin=s.min;s.t.anchorMax=s.max;s.t.pivot=s.pivot;s.t.sizeDelta=s.size;s.t.anchoredPosition=s.pos;s.t.gameObject.SetActive(s.active);}saved.Clear();}
    }
    void Set(string name,Vector2 pos,Vector2 size,bool active){var t=voiceCard.Find(name) as RectTransform;if(t==null)return;t.gameObject.SetActive(active);t.anchoredPosition=pos;t.sizeDelta=size;}
}


