using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public int points = 50; 

    public void Slice()
    {
        // Logic for slicing the fruit, e.g., play sound, particle effects, etc.
        // This method can be called when the fruit is sliced by the weapon.
        Destroy(this.gameObject); // Destroy the fruit object after slicing
    }
}
