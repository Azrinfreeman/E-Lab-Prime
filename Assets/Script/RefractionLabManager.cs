using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefractionLabManager : MonoBehaviour
{
    [Header("Rujukan Objek")]
    public RectTransform rayBox;
    public RectTransform laserNozzle;
    public RectTransform targetPoint;
    public LineRenderer laserBeam;

    [Header("Rujukan UI Teks Semasa")]
    public TextMeshProUGUI textSudutTuju;
    public TextMeshProUGUI textSudutBiasan;

    [Header("Visual Bantuan Sudut")]
    public Image lengkungTuju;
    public Image lengkungBiasan;

    [Header("Tetapan Fizik (Snell's Law)")]
    public float nAir = 1.0f;
    public float nKaca = 1.5f;
    public float tinggiBongkah = 3.0f;

    [Header("Sistem Rekod (Jadual Amali 8)")]
    // Nilai sudut i yang perlu dicari oleh pelajar (cth: 20°, 40°, 60°, 80°)
    public int[] targetAngles = { 20, 40, 60, 80 };

    // Masukkan kotak-kotak teks (SUDUT r) dari UI Jadual anda ke sini
    public TextMeshProUGUI[] resultTexts;
    public GameObject conclusionPanel;
    public Button recordButton;

    [Header("Tetapan Laser")]
    public float rayLength = 1000f;
    public float rotationOffset = 0f;

    [Header("Had & Snap Sudut")]
    public bool gunakanSnap = true;
    public float selangSnap = 10f;
    public float toleransiSnap = 3f;
    public float sudutMinimum = 10f;
    public float sudutMaksimum = 80f;

    private float lockedRadius;
    private int currentI = 0;
    private int currentR = 0;

    // Pembolehubah kawalan jadual
    private bool[] isRecorded;
    private int totalRecorded = 0;

    void Start()
    {
        if (rayBox != null && targetPoint != null)
        {
            lockedRadius = Vector3.Distance(rayBox.position, targetPoint.position);
        }

        if (laserBeam != null)
            laserBeam.positionCount = 4;
        if (conclusionPanel != null)
            conclusionPanel.SetActive(false);

        // Sediakan saiz logik mengikut jumlah sudut target
        isRecorded = new bool[targetAngles.Length];
    }

    void Update()
    {
        if (rayBox == null || targetPoint == null || laserBeam == null || laserNozzle == null)
            return;

        Vector3 targetPos = targetPoint.position;
        Vector3 rayBoxPos = rayBox.position;

        // 1. KIRA & SEKAT SUDUT SERETAN
        Vector2 dirFromTarget = rayBoxPos - targetPos;
        float rawAngle = Vector2.Angle(Vector2.up, dirFromTarget);
        rawAngle = Mathf.Clamp(rawAngle, sudutMinimum, sudutMaksimum);

        if (gunakanSnap)
        {
            float nearestSnap = Mathf.Round(rawAngle / selangSnap) * selangSnap;
            if (Mathf.Abs(rawAngle - nearestSnap) <= toleransiSnap)
            {
                rawAngle = nearestSnap;
            }
        }

        float sideSign = Mathf.Sign(dirFromTarget.x);
        if (sideSign == 0)
            sideSign = -1;
        float rad = rawAngle * Mathf.Deg2Rad;

        Vector3 finalDirection = new Vector3(Mathf.Sin(rad) * sideSign, Mathf.Cos(rad), 0);
        rayBox.position = targetPos + (finalDirection * lockedRadius);

        Vector3 directionToTarget = targetPos - rayBox.position;
        float angleRot = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        rayBox.rotation = Quaternion.Euler(0, 0, angleRot + rotationOffset);

        // 2. FORMULA PEMBIASAN (HUKUM SNELL)
        Vector3 nozzlePos = laserNozzle.position;
        float angleIncidentRad = rawAngle * Mathf.Deg2Rad;

        float sinR = (nAir * Mathf.Sin(angleIncidentRad)) / nKaca;
        float angleRefractedRad = Mathf.Asin(sinR);
        float angleRefractedDeg = angleRefractedRad * Mathf.Rad2Deg;

        // 3. LUKIS 4 TITIK LASER
        laserBeam.SetPosition(0, nozzlePos);
        laserBeam.SetPosition(1, targetPos);

        float xOffsetKaca = Mathf.Tan(angleRefractedRad) * tinggiBongkah * (-sideSign);
        Vector3 titikKeluar = new Vector3(
            targetPos.x + xOffsetKaca,
            targetPos.y - tinggiBongkah,
            targetPos.z
        );
        laserBeam.SetPosition(2, titikKeluar);

        Vector3 arahSinarMuncul = new Vector3(
            Mathf.Sin(angleIncidentRad) * (-sideSign),
            -Mathf.Cos(angleIncidentRad),
            0
        );
        Vector3 titikAkhirLaser = titikKeluar + (arahSinarMuncul.normalized * rayLength);
        laserBeam.SetPosition(3, titikAkhirLaser);

        // 4. UI & VISUAL SEMASA
        currentI = Mathf.RoundToInt(rawAngle);
        currentR = Mathf.RoundToInt(angleRefractedDeg);

        if (textSudutTuju != null)
            textSudutTuju.text = "Sudut Tuju, i : " + currentI + "°";
        if (textSudutBiasan != null)
            textSudutBiasan.text = "Sudut Biasan, r : " + currentR + "°";

        if (lengkungTuju != null)
            lengkungTuju.fillAmount = rawAngle / 360f;
        if (lengkungBiasan != null)
            lengkungBiasan.fillAmount = angleRefractedDeg / 360f;
    }

    // FUNGSI UTAMA: Dipanggil oleh butang RECORD hijau di bawah
    public void RecordData()
    {
        // Cari jika sudut i semasa sepadan dengan mana-mana baris dalam jadual (20, 40, 60, 80)
        int index = System.Array.IndexOf(targetAngles, currentI);

        if (index != -1)
        {
            // Jika ada dan belum pernah direkodkan
            if (!isRecorded[index])
            {
                if (resultTexts.Length > index && resultTexts[index] != null)
                {
                    // Masukkan nilai sudut biasan r ke dalam petak jadual yang betul
                    resultTexts[index].text = currentR.ToString() + "°";
                }

                isRecorded[index] = true;
                totalRecorded++;

                // Jika semua baris jadual (4 set data) sudah penuh
                if (totalRecorded >= targetAngles.Length)
                {
                    ExperimentCompleted();
                }
            }
            else
            {
                Debug.Log("Sudut " + currentI + "° sudah direkodkan!");
            }
        }
        else
        {
            Debug.Log(
                "Sila laras laser ke sudut genap jadual (20, 40, 60, atau 80) sebelum merekod."
            );
        }
    }

    private void ExperimentCompleted()
    {
        if (recordButton != null)
            recordButton.interactable = false;
        if (conclusionPanel != null)
            conclusionPanel.SetActive(true);
    }

    // FUNGSI UTAMA: Dipanggil oleh butang RESET hijau di bawah
    public void ResetTable()
    {
        totalRecorded = 0;
        for (int i = 0; i < isRecorded.Length; i++)
        {
            isRecorded[i] = false;
            if (resultTexts.Length > i && resultTexts[i] != null)
            {
                resultTexts[i].text = "-"; // Kembalikan teks jadual ke asal
            }
        }
        if (recordButton != null)
            recordButton.interactable = true;
        if (conclusionPanel != null)
            conclusionPanel.SetActive(false);
    }
}
