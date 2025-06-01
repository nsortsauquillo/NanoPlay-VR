using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FruitUI : MonoBehaviour
{
    public GameManager gameManager;

    public GameObject mainPanel; 
    public TextMeshProUGUI MainText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI LivesText;
    public TextMeshProUGUI FruitsSlicedText;


    private void Update()
    {
        LivesText.text = gameManager.lives.ToString();
        FruitsSlicedText.text = gameManager.fruitsSliced.ToString();
        ScoreText.text = gameManager.score.ToString();
    }

}
