using UnityEngine;

public class DragBandul : MonoBehaviour
{
    [Header("Rujukan Objek")]
    public Rigidbody rbBandulUtama;
    public Transform pivotTiang;

    [Header("Tetapan Putaran & Had (Realistik)")]
    public float offsetSudut = 90f;

    [Tooltip("Had maksimum bandul boleh ditarik ke kiri/kanan (Contoh: 80 darjah)")]
    public float hadSudutMaksimum = 80f;

    [Header("UI Sudut")]
    public TMPro.TextMeshProUGUI teksSudut;

    private Camera kameraUtama;
    private float jarakZ;
    private float lastSudutZ = 0f;
    private AudioSource audioSource;

    void Start()
    {
        kameraUtama = Camera.main;
        if (kameraUtama == null)
        {
            kameraUtama = FindFirstObjectByType<Camera>();
        }

        if (pivotTiang != null)
        {
            jarakZ = kameraUtama.WorldToScreenPoint(pivotTiang.position).z;
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (rbBandulUtama != null && !rbBandulUtama.isKinematic)
        {
            float currentSudutZ = rbBandulUtama.transform.localEulerAngles.z;
            if (currentSudutZ > 180f) currentSudutZ -= 360f;

            if (Mathf.Abs(currentSudutZ) < 2f && rbBandulUtama.angularVelocity.magnitude < 0.2f && rbBandulUtama.linearVelocity.magnitude < 0.2f)
            {
                rbBandulUtama.transform.localEulerAngles = Vector3.zero;
                rbBandulUtama.angularVelocity = Vector3.zero;
                rbBandulUtama.linearVelocity = Vector3.zero;
                rbBandulUtama.Sleep();
            }
        }

        if (teksSudut != null && rbBandulUtama != null)
        {
            float sudutZ = rbBandulUtama.transform.localEulerAngles.z;
            if (sudutZ > 180f) sudutZ -= 360f;
            
            if (!rbBandulUtama.IsSleeping() && !rbBandulUtama.isKinematic && rbBandulUtama.angularVelocity.magnitude > 0.1f)
            {
                if (lastSudutZ * sudutZ <= 0f && Mathf.Abs(sudutZ - lastSudutZ) < 20f && Mathf.Abs(sudutZ) < 5f)
                {
                    if (audioSource != null && audioSource.clip != null)
                    {
                        audioSource.PlayOneShot(audioSource.clip, 0.5f);
                    }
                }
            }
            lastSudutZ = sudutZ;
            
            float roundedSudut = Mathf.Abs(Mathf.Round(sudutZ));
            teksSudut.text = "Sudut: " + roundedSudut + "°";
            
            if (roundedSudut <= 10f)
            {
                teksSudut.color = KidFriendlySceneTheme.Feedback(this, Color.green);
            }
            else
            {
                teksSudut.color = KidFriendlySceneTheme.Feedback(this, Color.red);
            }
        }
    }

    void OnMouseDown()
    {
        if (rbBandulUtama != null)
        {
            rbBandulUtama.isKinematic = true;
            rbBandulUtama.linearVelocity = Vector3.zero;
            rbBandulUtama.angularVelocity = Vector3.zero;
        }
    }

    void OnMouseDrag()
    {
        if (pivotTiang == null || rbBandulUtama == null)
            return;

        Vector3 posisiTetikus = Input.mousePosition;
        posisiTetikus.z = jarakZ;
        Vector3 posisiDuniaTetikus = kameraUtama.ScreenToWorldPoint(posisiTetikus);

        Vector3 arahKeTetikus = posisiDuniaTetikus - pivotTiang.position;

        // 1. Kira sudut berdasarkan posisi tetikus
        float sudutZ = Mathf.Atan2(arahKeTetikus.y, arahKeTetikus.x) * Mathf.Rad2Deg;
        sudutZ += offsetSudut;

        // 2. TUKAR SUDUT KEPADA FORMAT -180 HINGGA 180 (Untuk mudah diurus)
        // Tanpa ini, Unity kadang-kadang mengira sudut sebagai 270 darjah yang mengelirukan Clamp
        if (sudutZ > 180f)
            sudutZ -= 360f;
        if (sudutZ < -180f)
            sudutZ += 360f;

        // 3. PENGAPIT (CLAMP) - Kunci sudut supaya tak lebih had yang ditetapkan
        sudutZ = Mathf.Clamp(sudutZ, -hadSudutMaksimum, hadSudutMaksimum);

        // 4. Putar bandul
        rbBandulUtama.transform.rotation = Quaternion.Euler(0, 0, sudutZ);
    }

    void OnMouseUp()
    {
        if (rbBandulUtama != null)
        {
            rbBandulUtama.isKinematic = false;
            rbBandulUtama.WakeUp();
        }
        var tut = FindAnyObjectByType<Lab1TutorialManager>();
        if (tut != null) tut.CompleteStep2();
    }
}
