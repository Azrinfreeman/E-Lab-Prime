using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSessionStatusUI : MonoBehaviour
{
    [Header("Main Strip")]
    public TextMeshProUGUI statusTitleText;
    public TextMeshProUGUI statusSubtitleText;
    public TextMeshProUGUI statusPillText;
    public Image statusPillImage;
    public Image statusAccentImage;

    [Header("Side Badge")]
    public GameObject sideBadgeRoot;
    public TextMeshProUGUI sideBadgeText;
    public Image sideBadgeImage;

    [Header("Open Account")]
    public MainMenuManager mainMenuManager;
    public Button statusStripButton;
    public Button statusPillButton;

    [Header("Colors")]
    public Color studentColor = new Color(0.18f, 0.77f, 0.71f, 1f);
    public Color guestColor = new Color(1f, 0.78f, 0.25f, 1f);
    public Color signedOutColor = new Color(0.1f, 0.16f, 0.28f, 0.92f);
    public Color darkPillColor = new Color(0f, 0f, 0f, 0.35f);
    private void Awake()
    {
        WireOpenAccountButtons();
    }

    private void OnEnable()
    {
        WireOpenAccountButtons();

        bool signedIn = PlayerPrefs.GetInt("ELabPrime_AccountSignedIn", 0) == 1;
        bool guest = PlayerPrefs.GetInt("ELabPrime_AccountGuest", 0) == 1;
        string name = guest ? "Tetamu" : PlayerPrefs.GetString("ELabPrime_AccountName", "Pelajar E-Lab");
        string details = guest ? "Mod tetamu" : BuildStudentDetails();
        Refresh(signedIn, guest, name, details);
    }

    public void Refresh(bool signedIn)
    {
        Refresh(signedIn, false, PlayerPrefs.GetString("ELabPrime_AccountName", "Pelajar E-Lab"), BuildStudentDetails());
    }

    public void Refresh(bool signedIn, bool guest, string displayName, string details)
    {
        if (!signedIn)
        {
            ApplyState(
                "AKSES PELAJAR BELUM AKTIF",
                "Log masuk untuk simpan progress pelajar",
                "SIGN IN",
                signedOutColor,
                darkPillColor,
                true);
            return;
        }

        if (guest)
        {
            ApplyState(
                "MOD TETAMU AKTIF",
                "Progress hanya disimpan pada peranti ini",
                "GUEST",
                guestColor,
                guestColor,
                true);
            return;
        }

        ApplyState(
            "AKSES PELAJAR AKTIF",
            string.IsNullOrEmpty(details) ? displayName : displayName + "  |  " + details,
            "ACTIVE",
            studentColor,
            studentColor,
            true);
    }

    private void ApplyState(string title, string subtitle, string pill, Color accent, Color pillColor, bool showBadge)
    {
        if (statusTitleText != null)
        {
            statusTitleText.text = title;
        }

        if (statusSubtitleText != null)
        {
            statusSubtitleText.text = subtitle;
        }

        if (statusPillText != null)
        {
            statusPillText.text = pill;
        }

        if (statusPillImage != null)
        {
            statusPillImage.color = pillColor;
        }

        if (statusAccentImage != null)
        {
            statusAccentImage.color = accent;
        }

        if (sideBadgeRoot != null)
        {
            sideBadgeRoot.SetActive(showBadge);
        }

        if (sideBadgeText != null)
        {
            sideBadgeText.text = pill;
        }

        if (sideBadgeImage != null)
        {
            sideBadgeImage.color = pillColor;
        }
    }

    private static string BuildStudentDetails()
    {
        string studentId = PlayerPrefs.GetString("ELabPrime_AccountStudentId", string.Empty);
        string className = PlayerPrefs.GetString("ELabPrime_AccountClass", string.Empty);

        if (string.IsNullOrEmpty(studentId) && string.IsNullOrEmpty(className))
        {
            return "Profil pelajar";
        }

        if (string.IsNullOrEmpty(className))
        {
            return "ID: " + studentId;
        }

        return "ID: " + studentId + "  |  Kelas: " + className;
    }


    private void WireOpenAccountButtons()
    {
        if (mainMenuManager == null)
        {
            mainMenuManager = Object.FindFirstObjectByType<MainMenuManager>(FindObjectsInactive.Include);
        }

        if (statusStripButton == null)
        {
            statusStripButton = GetComponent<Button>();
            if (statusStripButton == null)
            {
                statusStripButton = gameObject.AddComponent<Button>();
            }
        }

        ConfigureOpenAccountButton(statusStripButton, GetComponent<Image>());

        if (statusPillImage != null)
        {
            if (statusPillButton == null)
            {
                statusPillButton = statusPillImage.GetComponent<Button>();
                if (statusPillButton == null)
                {
                    statusPillButton = statusPillImage.gameObject.AddComponent<Button>();
                }
            }

            ConfigureOpenAccountButton(statusPillButton, statusPillImage);
        }
    }

    private void ConfigureOpenAccountButton(Button button, Graphic targetGraphic)
    {
        if (button == null)
        {
            return;
        }

        button.targetGraphic = targetGraphic;
        button.transition = Selectable.Transition.ColorTint;
        button.interactable = true;
        button.onClick.RemoveListener(OpenAccountPanel);
        button.onClick.AddListener(OpenAccountPanel);
    }

private void OpenAccountPanel()
    {
        if (mainMenuManager == null)
        {
            mainMenuManager = Object.FindFirstObjectByType<MainMenuManager>(FindObjectsInactive.Include);
        }

        if (mainMenuManager != null)
        {
            mainMenuManager.BukaAkaun();
            OpenAccountPanelImmediate(mainMenuManager);
        }
    }


private static void OpenAccountPanelImmediate(MainMenuManager manager)
    {
        if (manager == null)
        {
            return;
        }

        if (manager.panelPengaturan != null)
        {
            manager.panelPengaturan.SetActive(false);
        }

        if (manager.panelPencapaian != null)
        {
            manager.panelPencapaian.SetActive(false);
        }

        if (manager.panelAmali != null)
        {
            manager.panelAmali.SetActive(false);
        }

        if (manager.panelUtama != null)
        {
            manager.panelUtama.SetActive(false);
        }

        if (manager.panelAkaun != null)
        {
            manager.panelAkaun.SetActive(true);
            MainMenuAccountPanelController accountController = manager.panelAkaun.GetComponent<MainMenuAccountPanelController>();
            if (accountController != null)
            {
                accountController.ShowAccountOverview();
            }
        }
    }
}
