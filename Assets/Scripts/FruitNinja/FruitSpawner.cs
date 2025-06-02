using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public List<Transform> spawnPoints; 
    public List<GameObject> fruits;
    public GameManager gameManager;
    private IEnumerator SpawnFruit()
    {
        while (true)
        {
            yield return new WaitForSeconds(gameManager.burstTime);
            
            for(int i = 0; i< gameManager.burstCount; i++)
            {
                GameObject prefabToSpawn;
                if (Random.value < gameManager.bombChance)
                {
                    prefabToSpawn = fruits[fruits.Count - 1]; 
                }
                else
                {
                    int randomIndex = Random.Range(0, fruits.Count - 1); 
                    prefabToSpawn = fruits[randomIndex];
                }

                Transform spawnPoint = spawnPoints[i % spawnPoints.Count];
                GameObject obj = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
                obj.name = prefabToSpawn.name;

                Rigidbody rb = obj.GetComponent<Rigidbody>();

               
                Vector3 baseDirection = new Vector3(
                    0f,  
                    Random.Range(0.5f, 2.5f),                        
                    0f   
                ).normalized;

                float forceMagnitude = 8f * gameManager.forceMultiplier; 

                Vector3 launchForce = baseDirection * forceMagnitude;
                rb.AddForce(launchForce, ForceMode.Impulse);

                rb.AddTorque(new Vector3(
                    Random.Range(2f, 6f),
                    Random.Range(2f, 6f),
                    Random.Range(2f, 6f)
                ), ForceMode.Impulse);

                Destroy(obj, 4f);
            }
            

        }
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnFruit());
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
    }
}
