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
    public int fruitsSliced = 0;
    public bool GameStarted = false;

    float countdown = 4;


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
        }
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
        fruitsSliced++;
    }

    public void StartGame()
    {
        fruitSpawner.StartSpawning();
        score = 0;
        countdown = 4;
        fruitsSliced = 0;
        lives = 3;
        UI.LivesText.text = lives.ToString();
        UI.ScoreText.text = fruitsSliced.ToString();
        GameStarted = true;
    }

    public void StopGame()
    {
        fruitSpawner.StopSpawning();
        GameStarted = false;
        UI.MainText.text = " ";
    }
}
