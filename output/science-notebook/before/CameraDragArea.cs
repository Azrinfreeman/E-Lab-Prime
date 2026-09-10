using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class CameraDragArea : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    public Transform targetCamera;
    public float rotationSpeed = 0.2f;
    public float maxRotationAngle = 60f; // Max left/right degrees
    public float maxRotationAngleX = 20f; // Max up/down degrees
    public float snapBackSpeed = 5f;

    [Header("POV Transition")]
    public float transitionSpeed = 5f;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isTransitioning = false;

    private bool isDragging = false;
    private float currentRotationY = 0f;
    private float currentRotationX = 0f;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    
    private Coroutine autoRevertCoroutine;

    void Start()
    {
        if (targetCamera == null && Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }

        if (targetCamera != null)
        {
            originalPosition = targetCamera.position;
            originalRotation = targetCamera.rotation;
            defaultPosition = targetCamera.position;
            defaultRotation = targetCamera.rotation;
            targetPosition = targetCamera.position;
            targetRotation = targetCamera.rotation;
        }

        // Dynamically find and bind UI buttons globally (compatible with both 2D Overlay and 3D World Space)
        GameObject panelObj = GameObject.Find("CameraPOVPanel");
        if (panelObj != null)
        {
            Transform panel = panelObj.transform;
            BindButton(panel.Find("Btn_FrontView"), SetFrontView);
            BindButton(panel.Find("Btn_RightView"), SetRightView);
            BindButton(panel.Find("Btn_LeftView"), SetLeftView);
            BindButton(panel.Find("Btn_BehindView"), SetBehindView);
            BindButton(panel.Find("Btn_TopView"), SetTopView);
            BindButton(panel.Find("Btn_TextbookView"), SetTextbookView);
            
            // Lab 2 specific bindings
            BindButton(panel.Find("Btn_DefaultView"), SetLab2DefaultView);
            BindButton(panel.Find("Btn_AmmeterView"), SetAmmeterView);
            BindButton(panel.Find("Btn_TestTubesView"), SetTestTubesView);

            // Lab 3 specific bindings
            BindButton(panel.Find("Btn_ThermoA"), SetThermoAView);
            BindButton(panel.Find("Btn_ThermoB"), SetThermoBView);
        }
    }

    private void BindButton(Transform btnTrans, UnityEngine.Events.UnityAction action)
    {
        if (btnTrans != null)
        {
            Button btn = btnTrans.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        isTransitioning = false; // Stop transitioning if user starts manual rotation
        CancelAutoRevert(); // Stop auto-revert if user takes manual control

        if (targetCamera != null)
        {
            originalRotation = targetCamera.rotation;
            targetPosition = targetCamera.position;
        }
        currentRotationY = 0f;
        currentRotationX = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || targetCamera == null) return;

        float fovRatio = 1f;
        Camera cam = targetCamera.GetComponent<Camera>();
        if (cam != null)
        {
            fovRatio = cam.fieldOfView / 60f; // Scale speed based on zoom
        }

        // Horizontal mouse movement maps to rotation around Y axis
        float rotationDeltaY = eventData.delta.x * (rotationSpeed * fovRatio);
        currentRotationY += rotationDeltaY;
        currentRotationY = Mathf.Clamp(currentRotationY, -maxRotationAngle, maxRotationAngle);

        // Vertical mouse movement maps to rotation around X axis (pitch)
        float rotationDeltaX = eventData.delta.y * (rotationSpeed * fovRatio);
        currentRotationX += rotationDeltaX;
        currentRotationX = Mathf.Clamp(currentRotationX, -maxRotationAngleX, maxRotationAngleX);

        targetCamera.rotation = originalRotation * Quaternion.Euler(-currentRotationX, currentRotationY, 0);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    void Update()
    {
        if (isTransitioning && targetCamera != null)
        {
            // Smoothly move and rotate towards the target POV
            targetCamera.position = Vector3.Lerp(targetCamera.position, targetPosition, Time.deltaTime * transitionSpeed);
            targetCamera.rotation = Quaternion.Slerp(targetCamera.rotation, targetRotation, Time.deltaTime * transitionSpeed);

            // Finish transition if close enough
            if (Vector3.Distance(targetCamera.position, targetPosition) < 0.01f && 
                Quaternion.Angle(targetCamera.rotation, targetRotation) < 0.5f)
            {
                targetCamera.position = targetPosition;
                targetCamera.rotation = targetRotation;
                originalRotation = targetRotation;
                currentRotationY = 0f;
                currentRotationX = 0f;
                isTransitioning = false;
            }
            
            // Smoothly lerp FOV back to default 60 during transition
            Camera cam = targetCamera.GetComponent<Camera>();
            if (cam != null)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, Time.deltaTime * transitionSpeed);
            }
        }
        else if (!isDragging && targetCamera != null && (currentRotationY != 0f || currentRotationX != 0f))
        {
            // Smoothly snap back to original rotation when released
            currentRotationY = Mathf.Lerp(currentRotationY, 0f, Time.deltaTime * snapBackSpeed);
            currentRotationX = Mathf.Lerp(currentRotationX, 0f, Time.deltaTime * snapBackSpeed);
            targetCamera.rotation = originalRotation * Quaternion.Euler(-currentRotationX, currentRotationY, 0);

            // Snap completely to 0 if very close
            if (Mathf.Abs(currentRotationY) < 0.1f && Mathf.Abs(currentRotationX) < 0.1f)
            {
                currentRotationY = 0f;
                currentRotationX = 0f;
                targetCamera.rotation = originalRotation;
            }
        }
        
        // Mouse scroll zoom handling
        if (targetCamera != null && !isTransitioning)
        {
            Camera cam = targetCamera.GetComponent<Camera>();
            if (cam != null)
            {
                float scroll = Input.mouseScrollDelta.y;
                if (scroll != 0)
                {
                    float newFov = cam.fieldOfView - scroll * 5f;
                    cam.fieldOfView = Mathf.Clamp(newFov, 20f, 75f);
                }
            }
        }
    }

    // Camera perspective targets
    public void SetFrontView()
    {
        targetPosition = defaultPosition;
        targetRotation = defaultRotation;
        isTransitioning = true;
        ResetTabletPosition();
        CancelAutoRevert();
    }

    public void SetRightView()
    {
        SetTargetView(new Vector3(2.5f, 0.4f, 0f));
        TriggerAutoRevert();
    }

    public void SetLeftView()
    {
        SetTargetView(new Vector3(-2.5f, 0.4f, 0f));
        TriggerAutoRevert();
    }

    public void SetBehindView()
    {
        SetTargetView(new Vector3(0f, 0.4f, 2.5f));
        TriggerAutoRevert();
    }

    public void SetTopView()
    {
        SetTargetView(new Vector3(0f, 2.5f, -0.01f));
        TriggerAutoRevert();
    }

    public void SetLab2DefaultView()
    {
        Vector3 basePos = Vector3.zero;
        GameObject stand = GameObject.Find("RetortStand_3D");
        if (stand != null) basePos = stand.transform.position;

        targetPosition = basePos + new Vector3(0f, 1.2f, -2.5f);
        targetRotation = Quaternion.LookRotation((basePos + new Vector3(0f, 0.6f, 0f)) - targetPosition);
        isTransitioning = true;
        ResetTabletPosition();
        CancelAutoRevert();
    }

    public void SetAmmeterView()
    {
        GameObject ammeter = GameObject.Find("Ammeter_3D");
        if (ammeter != null)
        {
            Vector3 pos = ammeter.transform.position;
            targetPosition = pos + new Vector3(0f, 0.2f, -0.6f);
            targetRotation = Quaternion.LookRotation(pos - targetPosition);
            isTransitioning = true;
            ResetTabletPosition();
            TriggerAutoRevert();
        }
    }

    public void SetTestTubesView()
    {
        GameObject stand = GameObject.Find("RetortStand_3D");
        if (stand != null)
        {
            Vector3 pos = stand.transform.position + new Vector3(0f, 0.8f, 0f); // middle of test tubes
            targetPosition = pos + new Vector3(0f, 0.1f, -0.6f);
            targetRotation = Quaternion.LookRotation(pos - targetPosition);
            isTransitioning = true;
            ResetTabletPosition();
            TriggerAutoRevert();
        }
    }

    public void SetTextbookView()
    {
        CancelAutoRevert(); // No auto-revert while reading textbook
        
        GameObject notebook = GameObject.Find("Lab2_Notebook");
        if (notebook == null) notebook = GameObject.Find("Lab3_Notebook");
        if (notebook == null) notebook = GameObject.Find("Lab4_Notebook");
        if (notebook == null) notebook = GameObject.Find("Lab5_Notebook");

        if (notebook != null)
        {
            Vector3 notebookPos = notebook.transform.position;
            Vector3 forward = notebook.transform.forward;
            Vector3 up = notebook.transform.up;

            targetPosition = notebookPos + up * 0.4f - forward * 0.15f;
            targetRotation = Quaternion.LookRotation(notebookPos - targetPosition);
            isTransitioning = true;

            // Adjust tablet position to prevent clipping in Textbook View
            GameObject tablet = GameObject.Find("Lab3_3DTablet");
            if (tablet == null) tablet = GameObject.Find("Lab2_3DTablet");
            if (tablet == null) tablet = GameObject.Find("Lab4_3DTablet");
            if (tablet == null) tablet = GameObject.Find("Lab5_3DTablet");
            if (tablet != null)
            {
                // Place very close to camera to completely avoid clipping the notebook (Z=0.08)
                // Use LookRotation to perfectly face the camera lens to eliminate perspective distortion
                tablet.transform.localPosition = new Vector3(0.04f, 0.01f, 0.08f);
                Vector3 dirToCam = -tablet.transform.localPosition.normalized;
                tablet.transform.localRotation = Quaternion.LookRotation(Vector3.up, dirToCam);
                tablet.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
            }
        }
        else
        {
            Debug.LogWarning("Notebook not found to compute camera perspective target.");
        }
    }

    public void SetThermoAView()
    {
        WaterHeatingManager3D manager = Object.FindAnyObjectByType<WaterHeatingManager3D>();
        if (manager != null && manager.thermoPivotA != null && manager.thermoPivotA.parent != null)
        {
            Vector3 targetPos = manager.thermoPivotA.parent.position + new Vector3(0f, -0.05f, 0f);
            targetPosition = targetPos + new Vector3(0f, 0.15f, -0.7f);
            targetRotation = Quaternion.LookRotation(targetPos - targetPosition);
            isTransitioning = true;
            ResetTabletPosition();
            CancelAutoRevert();
        }
        else
        {
            Debug.LogWarning("Thermo A reference or pivot parent not found.");
        }
    }

    public void SetThermoBView()
    {
        WaterHeatingManager3D manager = Object.FindAnyObjectByType<WaterHeatingManager3D>();
        if (manager != null && manager.thermoPivotB != null && manager.thermoPivotB.parent != null)
        {
            Vector3 targetPos = manager.thermoPivotB.parent.position + new Vector3(0f, -0.05f, 0f);
            targetPosition = targetPos + new Vector3(0f, 0.15f, -0.7f);
            targetRotation = Quaternion.LookRotation(targetPos - targetPosition);
            isTransitioning = true;
            ResetTabletPosition();
            CancelAutoRevert();
        }
        else
        {
            Debug.LogWarning("Thermo B reference or pivot parent not found.");
        }
    }

    private void SetTargetView(Vector3 offset)
    {
        ResetTabletPosition();

        GameObject beaker = GameObject.Find("BeakerA_3D");
        if (beaker == null) beaker = GameObject.Find("Beaker_3D");

        if (beaker != null)
        {
            Vector3 beakerPos = beaker.transform.position;
            GameObject beakerB = GameObject.Find("BeakerB_3D");
            if (beakerB != null)
            {
                beakerPos = (beaker.transform.position + beakerB.transform.position) / 2f;
            }

            Vector3 beakerCenter = beakerPos - new Vector3(0f, 0.2f, 0f);

            targetPosition = beakerPos + offset;
            targetRotation = Quaternion.LookRotation(beakerCenter - targetPosition);
            isTransitioning = true;
        }
        else
        {
            Debug.LogWarning("Beaker target not found to compute camera perspective.");
        }
    }

    private void ResetTabletPosition()
    {
        GameObject tablet = GameObject.Find("Lab5_3DTablet");
        if (tablet != null)
        {
            // Lab 5 specific coordinates matching setup script
            tablet.transform.localPosition = new Vector3(0.2004f, -0.0798f, 0.3001f);
            tablet.transform.localRotation = Quaternion.Euler(-70f, 0f, 15f);
            tablet.transform.localScale = new Vector3(0.2943f, 0.2943f, 0.2943f);
            return;
        }

        tablet = GameObject.Find("Lab3_3DTablet");
        if (tablet != null)
        {
            // Positioned further right and slightly lower to avoid blocking lighter and salt bottle
            tablet.transform.localPosition = new Vector3(0.55f, -0.22f, 0.8f);
            tablet.transform.localRotation = Quaternion.Euler(-70f, 0f, 15f);
            tablet.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            return;
        }

        tablet = GameObject.Find("Lab2_3DTablet");
        if (tablet != null)
        {
            // Lab 2 specific coordinates
            tablet.transform.localPosition = new Vector3(0.105f, -0.06f, 0.24f);
            tablet.transform.localRotation = Quaternion.Euler(-70f, 0f, 15f);
            tablet.transform.localScale = new Vector3(0.21f, 0.21f, 0.21f);
            return;
        }

        tablet = GameObject.Find("Lab4_3DTablet");
        if (tablet != null)
        {
            // Lab 4 specific coordinates
            tablet.transform.localPosition = new Vector3(0.14f, -0.1f, 0.25f);
            tablet.transform.localRotation = Quaternion.Euler(-70f, 0f, 15f);
            tablet.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
        }
    }

    private void TriggerAutoRevert()
    {
        CancelAutoRevert();
        if (gameObject.activeInHierarchy)
        {
            autoRevertCoroutine = StartCoroutine(AutoRevertRoutine());
        }
    }

    private void CancelAutoRevert()
    {
        if (autoRevertCoroutine != null)
        {
            StopCoroutine(autoRevertCoroutine);
            autoRevertCoroutine = null;
        }
    }

    private IEnumerator AutoRevertRoutine()
    {
        yield return new WaitForSeconds(4f);
        
        if (GameObject.Find("Lab2_3DTablet") != null)
        {
            SetLab2DefaultView();
        }
        else
        {
            SetFrontView();
        }
    }
}
