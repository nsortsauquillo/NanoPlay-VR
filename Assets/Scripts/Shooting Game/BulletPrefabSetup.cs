using UnityEngine;

public class BulletPrefabSetup : MonoBehaviour
{
    [ContextMenu("Setup Bullet Prefab")]
    public void SetupBullet()
    {
        // Add Bullet script
        if (GetComponent<Bullet>() == null)
        {
            gameObject.AddComponent<Bullet>();
        }
        
        // Add Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure rigidbody
        rb.mass = 0.01f;
        rb.useGravity = false; // You can enable this if you want bullets to drop
        rb.drag = 0f;
        rb.angularDrag = 0f;
        
        // Add collider (trigger)
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.01f;
            sphereCol.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
        
        // Add audio source
        if (GetComponent<AudioSource>() == null)
        {
            AudioSource audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.spatialBlend = 1f; // 3D sound
            audioSrc.playOnAwake = false;
        }
        
        Debug.Log("Bullet prefab setup complete!");
    }
}