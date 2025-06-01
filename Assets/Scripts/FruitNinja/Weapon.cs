using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class Weapon : MonoBehaviour
{
    public GameManager gameManager;
    public FruitUI UI;

    public Transform startSlicePoint;
    public Transform endSlicePoint; 
    public LayerMask sliceableLayer;
    public VelocityEstimator estimator; 

    public Material crossSectionMaterial;
    public float cutForce = 2000;



    public void Update()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hitInfo, sliceableLayer);
        if (hasHit)
        {
            GameObject target = hitInfo.transform.gameObject;
            Slice(target);
        }
    }

    public void Slice(GameObject target)
    {
        Vector3 velocity = estimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity).normalized;
        planeNormal.Normalize();


        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            SetupSlicedComponent(upperHull);
            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);
            SetupSlicedComponent(lowerHull);

            Destroy(target);
        }
    }

    public void SetupSlicedComponent(GameObject slicedObj)
    {
        Rigidbody rb = slicedObj.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObj.AddComponent<MeshCollider>();
        collider.convex = true;
        rb.AddExplosionForce(cutForce, slicedObj.transform.position, 1f);
    }

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
