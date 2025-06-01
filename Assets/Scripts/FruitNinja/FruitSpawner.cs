using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public List<Transform> spawnPoints; 
    public List<GameObject> fruits;
    public Vector3 force; 
    
    private IEnumerator SpawnFruit(float time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            var rand = new System.Random();
            int numFruits = rand.Next(2, 5);
            for (int i = 0; i < numFruits; i++)
            {
                int index = rand.Next(0, fruits.Count);
                GameObject fruit = Instantiate(fruits[index], spawnPoints[i].position, Quaternion.identity);
                fruit.name = fruits[index].name;
                Rigidbody rb = fruit.GetComponent<Rigidbody>();
                rb.AddForce(force + new Vector3(Random.Range(0f, 0.1f), Random.Range(0f, 0.3f), Random.Range(0f, 0.1f)), ForceMode.Impulse);
                rb.AddTorque(new Vector3(Random.Range(2f, 6f), Random.Range(2f, 6f), Random.Range(2f, 6f)), ForceMode.Impulse);
                Destroy(fruit, 4);
            }

        }
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnFruit(4));
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
    }
}
