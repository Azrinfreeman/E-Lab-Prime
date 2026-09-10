using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DisplayGameVersion : MonoBehaviour
{
    void Start()
    {
        TextMeshProUGUI versionText = GetComponent<TextMeshProUGUI>();
        if (versionText != null)
        {
            versionText.text = "v" + Application.version;
        }
    }
}
