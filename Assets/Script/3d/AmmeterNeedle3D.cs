using UnityEngine;

public class AmmeterNeedle3D : MonoBehaviour
{
    public Transform needlePivot;
    
    // Rotation of needle at 0A
    public float zeroAngle = 45f;
    // Rotation of needle at target current (e.g. 1.5A)
    public float targetAngle = -45f;
    
    public float smoothSpeed = 2f;
    
    private float currentAngle;
    private float desiredAngle;

    void Start()
    {
        currentAngle = zeroAngle;
        desiredAngle = zeroAngle;
        
        if (needlePivot != null)
        {
            Vector3 rot = needlePivot.localEulerAngles;
            rot.z = currentAngle;
            needlePivot.localEulerAngles = rot;
        }
    }

    void Update()
    {
        if (needlePivot != null && Mathf.Abs(currentAngle - desiredAngle) > 0.01f)
        {
            currentAngle = Mathf.Lerp(currentAngle, desiredAngle, Time.deltaTime * smoothSpeed);
            
            // Apply a slight jitter for realism if it's reading current
            float jitter = (desiredAngle != zeroAngle) ? Random.Range(-1f, 1f) : 0f;
            
            Vector3 rot = needlePivot.localEulerAngles;
            rot.z = currentAngle + jitter;
            needlePivot.localEulerAngles = rot;
        }
    }

    public void TurnOn()
    {
        desiredAngle = targetAngle;
    }
    
    public void TurnOff()
    {
        desiredAngle = zeroAngle;
    }
}
