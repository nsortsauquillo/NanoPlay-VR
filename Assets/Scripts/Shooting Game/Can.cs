using UnityEngine;

public class Can : MonoBehaviour
{
    [Header("Can Settings")]
    [SerializeField] float hitForce = 3.0f;
    [SerializeField] float upwardForce = 8.0f;
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
    private float hitTime = 0f;
    
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
        // Extra safety check with timing to prevent rapid multiple hits
        float currentTime = Time.time;
        if (hasBeenHit && currentTime - hitTime < 0.1f) 
        {
            Debug.Log($"Can hit ignored - too soon after last hit. Time diff: {currentTime - hitTime}");
            return; // Prevent multiple hits within 0.1 seconds
        }
        
        if (hasBeenHit)
        {
            Debug.Log("Can already hit previously, ignoring additional hit");
            return;
        }
        
        hasBeenHit = true;
        hitTime = currentTime;
        Debug.Log($"Can hit for the first time at time {hitTime}!");
        
        // Apply impulse to create parabolic trajectory
        if (rb != null)
        {
            // Create a more pronounced horizontal force for parabolic motion
            Vector3 horizontalForce = new Vector3(hitDirection.x, 0, hitDirection.z).normalized * hitForce;
            
            // Strong upward force to create the arc
            Vector3 upwardImpulse = Vector3.up * upwardForce;
            
            // Apply forces separately for better control
            rb.AddForce(horizontalForce, ForceMode.Impulse);
            rb.AddForce(upwardImpulse, ForceMode.Impulse);
            
            // Optional: Add a slight random rotation for more realistic movement
            Vector3 randomTorque = new Vector3(
                Random.Range(-2f, 2f), 
                Random.Range(-2f, 2f), 
                Random.Range(-2f, 2f)
            );
            rb.AddTorque(randomTorque, ForceMode.Impulse);
            
            Debug.Log($"Applied horizontal impulse: {horizontalForce}, upward impulse: {upwardImpulse}");
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