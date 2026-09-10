using UnityEngine;

public class SolubilityThermometer3D : MonoBehaviour
{
    public SolubilityManager3D manager;
    public Transform mercuryColumn;
    public Renderer mercuryRenderer;

    private float currentVal = 0f;

    void Update()
    {
        if (manager == null) return;
        float targetVal = (manager.powerSwitch != null && manager.powerSwitch.isOn && manager.tempSlider != null) ? manager.tempSlider.value : 0f;
        currentVal = Mathf.Lerp(currentVal, targetVal, Time.deltaTime * 3f);

        if (mercuryColumn != null)
        {
            // Mercury rises from 0.02f (room temp) to 0.22f (hot 100°C) in local Y height
            float topY = Mathf.Lerp(0.02f, 0.22f, currentVal);
            mercuryColumn.localScale = new Vector3(mercuryColumn.localScale.x, topY * 0.5f, mercuryColumn.localScale.z);
            mercuryColumn.localPosition = new Vector3(0f, topY * 0.5f, 0f);
        }

        if (mercuryRenderer != null && mercuryRenderer.material != null)
        {
            Color coldColor = new Color(0.8f, 0.1f, 0.1f);
            Color hotColor = new Color(1.0f, 0.05f, 0.0f);
            Color c = Color.Lerp(coldColor, hotColor, currentVal);
            mercuryRenderer.material.color = c;
            if (mercuryRenderer.material.HasProperty("_BaseColor"))
            {
                mercuryRenderer.material.SetColor("_BaseColor", c);
            }
            if (mercuryRenderer.material.HasProperty("_EmissionColor"))
            {
                mercuryRenderer.material.SetColor("_EmissionColor", c * Mathf.Lerp(0.1f, 0.6f, currentVal));
            }
        }
    }
}
