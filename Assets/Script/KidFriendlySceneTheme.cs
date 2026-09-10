using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Explicit adoption of the approved linked-scene palette; no global per-frame recolouring.</summary>
public static class KidFriendlySceneTheme
{
    public static readonly string[] Scenes = { "Boot", "MainMenu", "Login", "Lab1_3D", "Lab2_3D", "Lab3_3D", "Lab4_3D", "Lab5_3D", "Lab6_0", "Lab7", "Lab8" };
    public static bool Includes(Scene scene) => Array.IndexOf(Scenes, scene.name) >= 0 && scene.path == "Assets/Scenes/" + scene.name + ".unity";
    public static bool Active(Component owner) => owner != null && Includes(owner.gameObject.scene);
    public static bool Current => Includes(SceneManager.GetActiveScene());
    // Material properties expect authored sRGB, unlike these UI vertex values in this project.
    public static Color MaterialColor(Color ui) => QualitySettings.activeColorSpace == ColorSpace.Linear ? ui.gamma : ui;
    public static void SetFeedback(TMPro.TMP_Text target, string message)
    {
        if (target == null) { Debug.LogWarning("Feedback text target missing"); return; }
        if (Active(target))
        {
            target.color = message.Contains("<color=green>") ? KidFriendlyPalette.Success : KidFriendlyPalette.Error;
            target.overrideColorTags = true;
        }
        target.text = message;
    }
    public static Color Feedback(Component owner, Color original)
    {
        if (!Active(owner)) return original;
        Color.RGBToHSV(original, out float hue, out float saturation, out float value);
        Color mapped = saturation < 0.18f ? KidFriendlyPalette.Ink :
            hue < 0.07f || hue > 0.94f ? KidFriendlyPalette.Error :
            hue < 0.19f ? KidFriendlyPalette.Warning :
            hue < 0.48f ? KidFriendlyPalette.Success : KidFriendlyPalette.Ink;
        mapped.a = original.a;
        return mapped;
    }
}
