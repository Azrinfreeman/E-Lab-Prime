using System.Collections;
using TMPro;
using UnityEngine;

public class AirCompositionManager : MonoBehaviour
{
    [Header("Experiment References")]
    public GameObject candleFlame;
    public ParticleSystem smokeEffect; // RUJUKAN BAHARU UNTUK ASAP
    public RectTransform cylinderWaterFill;
    public TextMeshProUGUI percentageText;
    public CylinderDrag cylinderDragScript;

    [Header("Simulation Settings")]
    public float burnTime = 3.0f;
    public float targetWaterHeight = 0.2f;
    public float waterRiseSpeed = 0.1f;

    [Header("Audio SFX")]
    public AudioSource audioSource;
    public AudioClip snapSound;
    public AudioClip sizzleSound;
    public AudioClip bubblingSound;
    public AudioClip successSound;

    [Header("Success UI")]
    public GameObject successPanel;

    private bool isExperimentStarted = false;
    private bool isWaterRising = false;

    void Start()
    {
        ResetExperiment();
    }

    void Update()
    {
        if (isWaterRising && cylinderWaterFill != null)
        {
            if (cylinderWaterFill.localScale.y < targetWaterHeight)
            {
                Vector3 currentScale = cylinderWaterFill.localScale;
                currentScale.y += waterRiseSpeed * Time.deltaTime;
                cylinderWaterFill.localScale = currentScale;

                if (percentageText != null)
                {
                    float currentPercentage = (currentScale.y / targetWaterHeight) * 20f;
                    percentageText.text =
                        "Isipadu Air: " + Mathf.RoundToInt(currentPercentage).ToString() + "%";
                }
            }
            else
            {
                isWaterRising = false;
                if (audioSource != null && audioSource.clip == bubblingSound)
                    audioSource.Stop();
                if (percentageText != null)
                    percentageText.text = "Isipadu Air: 20%";
                ExperimentCompleted();
            }
        }
    }

    public void StartExperiment()
    {
        if (!isExperimentStarted)
        {
            isExperimentStarted = true;
            PlaySFX(snapSound);
            StartCoroutine(ExperimentRoutine());
        }
    }

    private IEnumerator ExperimentRoutine()
    {
        yield return new WaitForSeconds(burnTime);

        // 1. Lilin terpadam
        if (candleFlame != null)
            candleFlame.SetActive(false);

        // 2. Mainkan bunyi desis
        PlaySFX(sizzleSound);

        // 3. MAIN KESAN ASAP!
        if (smokeEffect != null)
            smokeEffect.Play();

        yield return new WaitForSeconds(0.5f);

        isWaterRising = true;
        if (audioSource != null && bubblingSound != null)
        {
            audioSource.clip = bubblingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void ExperimentCompleted()
    {
        Invoke("ShowSuccessPopup", 1.5f);
    }

    private void ShowSuccessPopup()
    {
        if (successPanel != null)
        {
            successPanel.SetActive(true);
            PlaySFX(successSound);
        }
    }

    public void ResetExperiment()
    {
        StopAllCoroutines();

        isExperimentStarted = false;
        isWaterRising = false;

        if (candleFlame != null)
            candleFlame.SetActive(true);
        if (cylinderWaterFill != null)
            cylinderWaterFill.localScale = new Vector3(1, 0, 1);
        if (percentageText != null)
            percentageText.text = "Isipadu Air: 0%";
        if (successPanel != null)
            successPanel.SetActive(false);

        // Hentikan dan bersihkan asap jika reset ditekan awal
        if (smokeEffect != null)
        {
            smokeEffect.Stop();
            smokeEffect.Clear();
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (cylinderDragScript != null)
            cylinderDragScript.ResetCylinder();
    }

    public void GoToNextExperiment()
    {
        Debug.Log("Memuatkan Amali Seterusnya...");
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
