using UnityEngine;

public class Stopwatch3D : MonoBehaviour
{
    public TextMesh timeText;
    private bool isRunning = false;
    private float elapsedTime = 0f;

    private Camera mainCam;
    private Vector3 originalPos;
    private float distanceToCam;
    private bool isDragging = false;
    private bool isHovering = false;
    private Vector3 dragOffset;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null) mainCam = Object.FindFirstObjectByType<Camera>();
        originalPos = transform.position;
        UpdateTimeText();
    }

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimeText();
        }
    }

    private void UpdateTimeText()
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(elapsedTime - minutes * 60);
            int fraction = Mathf.FloorToInt((elapsedTime * 100) % 100);
            timeText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);
        }
    }

    public void StartStopwatch()
    {
        isRunning = true;
    }

    public void StopStopwatch()
    {
        isRunning = false;
    }

    public void ResetStopwatch()
    {
        isRunning = false;
        elapsedTime = 0f;
        UpdateTimeText();
    }

    void OnMouseEnter()
    {
        isHovering = true;
        // Optionally highlight the stopwatch slightly
        transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
    }

    void OnMouseExit()
    {
        isHovering = false;
        transform.localScale = Vector3.one;
    }

    void OnMouseDown()
    {
        isDragging = true;
        if (mainCam != null)
        {
            distanceToCam = mainCam.WorldToScreenPoint(transform.position).z;
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = distanceToCam;
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
            
            // Calculate exact offset without artificial lift for perfect cursor alignment
            dragOffset = transform.position - mouseWorldPos;
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging || mainCam == null) return;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = distanceToCam;
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        
        Vector3 targetPos = mouseWorldPos + dragOffset;
        
        // Constrain dragging to the table area roughly
        targetPos.x = Mathf.Clamp(targetPos.x, -2f, 2f);
        targetPos.z = Mathf.Clamp(targetPos.z, -1f, 1f);

        transform.position = targetPos;
    }

    void OnMouseUp()
    {
        isDragging = false;
        // Drop it back onto the table level
        Vector3 finalPos = transform.position;
        finalPos.y = originalPos.y;
        transform.position = finalPos;
    }
}
