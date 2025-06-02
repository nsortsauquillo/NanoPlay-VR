using UnityEngine;

public class Can : MonoBehaviour
{
    [Header("Can Settings")]
    [SerializeField] float hitForce = 10f;
    [SerializeField] AudioClip hitSound;
    [SerializeField] ParticleSystem hitEffect;
    
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool hasBeenHit = false;
    
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
        
        // Make sure we have a collider
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }
    
    public void OnHit(Vector3 hitPoint, Vector3 hitDirection)
    {
        if (hasBeenHit) return; // Prevent multiple hits from same bullet
        
        hasBeenHit = true;
        
        // Apply force to the can
        if (rb != null)
        {
            Vector3 force = hitDirection * hitForce;
            rb.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
        }
        
        // Play hit effect
        if (hitEffect != null)
        {
            hitEffect.transform.position = hitPoint;
            hitEffect.Play();
        }
        
        // Play hit sound
        if (audioSource && hitSound)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        Debug.Log("Can hit!");
        
        // Reset hit flag after a short time
        Invoke(nameof(ResetHitFlag), 0.1f);
    }
    
    void ResetHitFlag()
    {
        hasBeenHit = false;
    }
}