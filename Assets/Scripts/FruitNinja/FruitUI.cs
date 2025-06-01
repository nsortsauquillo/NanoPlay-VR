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


    private void Update()
    {
        LivesText.text = gameManager.lives.ToString();
        ScoreText.text = gameManager.score.ToString();
    }

}
