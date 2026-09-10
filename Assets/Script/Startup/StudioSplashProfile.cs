using UnityEngine;

[CreateAssetMenu(fileName = "StudioSplashProfile", menuName = "E-Lab Prime/Studio Splash Profile")]
public class StudioSplashProfile : ScriptableObject
{
    [Header("Flow")]
    public string nextSceneName = "MainMenu";
    public bool showEveryLaunch = true;
    public bool showOncePerVersion;

    [Header("Timing")]
    public float minimumDuration = 4.2f;
    public float skippableAfterSeconds = 1f;
    public float fadeInSeconds = 0.6f;
    public float holdSeconds = 2.2f;
    public float fadeOutSeconds = 0.7f;

    [Header("Brand")]
    public Sprite logo;
    public AudioClip stinger;
    public Color backgroundColor = new Color(0.015f, 0.032f, 0.055f, 1f);
    public Color accentColor = new Color(0.18f, 0.77f, 0.71f, 1f);
}
