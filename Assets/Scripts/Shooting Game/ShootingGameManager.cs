using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    [Header("Audio")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip synthwaveMusic;
    [SerializeField] AudioClip gameOverSound;
      // Game State
    private GameState currentState = GameState.Ready;
    private int currentScore = 0;
    private float gameTimer;
    private int currentPhase = 0;
    private bool gameStarted = false;
    private List<GameObject> activeCans = new List<GameObject>();
    private Coroutine canSpawnCoroutine;
    
    private Gun playerGun;
    
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
        }
    }
    
    void InitializeGame()
    {
        currentState = GameState.Ready;
        currentScore = 0;
        gameTimer = gameDuration;
        currentPhase = 0;
        
        playerGun = FindObjectOfType<Gun>();
        
        if (musicSource && synthwaveMusic)
        {
            musicSource.clip = synthwaveMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
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
    }    IEnumerator SpawnCansCoroutine()
    {
        while (currentState == GameState.Playing)
        {
            int cansToSpawn = Mathf.Min(currentPhase + 1, maxCans);
            
            ClearActiveCans();
            
            for (int i = 0; i < cansToSpawn && i < canSpawners.Length; i++)
            {
                SpawnCan(canSpawners[i]);
            }
            
            float spawnDelay = GetSpawnDelay();
            yield return new WaitForSeconds(spawnDelay);
        }
    }
      void SpawnCan(GameObject spawner)
    {
        if (canPrefab == null || spawner == null) return;
        
        GameObject newCan = Instantiate(canPrefab, spawner.transform.position, spawner.transform.rotation, canContainer);
        
        ConfigureCanForPhase(newCan);
        
        Can canScript = newCan.GetComponent<Can>();
        if (canScript != null)
        {
            // Aquí podrías agregar un evento personalizado para cuando la lata sea golpeada
        }
        
        activeCans.Add(newCan);
    }    void ConfigureCanForPhase(GameObject can)
    {
        // Game manager only ensures proper collider setup - no rigidbody modifications
        Collider canCollider = can.GetComponent<Collider>();
        if (canCollider != null)
        {
            canCollider.isTrigger = false; // Use collision detection, not trigger
        }
    }
      float GetSpawnDelay()
    {
        switch (currentPhase)
        {
            case 0: return 3f;
            case 1: return 2.5f;
            case 2: return 2f;
            case 3: return 1.5f;
            case 4: return 1f;
            default: return 3f;
        }
    }
    
    public void OnCanHit()
    {
        currentScore += 10;
        Debug.Log($"Can hit! Score: {currentScore}");
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
        
        Debug.Log($"Game Over! Final Score: {currentScore}");
        
        if (gameOverSound)
            musicSource.PlayOneShot(gameOverSound);
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
    
    // Public getters for external access
    public int GetCurrentScore() => currentScore;
    public float GetTimeRemaining() => gameTimer;
    public GameState GetCurrentState() => currentState;
    public bool IsGameActive() => currentState == GameState.Playing;
}