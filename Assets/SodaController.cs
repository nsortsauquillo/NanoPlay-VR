using System.Collections;
using UnityEngine;

public class SodaController : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeThreshold = 2.0f; // Acceleration needed to count as a shake
    public float rotationThreshold = 100.0f; // Angular velocity needed to count as a shake
    public float explosionForce = 700f; // Force applied to the cap

    [Header("References")]
    public GameObject cap;              // Reference to the bottle cap
    public ParticleSystem sodaSpray;    // Particle system for soda spray

    public GameObject liquid; // Material for the liquid inside the bottle

    private int shakeCount = 0;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool exploded = false;

    private float shakeCooldown = 1.0f; // Minimum time between shakes (seconds)
    private float shakeCooldownRange = 0.5f; // Random range for shake cooldown
    private float lastShakeTime = -1f;
    private float currentShakeCooldown = 1.0f; // Actual cooldown used after each shake


    private int shakesToExplode = 5;    // Number of shakes before explosion

    private Material liquidMaterial;

    // Cap shake variables
    private float capShakeTimer = 0f;
    private float capShakeDuration = 0f;
    private float capShakeMagnitude = 0f;
    private Vector3 capOriginalLocalPos;

    void Awake()
    {
        if (liquid != null)
        {
            liquidMaterial = liquid.GetComponent<Renderer>().material;
            if (liquidMaterial == null)
            {
                Debug.LogError("Liquid material not found on the liquid GameObject!");
            }
        }
        else
        {
            Debug.LogError("Liquid GameObject is not assigned!");
        }

        if (cap == null)
        {
            Debug.LogError("Cap GameObject is not assigned!");
        }

        if (sodaSpray == null)
        {
            Debug.LogError("Soda spray ParticleSystem is not assigned!");
        }
    }

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;

        // Random shake to explode
        //shakesToExplode = Random.Range(20, 40);

        // Initialize first cooldown
        currentShakeCooldown = shakeCooldown + Random.Range(-shakeCooldownRange, shakeCooldownRange);
    }

    void Update()
    {
        if (exploded) return;

        // Linear acceleration
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        float acceleration = (velocity - ((lastPosition - transform.position) / Time.deltaTime)).magnitude / Time.deltaTime;

        // Angular velocity
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        float angularVelocity = Mathf.Abs(angleInDegrees) / Time.deltaTime;

        Debug.Log($"Acceleration: {acceleration:F2}, Angular Velocity: {angularVelocity:F2}, Shake Count: {shakeCount}");

        if ((acceleration > shakeThreshold || angularVelocity > rotationThreshold) && (Time.time - lastShakeTime > currentShakeCooldown))
        {
            shakeCount++;
            lastShakeTime = Time.time;
            // Set a new random cooldown for the next shake
            currentShakeCooldown = shakeCooldown + Random.Range(-shakeCooldownRange, shakeCooldownRange);
            Debug.Log($"Shake detected! New shake count: {shakeCount}. Next cooldown: {currentShakeCooldown:F2}s");
            if (shakeCount >= shakesToExplode)
            {
                Debug.Log("Shake threshold reached! Exploding...");
                Explode();
            }
        }

        // --- Continuous cap shake effect based on remaining shakes ---
        if (cap != null && shakeCount < shakesToExplode)
        {
            float t = Mathf.Clamp01((float)shakeCount / (float)shakesToExplode);
            capShakeDuration = Mathf.Lerp(0.05f, 0.25f, t); // More intense as t increases
            capShakeMagnitude = Mathf.Lerp(0.01f, 0.05f, t);
            if (capOriginalLocalPos == Vector3.zero) capOriginalLocalPos = cap.transform.localPosition;
            capShakeTimer += Time.deltaTime;
            if (capShakeTimer > capShakeDuration) capShakeTimer = 0f;
            float shakeAmount = capShakeMagnitude * (1f - Mathf.Exp(-4f * t));
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount + Mathf.Lerp(0.01f, 0.08f, t);
            float z = Random.Range(-1f, 1f) * shakeAmount;
            cap.transform.localPosition = capOriginalLocalPos + new Vector3(x, y, z);
        }
        // Reset cap position if exploded or not shaking
        if ((cap == null || shakeCount >= shakesToExplode) && capOriginalLocalPos != Vector3.zero)
        {
            cap.transform.localPosition = capOriginalLocalPos;
        }
        // ------------------------------------------------------------

        // Linear fill for liquid material to hide exact explosion timing
        if (liquidMaterial != null && shakesToExplode > 0)
        {
            float t = Mathf.Clamp01((float)shakeCount / (float)shakesToExplode);
            // Linear fill: 0.3 at t=0, 0.7 at t=1
            float fill = Mathf.Lerp(0.5f, 0.7f, t);
            liquidMaterial.SetFloat("_Fill", fill);
            Debug.Log($"Liquid fill (linear, 0.3-0.7) updated: {fill:F2}");
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Explode()
    {
        exploded = true;
        if (cap != null)
        {
            // Displace the cap vertically before applying force
            cap.transform.position += Vector3.up * 0.1f; // Move cap up by 0.1 units

            Rigidbody rb = cap.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = cap.AddComponent<Rigidbody>();
                Debug.Log("Rigidbody added to cap at runtime.");
            }
            rb.isKinematic = false;
            rb.AddForce(transform.up * explosionForce);
            Debug.Log("Cap exploded with force!");
        }
        if (sodaSpray != null)
        {
            sodaSpray.Play();
            Debug.Log("Soda spray played!");
        }
        Debug.Log("Soda exploded!");
        // Optionally, disable further interaction
    }

    // --- AI/Manager-accessible method to simulate a shake ---
    public void SimulateShake()
    {
        if (exploded) return;
        shakeCount++;
        lastShakeTime = Time.time;
        currentShakeCooldown = shakeCooldown + Random.Range(-shakeCooldownRange, shakeCooldownRange);
        Debug.Log($"[AI] Simulated shake! New shake count: {shakeCount}. Next cooldown: {currentShakeCooldown:F2}s");
        if (shakeCount >= shakesToExplode)
        {
            Debug.Log("[AI] Shake threshold reached! Exploding...");
            Explode();
        }
    }
}