using TMPro; // Needed for UI Text
using UnityEngine;
using UnityEngine.UI;

public class PengawalPanjangTali : MonoBehaviour
{
    [Header("Rujukan Objek 3D")]
    public Transform taliBandul;
    public Transform pemberatBandul;

    [Header("Rujukan UI Slider")]
    public Slider sliderPanjang;

    [Tooltip("Drag your 'CM' text object here so it updates automatically")]
    public TextMeshProUGUI teksCM;

    [Header("Tetapan UI (Nilai Sentimeter)")]
    public float cmPalingPendek = 20f;
    public float cmPalingPanjang = 100f;

    [Header("Tetapan Had Tali (Unit 3D)")]
    public float panjangPalingPendek = 1.5f;
    public float panjangPalingPanjang = 4.0f;

    private float ketebalanTaliAsal;

    void Start()
    {
        if (taliBandul != null)
        {
            ketebalanTaliAsal = taliBandul.localScale.x;
        }

        if (sliderPanjang != null)
        {
            // Set slider range based on CM settings
            sliderPanjang.minValue = cmPalingPendek;
            sliderPanjang.maxValue = cmPalingPanjang;
            sliderPanjang.wholeNumbers = true;

            // Connect slider to the function
            sliderPanjang.onValueChanged.AddListener(UbahPanjang);

            // Trigger it once at the start to ensure everything is aligned
            UbahPanjang(sliderPanjang.value);
        }
    }

    public void UbahPanjang(float nilaiSliderCM)
    {
        if (pemberatBandul == null || taliBandul == null)
            return;

        // 1. UPDATE THE UI TEXT (e.g., "40 CM")
        if (teksCM != null)
        {
            teksCM.text = nilaiSliderCM.ToString("0") + " CM";
        }

        // 2. MATH MAGIC: Convert 20-100cm into 3D Unity Units
        // Find the percentage (0.0 to 1.0) of the slider
        float peratusan = Mathf.InverseLerp(cmPalingPendek, cmPalingPanjang, nilaiSliderCM);

        // Apply that percentage to the 3D sizes
        float panjangDunia3D = Mathf.Lerp(panjangPalingPendek, panjangPalingPanjang, peratusan);

        // 3. APPLY TO 3D OBJECTS
        pemberatBandul.localPosition = new Vector3(0, -panjangDunia3D, 0);
        taliBandul.localScale = new Vector3(
            ketebalanTaliAsal,
            panjangDunia3D / 2f,
            ketebalanTaliAsal
        );
        taliBandul.localPosition = new Vector3(0, -(panjangDunia3D / 2f), 0);
    }
}
