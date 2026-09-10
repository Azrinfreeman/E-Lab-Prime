using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    [Header("UI Inputs")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("Buttons")]
    public Button loginButton;
    public Button registerButton;

    [Header("Status Feedback")]
    public TextMeshProUGUI statusText;
    public float statusDisplayDuration = 3f;

    [Header("Scene Navigation")]
    public string mainMenuSceneName = "MainMenu";

    private Coroutine statusCoroutine;

    void Start()
    {
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(OnLoginClick);
        }

        if (registerButton != null)
        {
            registerButton.onClick.AddListener(OnRegisterClick);
        }

        if (statusText != null)
        {
            statusText.text = ""; // clear status text on start
        }
    }

    private void OnLoginClick()
    {
        if (usernameInput == null || passwordInput == null) return;

        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Sila masukkan nama pengguna dan kata laluan!", Color.red);
            
            // Highlight empty input fields by shaking them
            if (string.IsNullOrEmpty(username)) HighlightInputField(usernameInput);
            if (string.IsNullOrEmpty(password)) HighlightInputField(passwordInput);
            return;
        }

        // Mock authentication success
        ShowStatus("Log masuk berjaya! Memuatkan menu utama...", new Color(0.18f, 0.77f, 0.71f));
        StartCoroutine(LoadMenuWithDelay());
    }

    private void OnRegisterClick()
    {
        ShowStatus("Fungsi pendaftaran belum diaktifkan dalam demo ini.", Color.yellow);
    }

    private void HighlightInputField(TMP_InputField input)
    {
        if (input == null) return;
        StartCoroutine(ShakeInputField(input.transform));
    }

    private IEnumerator ShakeInputField(Transform trans)
    {
        Vector3 originalPos = trans.localPosition;
        float elapsed = 0f;
        float duration = 0.35f;
        float magnitude = 8f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            trans.localPosition = new Vector3(originalPos.x + x, originalPos.y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        trans.localPosition = originalPos;
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusText == null) return;

        if (statusCoroutine != null)
        {
            StopCoroutine(statusCoroutine);
        }

        statusCoroutine = StartCoroutine(DisplayStatusCoroutine(message, color));
    }

    private IEnumerator DisplayStatusCoroutine(string message, Color color)
    {
        statusText.text = message;
        statusText.color = color;
        yield return new WaitForSeconds(statusDisplayDuration);
        
        // Soft fade out
        float elapsed = 0f;
        float fadeDuration = 0.5f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            statusText.color = new Color(color.r, color.g, color.b, Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        statusText.text = "";
    }

    private IEnumerator LoadMenuWithDelay()
    {
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
