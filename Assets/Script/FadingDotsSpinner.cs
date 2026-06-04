using UnityEngine;
using UnityEngine.UI;

public class FadingDotsSpinner : MonoBehaviour
{
    [Header("Tetapan Titik (Dots)")]
    [Tooltip(
        "Seret kesemua objek imej titik (Dot_1, Dot_2...) ke sini mengikut urutan pusingan jam."
    )]
    public Image[] dots;

    [Header("Tetapan Animasi")]
    public float animationSpeed = 5f; // Kelajuan pusingan kelipan

    [Range(0f, 1f)]
    public float minAlpha = 0.2f; // Tahap malap paling rendah

    [Range(0f, 1f)]
    public float maxAlpha = 1.0f; // Tahap paling cerah (1.0 = penuh)

    private float offset = 0f;

    void Update()
    {
        // Tolak nilai offset berterusan berdasarkan kelajuan dan masa
        offset -= Time.deltaTime * animationSpeed;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null)
            {
                // Kira fasa animasi untuk setiap titik berdasarkan kedudukannya (i)
                // Kita bahagikan panjang array supaya efek bergelombang sekata
                float dotPhase = (float)i / dots.Length;

                // Guna Mathf.Repeat untuk memastikan nilai sentiasa antara 0 dan 1, berulang-ulang
                float currentPhase = Mathf.Repeat(offset + dotPhase, 1f);

                // Kemas kini Alpha imej titik tersebut
                Color currentColor = dots[i].color;

                // Gunakan Lerp untuk menukar Alpha dengan lancar antara malap dan cerah
                currentColor.a = Mathf.Lerp(minAlpha, maxAlpha, currentPhase);

                dots[i].color = currentColor;
            }
        }
    }
}
