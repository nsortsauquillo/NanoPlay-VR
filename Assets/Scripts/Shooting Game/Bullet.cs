using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] float speed = 20f;
    [SerializeField] float lifetime = 5f;
    [SerializeField] float damage = 1f;
    
    [Header("Effects")]
    [SerializeField] ParticleSystem trailEffect;
    [SerializeField] ParticleSystem impactEffect;
    [SerializeField] AudioClip impactSound;
    
    private Rigidbody rb;
    private AudioSource audioSource;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        // Set velocity
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Don't hit the gun or player
        if (other.CompareTag("Player") || other.CompareTag("Gun"))
            return;
            
        Debug.Log($"Bullet hit: {other.name}");
        
        // Check if we hit a can
        Can can = other.GetComponent<Can>();
        if (can != null)
        {
            can.OnHit(transform.position, transform.forward);
        }
        
        // Create impact effect
        if (impactEffect != null)
        {
            GameObject impact = Instantiate(impactEffect.gameObject, transform.position, Quaternion.LookRotation(transform.forward));
            Destroy(impact, 2f);
        }
        
        // Play impact sound
        if (audioSource && impactSound)
        {
            audioSource.PlayOneShot(impactSound);
        }
        
        // Destroy bullet
        Destroy(gameObject);
    }
}