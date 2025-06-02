using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireRate = 0.5f;
    
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
        
        // Validate components
        if (firePoint == null)
        {
            Debug.LogError("FirePoint is not assigned on Gun!");
        }
        
        if (bulletPrefab == null)
        {
            Debug.LogError("BulletPrefab is not assigned on Gun!");
        }
    }
    
    void OnTriggerPulled(ActivateEventArgs args)
    {
        Debug.Log("Trigger pulled!");
        if (Time.time >= lastFireTime + fireRate)
        {
            Fire();
            lastFireTime = Time.time;
        }
    }
    
    void Fire()
    {
        if (firePoint == null || bulletPrefab == null)
        {
            Debug.LogError("Cannot fire: FirePoint or BulletPrefab is missing!");
            return;
        }
        
        // Play effects
        if (muzzleFlash != null)
            muzzleFlash.Play();
            
        if (audioSource && fireSound)
            audioSource.PlayOneShot(fireSound);
        
        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        Debug.Log($"Bullet fired from {firePoint.position} in direction {firePoint.forward}");
    }
    
    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnTriggerPulled);
        }
    }
}