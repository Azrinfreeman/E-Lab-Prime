using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReflectionManager : MonoBehaviour
{
    [Header("Rujukan Objek")]
    public RectTransform rayBox;
    public RectTransform laserNozzle;
    public RectTransform targetPoint;
    public LineRenderer laserBeam;

    [Header("Rujukan UI Teks Semasa")]
    public TextMeshProUGUI textSudutTuju;
    public TextMeshProUGUI textSudutPantulan;

    [Header("Sistem Rekod (Jadual Khas)")]
    public int[] targetAngles = { 20, 40, 60, 80 }; // Saya buang 100 kerana tak logik secara fizik
    public TextMeshProUGUI[] resultTexts;
    public GameObject conclusionPanel;
    public Button recordButton;

    [Header("Visual Bantuan Sudut")]
    public Image lengkungTuju;
    public Image lengkungPantulan;

    [Header("Tetapan Laser")]
    public float rayLength = 1000f;
    public float rotationOffset = 0f;

    [Header("Tetapan Lekatan (Snap)")]
    public bool gunakanSnap = true;
    public float selangSnap = 10f;
    public float toleransiSnap = 3f;

    [Header("Had Pergerakan Sudut")]
    public float sudutMinimum = 10f; // Elak kotak bertindih dengan Garis Normal
    public float sudutMaksimum = 85f; // Elak kotak keluar skrin atau langgar meja

    private float lockedRadius;
    private int currentI = 0;
    private int currentR = 0;
    private bool[] isRecorded;
    private int totalRecorded = 0;

    void Start()
    {
        if (rayBox != null && targetPoint != null)
        {
            lockedRadius = Vector3.Distance(rayBox.position, targetPoint.position);
        }

        if (conclusionPanel != null)
            conclusionPanel.SetActive(false);
        isRecorded = new bool[targetAngles.Length];
    }

    void Update()
    {
        if (rayBox == null || targetPoint == null || laserBeam == null || laserNozzle == null)
            return;

        Vector3 targetPos = targetPoint.position;
        Vector3 rayBoxPos = rayBox.position;

        Vector2 dirFromTarget = rayBoxPos - targetPos;
        float rawAngle = Vector2.Angle(Vector2.up, dirFromTarget);

        // KUNCI HAD SUDUT DI SINI
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

        Vector3 nozzlePos = laserNozzle.position;
        Vector2 incomingVector = targetPos - nozzlePos;
        Vector2 normalVector = Vector2.up;
        Vector2 reflectedVector = Vector2.Reflect(incomingVector, normalVector);

        laserBeam.SetPosition(0, nozzlePos);
        laserBeam.SetPosition(1, targetPos);
        laserBeam.SetPosition(2, (Vector2)targetPos + (reflectedVector.normalized * rayLength));

        float angleIncidence = Vector2.Angle(nozzlePos - targetPos, normalVector);
        float angleReflection = Vector2.Angle(reflectedVector, normalVector);

        currentI = Mathf.RoundToInt(angleIncidence);
        currentR = Mathf.RoundToInt(angleReflection);

        if (textSudutTuju != null)
            textSudutTuju.text = "Sudut Tuju, i : " + currentI + "°";
        if (textSudutPantulan != null)
            textSudutPantulan.text = "Sudut Pantulan, r : " + currentR + "°";

        if (lengkungTuju != null)
            lengkungTuju.fillAmount = angleIncidence / 360f;
        if (lengkungPantulan != null)
            lengkungPantulan.fillAmount = angleReflection / 360f;
    }

    public void RecordData()
    {
        int index = System.Array.IndexOf(targetAngles, currentI);

        if (index != -1)
        {
            if (!isRecorded[index])
            {
                if (resultTexts.Length > index && resultTexts[index] != null)
                {
                    resultTexts[index].text = currentR.ToString();
                }

                isRecorded[index] = true;
                totalRecorded++;

                if (totalRecorded >= targetAngles.Length)
                {
                    ExperimentCompleted();
                }
            }
        }
    }

    private void ExperimentCompleted()
    {
        if (recordButton != null)
            recordButton.interactable = false;
        if (conclusionPanel != null)
            conclusionPanel.SetActive(true);
    }

    public void ResetTable()
    {
        totalRecorded = 0;
        for (int i = 0; i < isRecorded.Length; i++)
        {
            isRecorded[i] = false;
            if (resultTexts.Length > i && resultTexts[i] != null)
            {
                resultTexts[i].text = "-";
            }
        }
        if (recordButton != null)
            recordButton.interactable = true;
        if (conclusionPanel != null)
            conclusionPanel.SetActive(false);
    }
}
