using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple script to shrink and despawn things
/// </summary>
public class ShrinkDespawner : MonoBehaviour
{
    public float despawnTime = 2.5f;
    public float scaleRate = 0.05f;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = despawnTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
            timer -= Time.deltaTime;
        else
        {
            if (transform.localScale.magnitude > 0.02f)
                transform.localScale -= Vector3.one * scaleRate * Time.deltaTime;
            else
                Destroy(gameObject);
        }
    }
}
