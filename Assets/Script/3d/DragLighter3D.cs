using UnityEngine;

public class DragLighter3D : MonoBehaviour
{
    [Header("References")]
    public WaterHeatingManager3D heatingManager;
    public Transform[] targetBurners;

    [Header("Drag Settings")]
    public float dragHeight = 0.4f;

    private Camera mainCam;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private float distanceToCam;
    private bool isDragging = false;
    private bool hasIgnited = false;
    private float igniteTimer = 0f;
    private Transform indicatorObj;
    private Vector3 dragOffset;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null) mainCam = Object.FindFirstObjectByType<Camera>();
        
        if (heatingManager == null) heatingManager = Object.FindFirstObjectByType<WaterHeatingManager3D>();
        
        if (targetBurners == null || targetBurners.Length == 0)
        {
            GameObject bA = GameObject.Find("BurnerA_3D");
            GameObject bB = GameObject.Find("BurnerB_3D");
            if (bA != null && bB != null)
            {
                targetBurners = new Transform[] { bA.transform, bB.transform };
            }
        }

        originalPos = transform.position;
        originalRot = transform.rotation;
        indicatorObj = transform.Find("Indicator");
    }

    void OnMouseDown()
    {
        if (hasIgnited || (heatingManager != null && heatingManager.IsHeating)) return;
        
        if (heatingManager != null && !heatingManager.IsGasOn)
        {
            // Optional: Provide feedback to turn on gas first
            return;
        }

        isDragging = true;
        
        distanceToCam = mainCam.WorldToScreenPoint(transform.position).z;
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = distanceToCam;
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        
        // Calculate exact offset without artificial lift for perfect cursor alignment
        dragOffset = transform.position - mouseWorldPos;

        // Hide guide indicator when user starts dragging
        SetIndicatorActive(false);
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = distanceToCam;
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        
        transform.position = mouseWorldPos + dragOffset;

        if (targetBurners != null)
        {
            bool isNearBurner = false;
            foreach (Transform burner in targetBurners)
            {
                float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                                              new Vector3(burner.position.x, 0, burner.position.z));
                
                if (dist < 0.4f)
                {
                    isNearBurner = true;
                    // Tilt to ignite
                    transform.rotation = Quaternion.Euler(0f, 0f, 45f);
                    break;
                }
            }

            if (isNearBurner)
            {
                igniteTimer += Time.deltaTime;
                if (igniteTimer >= 0.5f && !hasIgnited)
                {
                    if (heatingManager != null && heatingManager.IsGasOn)
                    {
                        if (heatingManager.StartHeating())
                        {
                            hasIgnited = true;
                            isDragging = false;
                            gameObject.SetActive(false); // Hide lighter after successful ignite
                        }
                        else
                        {
                            // Could not start heating (e.g. salt not added yet), snap back
                            isDragging = false;
                            transform.position = originalPos;
                            transform.rotation = originalRot;
                            igniteTimer = 0f;
                            if (heatingManager.IsGasOn)
                            {
                                SetIndicatorActive(true);
                            }
                        }
                    }
                    else
                    {
                        // Gas is off, reset timer
                        igniteTimer = 0f;
                    }
                }
            }
            else
            {
                transform.rotation = originalRot;
                igniteTimer = 0f;
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        
        if (!hasIgnited)
        {
            transform.position = originalPos;
            transform.rotation = originalRot;
            
            // Restore indicator if gas is still on
            if (heatingManager != null && heatingManager.IsGasOn)
            {
                SetIndicatorActive(true);
            }
        }
        igniteTimer = 0f;
    }

    public void ResetLighter()
    {
        hasIgnited = false;
        igniteTimer = 0f;
        transform.position = originalPos;
        transform.rotation = originalRot;
        gameObject.SetActive(true);
        SetIndicatorActive(false); // Hidden initially until gas is turned on again
    }

    public void SetIndicatorActive(bool active)
    {
        if (indicatorObj == null) indicatorObj = transform.Find("Indicator");
        if (indicatorObj != null)
        {
            indicatorObj.gameObject.SetActive(active);

            // Dynamically set soft intensity/range at runtime to override any editor default
            if (active)
            {
                Light lt = indicatorObj.GetComponent<Light>();
                if (lt != null)
                {
                    lt.intensity = 0.18f;
                    lt.range = 0.25f;
                }

                // Turn off the text child component if it exists
                Transform textChild = indicatorObj.Find("Text");
                if (textChild != null)
                {
                    textChild.gameObject.SetActive(false);
                }
            }
        }
    }
}
