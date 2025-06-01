using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameManager gameManager;
    public FruitUI UI;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Fruit"))
        {
            Fruit fruit = collision.gameObject.GetComponent<Fruit>();
            if (fruit != null)
            {
                gameManager.IncreaseScore(fruit.points);
                //UI.ScoreText.text = gameManager.score.ToString();
                fruit.Slice();
            }
        }
        else if (collision.gameObject.CompareTag("Bomb"))
        {
            gameManager.DecreaseLife();
            //UI.LivesText.text = gameManager.lives.ToString();
            Destroy(collision.gameObject);
        }
    }

}
