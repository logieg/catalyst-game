using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple script to handle the shrink-based expiration of a poly particle
public class PolyParticle : MonoBehaviour
{
    public float expiration = 1.5f;
    public float shrinkRate = 0.08f;
    private float expireTimer = 0.0f;
    private bool expiring = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!expiring)
        {
            // Don't start expiring yet
            expireTimer += Time.deltaTime;
            if (expireTimer > expiration)
                expiring = true;
        }
    }

    private void FixedUpdate()
    {
        // Handle shrinking animation in FixedUpdate for consistency between editor and build
        if (expiring)
        {
            // Gradual shrink-death
            float shrinkMultiplier = 1.0f - shrinkRate;
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * shrinkMultiplier,
                gameObject.transform.localScale.y * shrinkMultiplier,
                gameObject.transform.localScale.z * shrinkMultiplier);
            // Final death
            if (gameObject.transform.localScale.x <= 0.05f)
                Destroy(gameObject);
        }
    }
}
