using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
    [Header("Kelajuan Pusingan")]
    public float kelajuan = 200f; // Nilai positif untuk pusing ikut jam, negatif untuk lawan jam

    void Update()
    {
        // Pusingkan ikon pada paksi Z (paksi 2D) secara berterusan
        transform.Rotate(0f, 0f, -kelajuan * Time.deltaTime);
    }
}