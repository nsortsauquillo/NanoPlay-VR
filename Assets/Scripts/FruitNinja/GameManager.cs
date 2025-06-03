using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public FruitSpawner fruitSpawner;
    public FruitUI UI;
    public int score = 0;
    public int lives = 3;
    public bool GameStarted = false;


    private float countdown = 4;
    private float gameTimer = 0;

    [Header("Dificultad")]
    public int burstCount = 2;
    public float burstTime = 3f;
    public float bombChance = 0.1f;
    public float forceMultiplier = 1f;

    [SerializeField] TextMeshProUGUI highScoreText;
    [SerializeField] TextMeshProUGUI currentScoreText;

    private int highScore = 0;

    private void Update()
    {
        if (GameStarted)
        {
            gameTimer += Time.deltaTime;
            UpdateDifficulty();
            currentScoreText.text = "Current Score: " + score.ToString();
        }
    }

    public void UpdateDifficulty()
    {
        burstCount = Mathf.Clamp(1 + (int)(gameTimer / 30f), 3, 7);
        burstTime = Mathf.Lerp(3f, 1f, gameTimer / 120f); // 3s → 1s
        bombChance = Mathf.Clamp01(gameTimer / 90f);           // 0 → 1
        forceMultiplier = Mathf.Lerp(1f, 1.8f, gameTimer / 100f);
    }

    public void DecreaseLife()
    {
        lives--;
        if(lives == 0)
        {
            UI.MainText.text = "Game Over!";
            StopGame();
        }
    }
     
    public void IncreaseScore(int points)
    {
        score += points;
    }  

    public void StartGame()
    {
        if (GameStarted) return; 
        UI.mainPanel.SetActive(true);
        fruitSpawner.StartSpawning();
        score = 0;
        countdown = 4;
        lives = 3;
        UI.LivesText.text = lives.ToString();
        UI.ScoreText.text = score.ToString();
        GameStarted = true;
    }

    public void StopGame()
    {
        if (!GameStarted) return;
      
        fruitSpawner.StopSpawning();
        GameStarted = false;
        UI.MainText.text = " ";
        if (score > highScore)
        {
            highScore = score;
            
        }
        
        highScoreText.text = "High Score: " + highScore.ToString();
        UI.mainPanel.SetActive(false);
    }
}
