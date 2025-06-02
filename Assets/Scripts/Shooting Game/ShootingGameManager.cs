using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShootingGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] float gameDuration = 120f;
    [SerializeField] int maxCans = 5;
    [SerializeField] float[] phaseTimings = { 10f, 25f, 40f, 60f };
    
    [Header("Game Objects")]
    [SerializeField] GameObject[] canSpawners;
    [SerializeField] GameObject canPrefab;
    [SerializeField] Transform canContainer;
    
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI highScoreText;
    [SerializeField] TextMeshProUGUI currentScoreText;
    
    [Header("Audio")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip synthwaveMusic;
    [SerializeField] AudioClip gameOverSound;
    [SerializeField] AudioClip newHighScoreSound;
    
    // Game State
    private GameState currentState = GameState.Ready;
    private int currentScore = 0;
    private int highScore = 0;
    private float gameTimer;
    private int currentPhase = 0;
    private bool gameStarted = false;
    private List<GameObject> activeCans = new List<GameObject>();
    private Coroutine canSpawnCoroutine;
    
    private Gun playerGun;
    
    // PlayerPrefs key for storing high score
    private const string HIGH_SCORE_KEY = "ShootingGame_HighScore";
    
    public enum GameState
    {
        Ready,
        Playing,
        GameOver
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void Update()
    {
        if (currentState == GameState.Playing)
        {
            UpdateGameTimer();
            UpdatePhase();
            CleanupDestroyedCans(); // Clean up null references
            UpdateCurrentScoreUI();
        }
    }
    
    void InitializeGame()
    {
        currentState = GameState.Ready;
        currentScore = 0;
        gameTimer = gameDuration;
        currentPhase = 0;
        
        // Load high score from PlayerPrefs
        LoadHighScore();
        
        // Update UI with loaded high score
        UpdateHighScoreUI();
        UpdateCurrentScoreUI();
        
        playerGun = FindObjectOfType<Gun>();
        
        if (musicSource && synthwaveMusic)
        {
            musicSource.clip = synthwaveMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    
    void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        Debug.Log($"Loaded high score: {highScore}");
    }
    
    void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
        Debug.Log($"Saved new high score: {highScore}");
    }
    
    void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }
    
    void UpdateCurrentScoreUI()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = $"Score: {currentScore}";
        }
    }

    
    bool CheckForNewHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            UpdateHighScoreUI();
            return true;
        }
        return false;
    }
    
    public void StartMinigame()
    {
        StartGame();
    }
    
    void StartGame()
    {
        currentState = GameState.Playing;
        gameStarted = true;
        gameTimer = gameDuration;
        currentScore = 0;
        currentPhase = 0;
        
        UpdateCurrentScoreUI();
        
        Debug.Log("Minigame started!");
        
        canSpawnCoroutine = StartCoroutine(SpawnCansCoroutine());
    }
    
    void UpdateGameTimer()
    {
        gameTimer -= Time.deltaTime;
        
        if (gameTimer <= 0f)
        {
            gameTimer = 0f;
            EndGame();
        }
    }
    
    void UpdatePhase()
    {
        int newPhase = GetCurrentPhase();
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            Debug.Log($"Fase cambiada a: {currentPhase + 1}");
        }
    }
    
    int GetCurrentPhase()
    {
        float elapsedTime = gameDuration - gameTimer;
        
        for (int i = phaseTimings.Length - 1; i >= 0; i--)
        {
            if (elapsedTime >= phaseTimings[i])
                return i + 1;
        }
        return 0;
    }
    
    IEnumerator SpawnCansCoroutine()
    {
        while (currentState == GameState.Playing)
        {
            int cansToSpawn = Mathf.Min(currentPhase + 1, maxCans);
            
            // Count how many cans are currently active (not destroyed)
            int currentActiveCans = GetActiveCanCount();
            
            // Only spawn new cans if we're below the maximum
            int cansNeeded = cansToSpawn - currentActiveCans;
            
            for (int i = 0; i < cansNeeded && i < canSpawners.Length; i++)
            {
                SpawnCan(canSpawners[i % canSpawners.Length]); // Use modulo to cycle through spawners
            }
            
            float spawnDelay = GetSpawnDelay();
            yield return new WaitForSeconds(spawnDelay);
        }
    }
    
    int GetActiveCanCount()
    {
        int count = 0;
        foreach (GameObject can in activeCans)
        {
            if (can != null) count++;
        }
        return count;
    }
    
    void CleanupDestroyedCans()
    {
        // Remove null references from the list (cans that hit the floor and were destroyed)
        activeCans.RemoveAll(can => can == null);
    }
    
    void SpawnCan(GameObject spawner)
    {
        if (canPrefab == null || spawner == null) return;
        
        GameObject newCan = Instantiate(canPrefab, spawner.transform.position, spawner.transform.rotation, canContainer);
        
        ConfigureCanForPhase(newCan);
        
        Can canScript = newCan.GetComponent<Can>();
        if (canScript != null)
        {
            // Set reference to this game manager
            canScript.SetGameManager(this);
        }
        
        activeCans.Add(newCan);
        Debug.Log($"Spawned can at {spawner.name}. Total active cans: {GetActiveCanCount()}");
    }
    
    void ConfigureCanForPhase(GameObject can)
    {
        Rigidbody rb = can.GetComponent<Rigidbody>();
        Collider canCollider = can.GetComponent<Collider>();
        
        if (canCollider != null)
        {
            canCollider.isTrigger = false; // Use collision detection, not trigger
        }
        
        if (rb == null) return;
        
        // Configure physics based on current phase
        switch (currentPhase)
        {
            case 0: // 0-10s: Lenta, cae recta
                rb.mass = 1f;
                rb.drag = 2f;
                break;
                
            case 1: // 10-25s: Más rápida
                rb.mass = 1f;
                rb.drag = 1f;
                break;
                
            case 2: // 25-40s: Movimiento lateral
                rb.mass = 1f;
                rb.drag = 1f;
                // Agregar fuerza lateral aleatoria pequeña
                rb.AddForce(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)), ForceMode.Impulse);
                break;
                
            case 3: // 40-60s: Más pesada, cae más rápido
                rb.mass = 1.3f;
                rb.drag = 0.8f;
                break;
                
            case 4: // 60+s: Rebote exagerado
                rb.mass = 1f;
                rb.drag = 0.5f;
                // Create bouncy physics material
                PhysicMaterial bouncyMat = new PhysicMaterial("Bouncy");
                bouncyMat.bounciness = 0.8f;
                bouncyMat.frictionCombine = PhysicMaterialCombine.Minimum;
                bouncyMat.bounceCombine = PhysicMaterialCombine.Maximum;
                canCollider.material = bouncyMat;
                break;
        }
    }
    
    float GetSpawnDelay()
    {
        switch (currentPhase)
        {
            case 0: return 4f;   // Slower spawning in early phases
            case 1: return 3f;
            case 2: return 2.5f;
            case 3: return 2f;
            case 4: return 1.5f;
            default: return 4f;
        }
    }
    
    public void OnCanHit()
    {
        currentScore += 10;
        Debug.Log($"Can hit! Score: {currentScore}");
        UpdateCurrentScoreUI();
    }
    
    public void OnCanDestroyed(GameObject can)
    {
        // Remove the can from our active list when it's destroyed (hits floor)
        if (activeCans.Contains(can))
        {
            activeCans.Remove(can);
            Debug.Log($"Can destroyed. Remaining active cans: {GetActiveCanCount()}");
        }
    }
    
    void ClearActiveCans()
    {
        foreach (GameObject can in activeCans)
        {
            if (can != null)
                Destroy(can);
        }
        activeCans.Clear();
    }
    
    void EndGame()
    {
        currentState = GameState.GameOver;
        gameStarted = false;
        
        if (canSpawnCoroutine != null)
            StopCoroutine(canSpawnCoroutine);
        
        ClearActiveCans();
        
        // Check for new high score
        bool isNewHighScore = CheckForNewHighScore();
        
        Debug.Log($"Game Over! Final Score: {currentScore}");
        
        if (isNewHighScore)
        {
            Debug.Log("NEW HIGH SCORE!");
            
            // Play special sound for new high score
            if (newHighScoreSound && musicSource)
            {
                musicSource.PlayOneShot(newHighScoreSound);
            }
            
            // You could add visual effects here like confetti, screen flash, etc.
            StartCoroutine(HighScoreAnimation());
        }
        else
        {
            // Play regular game over sound
            if (gameOverSound && musicSource)
            {
                musicSource.PlayOneShot(gameOverSound);
            }
        }
    }
    
    IEnumerator HighScoreAnimation()
    {
        // Simple high score text animation
        if (highScoreText != null)
        {
            Color originalColor = highScoreText.color;
            
            // Flash the high score text
            for (int i = 0; i < 3; i++)
            {
                highScoreText.color = Color.yellow;
                yield return new WaitForSeconds(0.3f);
                highScoreText.color = originalColor;
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
    
    public void RestartMinigame()
    {
        InitializeGame();
    }
    
    public void OnBulletFired()
    {
        // Aquí puedes agregar lógica adicional cuando se dispara
    }
    
    public void SetWeaponReference(Gun gun)
    {
        playerGun = gun;
    }
    
    // Public methods for external access to scores
    public int GetCurrentScore() => currentScore;
    public int GetHighScore() => highScore;
    public float GetTimeRemaining() => gameTimer;
    public GameState GetCurrentState() => currentState;
    public bool IsGameActive() => currentState == GameState.Playing;
    
    // Public method to manually reset high score (for testing or settings menu)
    public void ResetHighScore()
    {
        highScore = 0;
        SaveHighScore();
        UpdateHighScoreUI();
        Debug.Log("High score reset to 0");
    }
}