using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManagerUI : MonoBehaviour
{
    [Header("Audio Sliders")]
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Graphics")]
    public TMP_Dropdown qualityDropdown;

    [Header("Data Management")]
    public Button resetProgressButton;
    
    [Header("Confirmation Panel")]
    public GameObject confirmationPanel;
    public Button confirmResetButton;
    public Button cancelResetButton;

    private void Start()
    {
        InitializeSettings();

        // Audio Listeners
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        // Graphics Listeners
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);

        // Data Reset Listener
        if (resetProgressButton != null)
            resetProgressButton.onClick.AddListener(RequestReset);

        if (confirmResetButton != null)
            confirmResetButton.onClick.AddListener(PerformReset);

        if (cancelResetButton != null)
            cancelResetButton.onClick.AddListener(CancelReset);
            
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    private void InitializeSettings()
    {
        // Load Audio Settings (0 to 1 float)
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        // Load Graphics Settings
        if (qualityDropdown != null)
        {
            // Populate dropdown with actual quality levels
            qualityDropdown.ClearOptions();
            string[] names = QualitySettings.names;
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(names));
            
            // Set current quality level
            int currentQuality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            qualityDropdown.value = currentQuality;
            QualitySettings.SetQualityLevel(currentQuality, true);
        }
    }

    public void SetMasterVolume(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        AudioListener.volume = value; // Simple implementation. Ideally route to an AudioMixer.
    }

    public void SetBGMVolume(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        // Implement specific BGM channel volume scaling if using an AudioMixer
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        // Implement specific SFX channel volume scaling if using an AudioMixer
    }

    public void SetQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("QualityLevel", index);
        PlayerPrefs.Save();
    }

    public void RequestReset()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
        else
        {
            // Fallback if no panel is wired
            PerformReset();
        }
    }

    public void CancelReset()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    public void PerformReset()
    {
        // Reset Economy
        PlayerPrefs.SetInt("PlayerXP", 0);
        PlayerPrefs.SetInt("PlayerCredits", 0);

        // Reset Lab Progress
        for (int i = 1; i <= 8; i++)
        {
            PlayerPrefs.SetInt($"Lab{i}_3D_Completed", 0);
            PlayerPrefs.SetInt($"Lab{i}_Completed", 0);
            PlayerPrefs.SetFloat($"Lab{i}_Progress", 0f);
            // Optionally, relock them (except Lab 1)
            PlayerPrefs.SetInt($"Lab{i}_Unlocked", i == 1 ? 1 : 0); 
        }

        PlayerPrefs.Save();
        Debug.Log("Progress has been completely reset.");

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        // Visual feedback on the button could go here
        if (resetProgressButton != null)
        {
            var btnText = resetProgressButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = "PROGRESS RESET!";
        }
    }
}
