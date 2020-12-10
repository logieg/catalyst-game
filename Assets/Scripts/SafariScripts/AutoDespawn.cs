using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple script to automatically despawn an object after a specified number of seconds
/// </summary>
public class AutoDespawn : MonoBehaviour
{
    public float timeToDespawn = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeToDespawn -= Time.deltaTime;
        if (timeToDespawn < 0.0f)
            Destroy(gameObject);
    }
}
