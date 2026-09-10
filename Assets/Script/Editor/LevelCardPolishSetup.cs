#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class LevelCardPolishSetup
{
    static readonly string[] Titles={"Ayunan Bandul","Elektrolisis Air","Pemanasan Air","Penyejatan Air","Pelarutan Gula","Komposisi Udara","Pantulan Cahaya","Pembiasan Cahaya"};
    static readonly string[] Descs={"Panjang tali & tempoh ayunan","Kenali hidrogen & oksigen","Bandingkan takat didih","Terokai faktor penyejatan","Suhu, kacauan & saiz zarah","Kaji peranan oksigen","Sudut tuju & sudut pantulan","Lihat arah cahaya berubah"};
    static Color Ink=>KidFriendlyPalette.Ink;
    public static void Apply()
    {
        var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(s.name!="MainMenu"||EditorApplication.isPlaying)throw new System.InvalidOperationException("MainMenu edit mode required");
        var cards=s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<AmaliCardProgressUI>(true)).OrderBy(c=>c.labIndex).ToArray();
        if(cards.Length!=8)throw new System.InvalidOperationException("Expected eight cards");
        foreach(var c in cards)
        {
            int i=c.labIndex-1;var t=c.transform;var accent=new[]{KidFriendlyPalette.Blue,KidFriendlyPalette.Mint,KidFriendlyPalette.Lavender,KidFriendlyPalette.Yellow}[i%4];
            var font=t.Find("TextContainer/Title").GetComponent<TMP_Text>().font;
            foreach(var l in t.GetComponentsInChildren<LayoutGroup>(true))l.enabled=false;
            foreach(Transform child in t)child.gameObject.SetActive(false);
            var old=t.Find("PolishedContent");if(old!=null)Object.DestroyImmediate(old.gameObject);
            var bg=t.GetComponent<Image>();bg.color=Color.Lerp(KidFriendlyPalette.Surface,accent,.08f);
            var root=Rect("PolishedContent",t,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var stripe=Rect("Accent",root,new Vector2(0,.14f),new Vector2(0,.86f),new Vector2(5,0),new Vector2(5,0));stripe.gameObject.AddComponent<Image>().color=accent;
            var tile=Rect("Diagram",root,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(64,0),new Vector2(86,88));
            var ti=tile.gameObject.AddComponent<Image>();ti.sprite=bg.sprite;ti.type=Image.Type.Sliced;ti.pixelsPerUnitMultiplier=3;ti.color=Color.Lerp(KidFriendlyPalette.Surface,accent,.3f);ti.raycastTarget=false;
            Draw(tile,i,accent);
            Label("Number",root,font,"AMALI "+(i+1).ToString("00"),13,new Vector2(120,-12),new Vector2(-16,-31),KidFriendlyPalette.Muted);
            Label("Title",root,font,Titles[i],23,new Vector2(120,-32),new Vector2(-16,-61),Ink);
            Label("Desc",root,font,Descs[i],15,new Vector2(120,-63),new Vector2(-16,-85),KidFriendlyPalette.Muted);
            var st=Label("Status",root,font,"MULA",12,new Vector2(120,-94),new Vector2(-150,-115),Ink);c.statusText=st;
            var pct=Label("Percent",root,font,"0%",12,new Vector2(0,-94),new Vector2(-18,-115),Ink);pct.alignment=TextAlignmentOptions.Right;
            ((RectTransform)pct.transform).anchorMin=new Vector2(.82f,1);c.percentageText=pct;
            var track=Rect("Track",root,new Vector2(0,0),new Vector2(1,0),Vector2.zero,Vector2.zero);track.offsetMin=new Vector2(120,12);track.offsetMax=new Vector2(-18,18);track.gameObject.AddComponent<Image>().color=KidFriendlyPalette.Neutral;
            var fill=Rect("Fill",track,Vector2.zero,new Vector2(0,1),Vector2.zero,Vector2.zero);fill.gameObject.AddComponent<Image>().color=accent;c.fillRect=fill;
            foreach(var graphic in root.GetComponentsInChildren<Graphic>())graphic.raycastTarget=false;
            var button=t.GetComponent<Button>();var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(.92f,.96f,1);colors.pressedColor=new Color(.8f,.88f,.96f);button.colors=colors;
            c.UpdateProgress();EditorUtility.SetDirty(c);
        }
        EditorSceneManager.MarkSceneDirty(s);
    }
    static RectTransform Rect(string name,Transform p,Vector2 min,Vector2 max,Vector2 pos,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=min;r.anchorMax=max;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
    static TextMeshProUGUI Label(string name,Transform p,TMP_FontAsset font,string text,float size,Vector2 min,Vector2 max,Color color){var r=Rect(name,p,new Vector2(0,1),new Vector2(1,1),Vector2.zero,Vector2.zero);r.offsetMin=new Vector2(min.x,max.y);r.offsetMax=new Vector2(max.x,min.y);var l=r.gameObject.AddComponent<TextMeshProUGUI>();l.font=font;l.text=text;l.fontSize=size;l.enableAutoSizing=true;l.fontSizeMin=size-3;l.fontSizeMax=size;l.color=color;l.fontStyle=name=="Title"?FontStyles.Bold:FontStyles.Normal;return l;}
    static void Line(Transform p,float x,float y,float w,float h,Color c,float angle=0){var r=Rect("Shape",p,Vector2.one*.5f,Vector2.one*.5f,new Vector2(x,y),new Vector2(w,h));r.localEulerAngles=new Vector3(0,0,angle);r.gameObject.AddComponent<Image>().color=c;}
    static void Draw(Transform p,int i,Color c)
    {
        if(i==0){Line(p,0,27,54,4,Ink);for(int j=-1;j<=1;j++){Line(p,j*19,5,2,40,Ink,-j*15);Line(p,j*24,-18,12,12,c);}return;}
        if(i>=6){Line(p,0,-10,60,3,Ink);Line(p,-17,8,3,46,c,-40);Line(p,17,i==6?8:-23,3,46,Ink,i==6?40:-25);return;}
        Line(p,-24,0,3,48,Ink);Line(p,24,0,3,48,Ink);Line(p,0,-24,50,3,Ink);Line(p,0,-8,44,26,c);
        if(i==1){Line(p,-10,20,3,35,Ink);Line(p,10,20,3,35,Ink);Line(p,-10,0,4,4,Color.white);Line(p,10,-8,4,4,Color.white);}
        if(i==2){Line(p,0,-34,18,6,KidFriendlyPalette.Coral);Line(p,6,17,3,27,Ink);}
        if(i==3){for(int j=-1;j<=1;j++)Line(p,j*13,29,2,16,Ink,j*10);}
        if(i==4){Line(p,9,20,3,34,Ink,-20);Line(p,-10,-4,7,7,Color.white);Line(p,6,-16,5,5,Color.white);}
        if(i==5){Line(p,0,0,10,29,KidFriendlyPalette.Surface);Line(p,0,20,6,10,KidFriendlyPalette.Coral);}
    }
}
#endif

