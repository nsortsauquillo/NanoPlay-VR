using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SodaShakeManager : MonoBehaviour
{
    public SodaController sodaController; // Assign in inspector

    private enum GameState { Waiting, PlayerTurn, AITurn, AIWaitForPlayer, Result }
    private GameState state = GameState.Waiting;

    private bool gameActive = false;
    private bool playerExploded = false;
    private bool aiExploded = false;
    private float aiTurnDelay = 1.5f;
    private float aiTurnTimer = 0f;
    private int turnCount = 0;

    // Track if player has released the soda this turn
    private bool playerReleasedThisTurn = false;

    // AI shake parameters
    private bool aiShaking = false;
    private float aiShakeDuration = 1.2f; // total shake time
    private float aiShakeElapsed = 0f;
    private float aiShakeAmplitude = 0.18f;
    private float aiShakeFrequency = 6f;

    // AI lift parameters
    private Vector3 aiLiftPositionOffset = new Vector3(0, 0.8f, 0); // How high to lift the soda
    private Vector3 aiOriginalPosition;
    private Quaternion aiOriginalRotation;

    // Track if player has grabbed after AI turn
    private bool playerGrabbedAfterAI = false;

    // Audio for win/lose
    public AudioClip playerWinClip;
    public AudioClip aiWinClip;
    private AudioSource audioSource;

    void Start()
    {
        state = GameState.Waiting;
        gameActive = false;
        playerExploded = false;
        aiExploded = false;
        aiTurnTimer = 0f;
        turnCount = 0;
        playerReleasedThisTurn = false;
        aiShaking = false;
        aiShakeElapsed = 0f;
        aiOriginalRotation = sodaController.transform.rotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Optionally, auto-start
        //StartGame();
    }

    void Update()
    {
        if (!gameActive) return;

        switch (state)
        {
            case GameState.PlayerTurn:
                if (sodaController == null) break;
                if (sodaController.exploded)
                {
                    playerExploded = true;
                    state = GameState.Result;
                }
                if (!sodaController.isGrabbed && !playerReleasedThisTurn)
                {
                    playerReleasedThisTurn = true;
                }
                if (playerReleasedThisTurn)
                {
                    state = GameState.AITurn;
                    aiTurnTimer = 0f;
                    playerReleasedThisTurn = false;
                }
                break;

            case GameState.AITurn:
                aiTurnTimer += Time.deltaTime;
                // Visualize AI turn: e.g., change soda color or log message
                if (sodaController != null)
                {
                    var rend = sodaController.GetComponent<Renderer>();
                    if (rend != null)
                        rend.material.color = Color.yellow;
                }
                if (aiTurnTimer < aiTurnDelay)
                {
                    Debug.Log("AI is thinking... (visual cue active)");
                }
                else
                {
                    // Restore rotation to original at the start of AI turn
                    sodaController.transform.rotation = aiOriginalRotation;
                    // AI starts shaking physically in the air
                    if (!aiShaking && !sodaController.exploded)
                    {
                        aiShaking = true;
                        aiShakeElapsed = 0f;

                        // Save original position/rotation to restore after AI turn
                        aiOriginalPosition = sodaController.transform.position;
                        // aiOriginalRotation is already set in Start and after each AI turn

                        // Lift the soda up in the air and freeze rotation for more control
                        Rigidbody rb = sodaController.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.velocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                            rb.useGravity = false;
                            rb.constraints = RigidbodyConstraints.FreezeRotation;
                            sodaController.transform.position = aiOriginalPosition + aiLiftPositionOffset;
                        }

                        // Optionally, "grab" the soda for AI
                        sodaController.isGrabbed = true;
                    }
                }

                // Smooth vertical shake using sine wave
                if (aiShaking && !sodaController.exploded)
                {
                    aiShakeElapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(aiShakeElapsed / aiShakeDuration);
                    float shake = Mathf.Sin(aiShakeElapsed * aiShakeFrequency * Mathf.PI * 2) * aiShakeAmplitude * (1f - t * 0.5f); // fade out a bit

                    Rigidbody rb = sodaController.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 basePos = aiOriginalPosition + aiLiftPositionOffset;
                        rb.MovePosition(basePos + Vector3.up * shake);
                    }

                    if (aiShakeElapsed >= aiShakeDuration || sodaController.exploded)
                    {
                        aiShaking = false;
                        // "Release" the soda for player to grab
                        sodaController.isGrabbed = false;

                        // Restore physics so the bottle can be grabbed and falls naturally
                        Rigidbody rbRelease = sodaController.GetComponent<Rigidbody>();
                        if (rbRelease != null)
                        {
                            rbRelease.useGravity = true;
                            rbRelease.constraints = RigidbodyConstraints.None;
                        }
                        // Reset rotation to original
                        sodaController.transform.rotation = aiOriginalRotation;

                        // Wait for player to grab and then release the bottle
                        state = GameState.AIWaitForPlayer;
                        playerGrabbedAfterAI = false;
                    }
                }
                break;

            case GameState.AIWaitForPlayer:
                // After AI's turn, just wait for player to grab and then release the bottle (do not keep it floating)
                if (sodaController.exploded)
                {
                    aiExploded = true;
                    state = GameState.Result;
                }
                else if (!playerGrabbedAfterAI && sodaController.isGrabbed)
                {
                    // Player grabbed the soda, now wait for them to release it
                    playerGrabbedAfterAI = true;
                }
                else if (playerGrabbedAfterAI && !sodaController.isGrabbed)
                {
                    // Player has released the soda after grabbing it
                    Rigidbody rb = sodaController.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.useGravity = true;
                        rb.constraints = RigidbodyConstraints.None;
                    }
                    // Restore color before giving back to player
                    var rend = sodaController.GetComponent<Renderer>();
                    if (rend != null)
                        rend.material.color = Color.white;
                    state = GameState.PlayerTurn;
                    playerReleasedThisTurn = false;
                    playerGrabbedAfterAI = false;
                }
                break;

            case GameState.Result:
                gameActive = false;
                if (playerExploded)
                {
                    Debug.Log("Player exploded! AI wins.");
                    if (aiWinClip != null && audioSource != null) audioSource.PlayOneShot(aiWinClip);
                }
                else if (aiExploded)
                {
                    Debug.Log("AI exploded! Player wins.");
                    if (playerWinClip != null && audioSource != null) audioSource.PlayOneShot(playerWinClip);
                }
                else
                {
                    Debug.Log("Game ended unexpectedly.");
                }
                break;
        }
    }

    public void StartGame()
    {
        state = GameState.PlayerTurn;
        gameActive = true;
        playerExploded = false;
        aiExploded = false;
        aiTurnTimer = 0f;
        turnCount = 0;
        playerReleasedThisTurn = false;
        aiShaking = false;
        aiShakeElapsed = 0f;
        Debug.Log("Soda Shake Game Started! Player goes first.");
    }

    void OnTriggerEnter(Collider other)
    {
        // Only start/restart if not already active
        if (!gameActive && other.CompareTag("Player"))
        {
            StartGame();
        }
    }
}