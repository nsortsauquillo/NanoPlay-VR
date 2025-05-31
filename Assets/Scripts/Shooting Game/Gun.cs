using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] Transform firePoint;
    [SerializeField] float range = 100f;
    [SerializeField] float fireRate = 0.5f;
    [SerializeField] LayerMask targetLayer = -1; // Default to all layers
    
    [Header("Effects")]
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip fireSound;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private float lastFireTime;
    
    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(OnTriggerPulled);
        }
        
    }
    
    void OnTriggerPulled(ActivateEventArgs args)
    {
        Debug.Log("Trigger pulled!");
        if (Time.time >= lastFireTime + fireRate)
        {
            Debug.Log("Trigger pulled!");
            Fire();
            lastFireTime = Time.time;
        }
    }
    
    void Fire()
    {
        // Play effects
        if (muzzleFlash != null)
            muzzleFlash.Play();
            
        if (audioSource && fireSound)
            audioSource.PlayOneShot(fireSound);
        
        // Raycast
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
        {
            // Check if we hit a can
            Can can = hit.collider.GetComponent<Can>();
            if (can != null)
            {
                can.OnHit(hit.point, firePoint.forward);
            }
        }
        
        // Debug line to see where we're shooting
        Debug.DrawRay(firePoint.position, firePoint.forward * range, Color.red, 0.5f);
    }
    
    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnTriggerPulled);
        }
    }
}