using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public static event Action OnEconomyUpdated;

    private const string XP_PREF_KEY = "PlayerXP";
    private const string CREDITS_PREF_KEY = "PlayerCredits";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("EconomyManager");
            obj.AddComponent<EconomyManager>();
            // Awake will set Instance and DontDestroyOnLoad
        }
    }

    public int GetXP()
    {
        return PlayerPrefs.GetInt(XP_PREF_KEY, 0);
    }

    public int GetLevel()
    {
        // Simple leveling curve: 1000 XP per level. Level 1 is the starting level.
        return (GetXP() / 1000) + 1;
    }

    public int GetCredits()
    {
        return PlayerPrefs.GetInt(CREDITS_PREF_KEY, 0);
    }

    public void AddXP(int amount)
    {
        int currentXP = GetXP();
        PlayerPrefs.SetInt(XP_PREF_KEY, currentXP + amount);
        PlayerPrefs.Save();
        OnEconomyUpdated?.Invoke();
    }

    public void AddCredits(int amount)
    {
        int currentCredits = GetCredits();
        PlayerPrefs.SetInt(CREDITS_PREF_KEY, currentCredits + amount);
        PlayerPrefs.Save();
        OnEconomyUpdated?.Invoke();
    }
}
