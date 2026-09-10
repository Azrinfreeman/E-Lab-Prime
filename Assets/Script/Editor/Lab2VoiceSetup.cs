#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static class Lab2VoiceSetup
{
    public static void Apply(Scene scene)
    {
        if(scene.path!="Assets/Scenes/Lab2_3D.unity" || EditorApplication.isPlaying) throw new InvalidOperationException("Lab2 edit mode required");
        var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
        var manager=all.Select(t=>t.GetComponent<Lab2TutorialManager>()).Single(t=>t!=null);
        var oldCard=all.FirstOrDefault(t=>t.name=="Lab2VoiceCard");
        if(oldCard!=null)UnityEngine.Object.DestroyImmediate(oldCard.gameObject);
        var canvasRoot=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="Lab2VoiceCanvas");
        if(canvasRoot==null){canvasRoot=new GameObject("Lab2VoiceCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));SceneManager.MoveGameObjectToScene(canvasRoot,scene);}
        var canvas=canvasRoot.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=5;
        var scaler=canvasRoot.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        var existing=canvas.transform.Find("Lab2VoiceCard");
        if(existing!=null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
        var card=Rect("Lab2VoiceCard",canvas.transform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-860),new Vector2(640,190));
        card.pivot=new Vector2(.5f,1);
        var background=card.gameObject.AddComponent<Image>();background.color=KidFriendlyPalette.Surface;background.raycastTarget=false;
        var controller=card.gameObject.AddComponent<Lab2VoiceInstructions>();
        controller.voice=card.gameObject.AddComponent<AudioSource>();controller.voice.playOnAwake=false;controller.voice.volume=.8f;
        var font=all.Where(t=>t!=null).Select(t=>t.GetComponent<TMP_Text>()).FirstOrDefault(t=>t!=null && t.font!=null)?.font;
        controller.transcript=Label("Transcript",card,new Vector2(0,-12),new Vector2(612,95),font,20,"Bersedia untuk eksperimen elektrolisis...");
        controller.transcript.enableAutoSizing=true;controller.transcript.fontSizeMin=16;controller.transcript.fontSizeMax=20;
        controller.status=Label("Status",card,new Vector2(0,-154),new Vector2(612,25),font,14,"Arahan suara");
        controller.replayButton=Button("Replay",card,-200,"Ulang Arahan",font,KidFriendlyPalette.Yellow);
        controller.stopButton=Button("Stop",card,0,"Hentikan suara",font,KidFriendlyPalette.Coral);
        controller.muteButton=Button("Mute",card,200,"Suara: Hidup",font,KidFriendlyPalette.Mint);
        controller.muteLabel=controller.muteButton.GetComponentInChildren<TMP_Text>();
        controller.loadingOverlay=all.FirstOrDefault(t=>t!=null && t.name=="Panel_LoadingSreen")?.gameObject;
        for(int i=0;i<3;i++)controller.clips[i]=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Lab2/Instructions/step_"+(i+1)+"_ms.mp3");
        manager.voiceInstructions=controller;
        EditorUtility.SetDirty(manager);EditorSceneManager.MarkSceneDirty(scene);
    }
    static RectTransform Rect(string name,Transform parent,Vector2 a,Vector2 b,Vector2 pos,Vector2 size)
    {var t=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();t.SetParent(parent,false);t.anchorMin=a;t.anchorMax=b;t.anchoredPosition=pos;t.sizeDelta=size;return t;}
    static TMP_Text Label(string name,Transform parent,Vector2 pos,Vector2 size,TMP_FontAsset font,float fontsize,string text)
    {var t=Rect(name,parent,new Vector2(.5f,1),new Vector2(.5f,1),pos,size);t.pivot=new Vector2(.5f,1);var label=t.gameObject.AddComponent<TextMeshProUGUI>();label.font=font;label.fontSize=fontsize;label.color=KidFriendlyPalette.Ink;label.text=text;label.raycastTarget=false;label.alignment=TextAlignmentOptions.Center;return label;}
    static Button Button(string name,Transform parent,float x,string text,TMP_FontAsset font,Color colour)
    {var t=Rect(name,parent,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(x,-128),new Vector2(188,40));var img=t.gameObject.AddComponent<Image>();img.color=colour;var button=t.gameObject.AddComponent<Button>();button.targetGraphic=img;Label("Label",t,new Vector2(0,-7),new Vector2(184,30),font,17,text);return button;}
}
#endif






