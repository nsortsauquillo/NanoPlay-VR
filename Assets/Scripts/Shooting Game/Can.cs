using UnityEngine;

public class Can : MonoBehaviour
{
    [Header("Can Settings")]
    [SerializeField] float hitForce = 1.1f;      // Horizontal force
    [SerializeField] float upwardForce = 3.0f;   // Upward force - much smaller!
    [SerializeField] AudioClip hitSound;
    [SerializeField] ParticleSystem hitEffect;
    
    [Header("Physics Settings")]
    [SerializeField] float canMass = 0.3f;
    [SerializeField] float canDrag = 1.5f;
    [SerializeField] float canAngularDrag = 3f;
    
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool hasBeenHit = false;
    private ShootingGameManager gameManager;
    
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
        
        // Find the game manager
        gameManager = FindObjectOfType<ShootingGameManager>();
        
        Debug.Log($"Can initialized: {gameObject.name}");
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if can hit the floor/ground
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Floor") || 
            collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("Can hit the floor, destroying...");
            Destroy(gameObject);
        }
    }
    
    public void OnHit(Vector3 hitPoint, Vector3 hitDirection)
    {
        // Simple check to prevent multiple hits
        if (hasBeenHit)
        {
            Debug.Log("Can already hit previously, ignoring additional hit");
            return;
        }
        
        hasBeenHit = true;
        Debug.Log($"Can hit! Hit direction: {hitDirection}");
        
        // Apply impulse to create parabolic trajectory
        if (rb != null)
        {
            // Create horizontal force (remove Y component to keep it horizontal)
            Vector3 horizontalDirection = new Vector3(hitDirection.x, 0, hitDirection.z).normalized;
            Vector3 horizontalForce = horizontalDirection * hitForce;
            
            // Always apply upward force regardless of hit direction
            Vector3 upwardImpulse = Vector3.up * upwardForce;
            
            Debug.Log($"Horizontal force: {horizontalForce}, Upward force: {upwardImpulse}");
            
            // Apply forces separately
            rb.AddForce(horizontalForce, ForceMode.Impulse);
            rb.AddForce(upwardImpulse, ForceMode.Impulse);
            
            // Add some rotation for realism
            Vector3 randomTorque = new Vector3(
                Random.Range(-1f, 1f), 
                Random.Range(-1f, 1f), 
                Random.Range(-1f, 1f)
            );
            rb.AddTorque(randomTorque, ForceMode.Impulse);
            
            Debug.Log("Forces applied successfully!");
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
        
        Debug.Log("Can hit processing complete!");
    }
    
    void ResetHitFlag()
    {
        hasBeenHit = false;
    }
}