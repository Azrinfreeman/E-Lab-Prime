using UnityEngine;
using UnityEngine.EventSystems;

public class BouncyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Tetapan Animasi")]
    public float saizDitekan = 0.85f; // Saiz butang bila ditekan (mengecil sikit)
    public float kelajuanAnimasi = 15f; // Kelajuan butang melantun

    private Vector3 saizAsal;
    private Vector3 saizSasaran;

    void Start()
    {
        // Simpan saiz asal butang (biasanya 1, 1, 1)
        saizAsal = transform.localScale;
        saizSasaran = saizAsal;
    }

    void Update()
    {
        // Lerp (Linear Interpolation) untuk menggerakkan saiz butang ke arah saiz sasaran dengan lancar
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            saizSasaran,
            Time.deltaTime * kelajuanAnimasi
        );
    }

    // Fungsi ini dipanggil secara automatik apabila tetikus/jari MENENGAN butang
    public void OnPointerDown(PointerEventData eventData)
    {
        saizSasaran = saizAsal * saizDitekan;
    }

    // Fungsi ini dipanggil secara automatik apabila tetikus/jari MELEPASKAN butang
    public void OnPointerUp(PointerEventData eventData)
    {
        saizSasaran = saizAsal;
    }
}
