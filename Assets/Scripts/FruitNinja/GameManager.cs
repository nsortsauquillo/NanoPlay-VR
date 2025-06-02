using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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


    public void Start()
    {
        StartGame();
    }
    private void Update()
    {
        if (GameStarted)
        {
            if(countdown >= 0)
            {
                countdown -= Time.deltaTime;
                if(countdown > 1)
                {
                    UI.MainText.text = "Game starts in " + Mathf.CeilToInt(countdown).ToString();   
                }
                else
                {
                    UI.MainText.text = "Go!";
                }
            }
            else
            {
                gameTimer += Time.deltaTime;
                UpdateDifficulty();
            }
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
            StopGame();
            UI.MainText.text = "Game Over!";
        }
    }

    public void IncreaseScore(int points)
    {
        score += points;
    }

    public void StartGame()
    {
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
        fruitSpawner.StopSpawning();
        GameStarted = false;
        UI.MainText.text = " ";
    }
}
