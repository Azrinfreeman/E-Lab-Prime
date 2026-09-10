using UnityEngine;
using TMPro;

public class AmaliPanelStatsUI : MonoBehaviour
{
    private TextMeshProUGUI statsText;

    private void Awake()
    {
        statsText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        EconomyManager.OnEconomyUpdated += UpdateStatsUI;
        UpdateStatsUI(); // Initial update when enabled
    }

    private void OnDisable()
    {
        EconomyManager.OnEconomyUpdated -= UpdateStatsUI;
    }

    private void UpdateStatsUI()
    {
        if (statsText == null) return;
        
        int xp = EconomyManager.Instance != null ? EconomyManager.Instance.GetXP() : 0;
        int level = EconomyManager.Instance != null ? EconomyManager.Instance.GetLevel() : 1;
        int credits = EconomyManager.Instance != null ? EconomyManager.Instance.GetCredits() : 0;

        string labelColour = gameObject.scene.name == "MainMenu" ? ColorUtility.ToHtmlStringRGB(KidFriendlyPalette.Muted) : "aaaaaa";
        statsText.text = $"<color=#{labelColour}>CREDITS:</color> {credits}   <color=#{labelColour}>LVL {level}:</color> {xp} XP";
    }
}
