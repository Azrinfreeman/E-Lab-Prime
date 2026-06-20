using UnityEngine;

public class DragGaram3D : MonoBehaviour
{
    [Header("References")]
    public WaterHeatingManager3D heatingManager;
    public Transform targetBeakerB;
    public ParticleSystem saltParticles;
    public ParticleSystem dissolvingParticles;

    [Header("Drag Settings")]
    public float dragHeight = 0.4f; // Extra lift off the table when dragging

    private Camera mainCam;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private float distanceToCam;
    private bool isDragging = false;
    private bool hasPoured = false;
    private float pourTimer = 0f;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null) mainCam = Object.FindFirstObjectByType<Camera>();
        
        if (heatingManager == null) heatingManager = Object.FindFirstObjectByType<WaterHeatingManager3D>();
        if (targetBeakerB == null) 
        {
            GameObject beakerObj = GameObject.Find("BeakerB_3D");
            if (beakerObj != null) targetBeakerB = beakerObj.transform;
        }

        originalPos = transform.position;
        originalRot = transform.rotation;
        if (saltParticles != null) saltParticles.Stop();
    }

    void OnMouseDown()
    {
        if (hasPoured || (heatingManager != null && heatingManager.IsHeating)) return;
        isDragging = true;
        distanceToCam = mainCam.WorldToScreenPoint(transform.position).z;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = distanceToCam;
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        
        // Add height to float above the table during drag
        mouseWorldPos.y += dragHeight;

        transform.position = mouseWorldPos;

        // Check distance to target beaker B
        if (targetBeakerB != null)
        {
            float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                                          new Vector3(targetBeakerB.position.x, 0, targetBeakerB.position.z));
            
            // If near beaker B, tilt and pour
            if (dist < 0.4f)
            {
                transform.position = targetBeakerB.position + new Vector3(0f, 0.45f, -0.08f); // Position over beaker B
                transform.rotation = Quaternion.Euler(110f, 0f, 0f); // Tilt down to pour
                
                if (saltParticles != null && !saltParticles.isPlaying)
                {
                    saltParticles.Play();
                }

                pourTimer += Time.deltaTime;
                if (pourTimer >= 1.5f && !hasPoured)
                {
                    hasPoured = true;
                    if (saltParticles != null) saltParticles.Stop();
                    if (dissolvingParticles != null) dissolvingParticles.Play();
                    if (heatingManager != null) heatingManager.AddImpurities();
                    
                    isDragging = false;
                    gameObject.SetActive(false); // Hide salt bottle after successful pour
                }
            }
            else
            {
                // Upright while moving
                transform.rotation = originalRot;
                if (saltParticles != null && saltParticles.isPlaying)
                {
                    saltParticles.Stop();
                }
                pourTimer = 0f;
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        
        // If the user releases the mouse while it was pouring, consider it successful!
        if (pourTimer > 0.1f && !hasPoured)
        {
            hasPoured = true;
            if (dissolvingParticles != null) dissolvingParticles.Play();
            if (heatingManager != null) heatingManager.AddImpurities();
            gameObject.SetActive(false); // Hide salt bottle after successful pour
        }
        else if (!hasPoured)
        {
            // Snap back to original position
            transform.position = originalPos;
            transform.rotation = originalRot;
        }

        if (saltParticles != null) saltParticles.Stop();
        pourTimer = 0f;
    }

    public void ResetSalt()
    {
        hasPoured = false;
        pourTimer = 0f;
        transform.position = originalPos;
        transform.rotation = originalRot;
        gameObject.SetActive(true);
        if (saltParticles != null) saltParticles.Stop();
        if (dissolvingParticles != null) dissolvingParticles.Stop();
    }
}
