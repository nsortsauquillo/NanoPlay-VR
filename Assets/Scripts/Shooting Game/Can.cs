using UnityEngine;

public class Can : MonoBehaviour
{
    [Header("Can Settings")]
    [SerializeField] float hitForce = 10f;
    [SerializeField] AudioClip hitSound;
    
    private Rigidbody rb;
    private AudioSource audioSource;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Add rigidbody if it doesn't exist
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
        }
        
        // Add audio source if it doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }
    
    public void OnHit(Vector3 hitPoint, Vector3 hitDirection)
    {
        // Apply force to the can
        if (rb != null)
        {
            Vector3 force = hitDirection * hitForce;
            rb.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
        }
        
        // Play hit sound
        if (audioSource && hitSound)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        Debug.Log("Can hit!");
    }
}