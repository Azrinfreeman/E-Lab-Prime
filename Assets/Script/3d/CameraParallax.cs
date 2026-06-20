using UnityEngine;

public class CameraParallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float parallaxStrength = 0.03f;
    public float smoothSpeed = 5f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        // Get mouse position normalized from -1 to 1
        float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

        // Clamp just in case mouse goes way off screen
        mouseX = Mathf.Clamp(mouseX, -1f, 1f);
        mouseY = Mathf.Clamp(mouseY, -1f, 1f);

        // Calculate target offset
        Vector3 targetOffset = new Vector3(mouseX * parallaxStrength, mouseY * parallaxStrength, 0f);
        
        // Smoothly move the camera
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPosition + targetOffset, Time.deltaTime * smoothSpeed);
    }
}
