using UnityEngine;

public class Can : MonoBehaviour
{
    [Header("Can Settings")]
    [SerializeField] float hitForce = 3.0f;      // Horizontal force
    [SerializeField] float upwardForce = 5.0f;   // Base upward force
    [SerializeField] float upwardVariation = 2.0f; // Variation in upward force
    [SerializeField] AudioClip hitSound;
    [SerializeField] ParticleSystem hitEffect;
    
    [Header("Physics Settings")]
    [SerializeField] float canMass = 0.3f;
    [SerializeField] float canDrag = 1.5f;
    [SerializeField] float canAngularDrag = 3f;
    
    private Rigidbody rb;
    private AudioSource audioSource;
    private ShootingGameManager gameManager;
    private int hitCount = 0; // Track number of hits instead of preventing multiple hits
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Add rigidbody if it doesn't exist
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure rigidbody for slower falling and better physics
        if (rb != null)
        {
            rb.mass = canMass;
            rb.drag = canDrag;
            rb.angularDrag = canAngularDrag;
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
        
        // Find the game manager if not already set
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<ShootingGameManager>();
        }
        
        Debug.Log($"Can initialized: {gameObject.name}");
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if can hit the floor/ground
        if (collision.gameObject.CompareTag("Ground") || 
            collision.gameObject.CompareTag("Floor") || 
            collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            collision.gameObject.name.ToLower().Contains("floor") ||
            collision.gameObject.name.ToLower().Contains("ground"))
        {
            Debug.Log($"Can hit the floor after {hitCount} hits, destroying...");
            
            // Notify game manager before destroying
            if (gameManager != null)
            {
                gameManager.OnCanDestroyed(gameObject);
            }
            
            Destroy(gameObject);
        }
    }
    
    public void OnHit(Vector3 hitPoint, Vector3 hitDirection)
    {
        hitCount++; // Increment hit counter - allow multiple hits
        Debug.Log($"Can hit #{hitCount}!");
        
        // Apply impulse to create parabolic trajectory
        if (rb != null)
        {
            // Create random horizontal direction instead of using bullet direction
            Vector3 randomHorizontalDirection = new Vector3(
                Random.Range(-1f, 1f), 
                0, 
                Random.Range(-1f, 1f)
            ).normalized;
            
            Vector3 horizontalForce = randomHorizontalDirection * hitForce;
            
            // Add random variation to upward force
            float randomUpwardForce = upwardForce + Random.Range(-upwardVariation, upwardVariation);
            Vector3 upwardImpulse = Vector3.up * randomUpwardForce;
            
            Debug.Log($"Random horizontal force: {horizontalForce}, Variable upward force: {upwardImpulse}");
            
            // Apply forces separately
            rb.AddForce(horizontalForce, ForceMode.Impulse);
            rb.AddForce(upwardImpulse, ForceMode.Impulse);
            
            // Add some rotation for realism (reduce with multiple hits to prevent excessive spinning)
            float rotationMultiplier = Mathf.Max(0.3f, 1f / hitCount);
            Vector3 randomTorque = new Vector3(
                Random.Range(-2f, 2f), 
                Random.Range(-2f, 2f), 
                Random.Range(-2f, 2f)
            ) * rotationMultiplier;
            rb.AddTorque(randomTorque, ForceMode.Impulse);
            
            Debug.Log("Random forces applied successfully!");
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
        
        // Notify the game manager for scoring
        if (gameManager != null)
        {
            gameManager.OnCanHit();
        }
        
        Debug.Log($"Can hit processing complete! Total hits on this can: {hitCount}");
    }
    
    public void SetGameManager(ShootingGameManager manager)
    {
        gameManager = manager;
    }
    
    public int GetHitCount()
    {
        return hitCount;
    }
}