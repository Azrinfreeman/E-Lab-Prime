using UnityEngine;
using UnityEngine.UI;

public class PengaturanManager : MonoBehaviour
{
    [Header("Rujukan UI Pengaturan")]
    public Slider sliderSuara;

    void Start()
    {
        // Tetapkan nilai awal Slider mengikut kelantangan (volume) semasa
        if (sliderSuara != null)
        {
            sliderSuara.value = AudioListener.volume;
            // Pautkan fungsi UbahSuara secara automatik apabila slider ditarik
            sliderSuara.onValueChanged.AddListener(UbahSuara);
        }
    }

    // Fungsi ini dikawal oleh Slider untuk mengubah bunyi
    public void UbahSuara(float nilaiVolume)
    {
        AudioListener.volume = nilaiVolume; // Nilai antara 0.0 (senyap) hingga 1.0 (maksimum)
    }
}
