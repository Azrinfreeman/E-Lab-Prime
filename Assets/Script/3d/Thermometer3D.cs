using UnityEngine;

public class Thermometer3D : MonoBehaviour
{
    [Tooltip("Pivot of the red liquid column cylinder.")]
    public Transform liquidPivot;

    [Header("Temperature Settings")]
    [Tooltip("Target temperature (0.0 to 1.0)")]
    public float targetTemperature = 0f;
    [Tooltip("Current smoothed temperature (0.0 to 1.0)")]
    public float currentTemperature = 0f;
    [Tooltip("Speed at which the temperature changes.")]
    public float responseSpeed = 0.5f;

    [Header("Visual Scale Limits")]
    public float minScaleY = 0.02f;
    public float maxScaleY = 0.33f;

    void Update()
    {
        // Smoothly move towards the target temperature
        currentTemperature = Mathf.MoveTowards(currentTemperature, targetTemperature, responseSpeed * Time.deltaTime);

        if (liquidPivot != null)
        {
            float scaleY = Mathf.Lerp(minScaleY, maxScaleY, currentTemperature);
            Vector3 scale = liquidPivot.localScale;
            scale.y = scaleY;
            liquidPivot.localScale = scale;
        }
    }
}
