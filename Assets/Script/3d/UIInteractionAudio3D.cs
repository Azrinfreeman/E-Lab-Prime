using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInteractionAudio3D : MonoBehaviour, IPointerDownHandler, IEndDragHandler
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for UI clicks
        
        // Load the clip from Resources or if it was assigned.
        // We will assign it via execute_code or inspector.
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Play on click for buttons and initial slider grab
        PlayClick();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Play on letting go of a slider
        PlayClick();
    }

    private void PlayClick()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
