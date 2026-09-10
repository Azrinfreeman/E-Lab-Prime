using System;
using UnityEngine;

public enum ProfessorGuideContext
{
    FirstLaunchWelcome,
    SignedOutPrompt,
    SignedInWelcome,
    StartExperiments,
    Settings,
    Achievements,
    IdleHint
}


public enum ProfessorAvatarPose
{
    Idle,
    Talk,
    Point,
    Smile
}
[Serializable]
public class ProfessorDialogueLine
{
    public ProfessorGuideContext context;
    public ProfessorAvatarPose avatarPose = ProfessorAvatarPose.Idle;
    [TextArea(2, 4)] public string malayText;
    [TextArea(1, 3)] public string englishSubtitle;
    public string targetKey;
    public AudioClip voiceClip;
    public bool skippable = true;
}

[Serializable]
public class ProfessorGuideTarget
{
    public string key;
    public UnityEngine.UI.Button button;
    public UnityEngine.UI.Image highlight;
}
