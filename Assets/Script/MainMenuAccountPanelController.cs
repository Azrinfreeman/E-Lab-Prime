using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuAccountPanelController : MonoBehaviour
{
    private const string SignedInKey = "ELabPrime_AccountSignedIn";
    private const string GuestKey = "ELabPrime_AccountGuest";
    private const string NameKey = "ELabPrime_AccountName";
    private const string StudentIdKey = "ELabPrime_AccountStudentId";
    private const string ClassKey = "ELabPrime_AccountClass";
    private const string PasswordKey = "ELabPrime_AccountPassword";

    [Header("Forms")]
    public GameObject loginForm;
    public GameObject registerForm;
    public GameObject signedInPanel;
    public GameObject tabsRoot;

    [Header("Login Inputs")]
    public TMP_InputField loginStudentIdInput;
    public TMP_InputField loginPasswordInput;

    [Header("Register Inputs")]
    public TMP_InputField registerNameInput;
    public TMP_InputField registerStudentIdInput;
    public TMP_InputField registerClassInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerConfirmPasswordInput;

    [Header("Buttons")]
    public Button loginTabButton;
    public Button registerTabButton;
    public Button loginButton;
    public Button registerButton;
    public Button guestButton;
    public Button logoutButton;

    [Header("Tab Images")]
    public Image loginTabImage;
    public Image registerTabImage;

    [Header("Text")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI signedInNameText;
    public TextMeshProUGUI signedInDetailsText;
    public TextMeshProUGUI signedInInitialsText;
    public TextMeshProUGUI signedInModeText;
    public TextMeshProUGUI signedInSaveStatusText;
    public TextMeshProUGUI signedInAccessText;
    public TextMeshProUGUI signedInDeviceText;
    public TextMeshProUGUI signedInPrimaryActionText;
    public Image signedInModePillImage;

    [Header("Colors")]
    public Color activeTabColor = new Color(0.18f, 0.77f, 0.71f, 0.95f);
    public Color inactiveTabColor = new Color(0f, 0f, 0f, 0.25f);
    public Color successColor = new Color(0.18f, 0.77f, 0.71f, 1f);
    public Color warningColor = new Color(1f, 0.78f, 0.25f, 1f);
    public Color errorColor = new Color(1f, 0.38f, 0.34f, 1f);

    [Header("Session Badge")]
    public MainMenuSessionStatusUI sessionStatusUI;

    private void Awake()
    {
        WireButtons();
    }

    private void OnEnable()
    {
        RefreshState();
    }

    public void ShowLoginTab()
    {
        SetFormMode(true);
        SetStatus("Masukkan ID pelajar dan kata laluan.", warningColor);
    }

    public void ShowRegisterTab()
    {
        SetFormMode(false);
        SetStatus("Daftar profil pelajar baharu pada peranti ini.", warningColor);
    }

public void ShowAccountOverview()
    {
        RefreshState();

        if (!IsSignedIn())
        {
            ShowLoginTab();
            return;
        }

        if (loginForm != null)
        {
            loginForm.SetActive(false);
        }

        if (registerForm != null)
        {
            registerForm.SetActive(false);
        }

        if (signedInPanel != null)
        {
            signedInPanel.SetActive(true);
        }

        if (tabsRoot != null)
        {
            tabsRoot.SetActive(false);
        }

        if (guestButton != null)
        {
            guestButton.gameObject.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }

        SetStatus(IsGuest()
            ? "Mod tetamu aktif. Progress hanya disimpan pada peranti ini."
            : "Profil pelajar aktif. Kemajuan amali akan disimpan.",
            IsGuest() ? warningColor : successColor);
    }


    public void OnLoginClicked()
    {
        string studentId = GetText(loginStudentIdInput);
        string password = GetText(loginPasswordInput);

        if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password))
        {
            SetStatus("Sila isi ID pelajar dan kata laluan.", errorColor);
            return;
        }

        string savedStudentId = PlayerPrefs.GetString(StudentIdKey, string.Empty);
        string savedPassword = PlayerPrefs.GetString(PasswordKey, string.Empty);

        if (!string.IsNullOrEmpty(savedStudentId) && (studentId != savedStudentId || password != savedPassword))
        {
            SetStatus("ID pelajar atau kata laluan tidak sepadan.", errorColor);
            return;
        }

        if (string.IsNullOrEmpty(savedStudentId))
        {
            PlayerPrefs.SetString(StudentIdKey, studentId);
            PlayerPrefs.SetString(PasswordKey, password);
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(NameKey, string.Empty)))
            {
                PlayerPrefs.SetString(NameKey, "Pelajar E-Lab");
            }
        }

        PlayerPrefs.SetInt(SignedInKey, 1);
        PlayerPrefs.SetInt(GuestKey, 0);
        PlayerPrefs.Save();

        RefreshState();
        SetStatus("Log masuk berjaya.", successColor);
    }

    public void OnRegisterClicked()
    {
        string name = GetText(registerNameInput);
        string studentId = GetText(registerStudentIdInput);
        string className = GetText(registerClassInput);
        string password = GetText(registerPasswordInput);
        string confirmPassword = GetText(registerConfirmPasswordInput);

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password))
        {
            SetStatus("Nama, ID pelajar dan kata laluan diperlukan.", errorColor);
            return;
        }

        if (password != confirmPassword)
        {
            SetStatus("Pengesahan kata laluan tidak sepadan.", errorColor);
            return;
        }

        PlayerPrefs.SetString(NameKey, name);
        PlayerPrefs.SetString(StudentIdKey, studentId);
        PlayerPrefs.SetString(ClassKey, className);
        PlayerPrefs.SetString(PasswordKey, password);
        PlayerPrefs.SetInt(SignedInKey, 1);
        PlayerPrefs.SetInt(GuestKey, 0);
        PlayerPrefs.Save();

        ClearRegisterPasswords();
        RefreshState();
        SetStatus("Profil pelajar telah didaftarkan.", successColor);
    }

    public void ContinueAsGuest()
    {
        PlayerPrefs.SetInt(SignedInKey, 1);
        PlayerPrefs.SetInt(GuestKey, 1);
        PlayerPrefs.Save();

        RefreshState();
        SetStatus("Mod tetamu aktif. Progress hanya disimpan pada peranti ini.", warningColor);
    }

    public void Logout()
    {
        PlayerPrefs.SetInt(SignedInKey, 0);
        PlayerPrefs.SetInt(GuestKey, 0);
        PlayerPrefs.Save();

        RefreshState();
        SetStatus("Sesi akaun telah ditamatkan.", warningColor);
    }

    public void SignOut()
    {
        Logout();
    }

    public bool IsSignedIn()
    {
        return PlayerPrefs.GetInt(SignedInKey, 0) == 1;
    }

    public bool IsGuest()
    {
        return PlayerPrefs.GetInt(GuestKey, 0) == 1;
    }

    public string GetDisplayName()
    {
        if (IsGuest())
        {
            return "Tetamu";
        }

        return PlayerPrefs.GetString(NameKey, "Pelajar E-Lab");
    }

    public string GetStudentDetails()
    {
        if (IsGuest())
        {
            return "Mod tetamu";
        }

        string studentId = PlayerPrefs.GetString(StudentIdKey, string.Empty);
        string className = PlayerPrefs.GetString(ClassKey, string.Empty);

        if (string.IsNullOrEmpty(className))
        {
            return string.IsNullOrEmpty(studentId) ? "Profil pelajar" : "ID: " + studentId;
        }

        return "ID: " + studentId + "  |  Kelas: " + className;
    }

    private void WireButtons()
    {
        if (loginTabButton != null)
        {
            loginTabButton.onClick.RemoveListener(ShowLoginTab);
            loginTabButton.onClick.AddListener(ShowLoginTab);
        }

        if (registerTabButton != null)
        {
            registerTabButton.onClick.RemoveListener(ShowRegisterTab);
            registerTabButton.onClick.AddListener(ShowRegisterTab);
        }

        if (loginButton != null)
        {
            loginButton.onClick.RemoveListener(OnLoginClicked);
            loginButton.onClick.AddListener(OnLoginClicked);
        }

        if (registerButton != null)
        {
            registerButton.onClick.RemoveListener(OnRegisterClicked);
            registerButton.onClick.AddListener(OnRegisterClicked);
        }

        if (guestButton != null)
        {
            guestButton.onClick.RemoveListener(ContinueAsGuest);
            guestButton.onClick.AddListener(ContinueAsGuest);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveListener(Logout);
            logoutButton.onClick.AddListener(Logout);
        }
    }

private void RefreshState()
    {
        bool signedIn = IsSignedIn();
        bool guest = IsGuest();

        if (signedInPanel != null)
        {
            signedInPanel.SetActive(signedIn);
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(!signedIn);
        }

        if (loginForm != null)
        {
            loginForm.SetActive(!signedIn);
        }

        if (registerForm != null)
        {
            registerForm.SetActive(false);
        }

        if (tabsRoot != null)
        {
            tabsRoot.SetActive(!signedIn);
        }

        if (guestButton != null)
        {
            guestButton.gameObject.SetActive(!signedIn);
        }

        if (signedInNameText != null)
        {
            signedInNameText.text = signedIn ? GetDisplayName() : "Belum log masuk";
        }

        if (signedInDetailsText != null)
        {
            signedInDetailsText.text = signedIn ? GetStudentDetails() : "Log masuk untuk simpan progress pelajar.";
        }

        RefreshSignedInDetails(signedIn, guest);

        if (sessionStatusUI != null)
        {
            sessionStatusUI.Refresh(signedIn, guest, GetDisplayName(), GetStudentDetails());
        }

        SetTabVisual(true);
    }

private void SetFormMode(bool loginMode)
    {
        if (IsSignedIn())
        {
            ShowAccountOverview();
            return;
        }

        if (loginForm != null)
        {
            loginForm.SetActive(loginMode);
        }

        if (registerForm != null)
        {
            registerForm.SetActive(!loginMode);
        }

        if (signedInPanel != null)
        {
            signedInPanel.SetActive(false);
        }

        if (tabsRoot != null)
        {
            tabsRoot.SetActive(true);
        }

        if (guestButton != null)
        {
            guestButton.gameObject.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
        }

        SetTabVisual(loginMode);
    }

    private void SetTabVisual(bool loginMode)
    {
        if (loginTabImage != null)
        {
            loginTabImage.color = loginMode ? activeTabColor : inactiveTabColor;
        }

        if (registerTabImage != null)
        {
            registerTabImage.color = loginMode ? inactiveTabColor : activeTabColor;
        }
    }

    private void SetStatus(string message, Color color)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = message;
        statusText.color = color;
    }

    private static string GetText(TMP_InputField input)
    {
        return input == null ? string.Empty : input.text.Trim();
    }

    private void ClearRegisterPasswords()
    {
        if (registerPasswordInput != null)
        {
            registerPasswordInput.text = string.Empty;
        }

        if (registerConfirmPasswordInput != null)
        {
            registerConfirmPasswordInput.text = string.Empty;
        }
    }


private void RefreshSignedInDetails(bool signedIn, bool guest)
    {
        if (!signedIn)
        {
            SetText(signedInInitialsText, "EL");
            SetText(signedInModeText, "BELUM AKTIF");
            SetText(signedInSaveStatusText, "Log masuk untuk aktifkan simpanan.");
            SetText(signedInAccessText, "Akses makmal belum aktif.");
            SetText(signedInDeviceText, "Tiada sesi aktif pada peranti ini.");
            SetText(signedInPrimaryActionText, "LOG KELUAR");

            if (signedInModePillImage != null)
            {
                signedInModePillImage.color = inactiveTabColor;
            }

            return;
        }

        SetText(signedInInitialsText, GetInitials(GetDisplayName()));
        SetText(signedInModeText, guest ? "MOD TETAMU" : "PROFIL AKTIF");
        SetText(signedInSaveStatusText, guest
            ? "Progress disimpan tempatan."
            : "Kemajuan amali disimpan.");
        SetText(signedInAccessText, guest
            ? "Akses pembelajaran terhad."
            : "Sesi makmal sedia digunakan.");
        SetText(signedInDeviceText, guest
            ? "Sesi tetamu pada peranti ini."
            : "Profil peranti aktif.");
        SetText(signedInPrimaryActionText, "LOG KELUAR");

        if (signedInModePillImage != null)
        {
            signedInModePillImage.color = gameObject.scene.name == "MainMenu" ? (guest ? KidFriendlyPalette.Yellow : KidFriendlyPalette.Mint) : (guest ? warningColor : successColor);
        }
    }

    private static void SetText(TextMeshProUGUI target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private static string GetInitials(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return "EL";
        }

        string[] parts = displayName.Trim().Split(' ');
        string initials = string.Empty;
        for (int i = 0; i < parts.Length && initials.Length < 2; i++)
        {
            if (!string.IsNullOrWhiteSpace(parts[i]))
            {
                initials += char.ToUpperInvariant(parts[i][0]);
            }
        }

        return string.IsNullOrEmpty(initials) ? "EL" : initials;
    }
}
