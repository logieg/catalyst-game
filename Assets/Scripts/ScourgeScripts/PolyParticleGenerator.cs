using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generates a pile of small polygon particles that quickly expire (for use in hit effects)
public class PolyParticleGenerator : MonoBehaviour
{
    public GameObject polyParticlePrefab;       // The poly particle object to spawn
    public int spawnCount = 6;                  // The number of poly particles to spawn
    public float spawnAreaRadius = 0.5f;        // The radius of the horizontal region where poly particles can spawn from
    public float horizontalLaunchForce = 80.0f; // The maximum horizontal force applied to each particle
    public float verticalLaunchForce = 800.0f;  // The maximum vertical force applied to each particle (based on a particle with rigidbody mass of 3)

    // Start is called before the first frame update
    void Start()
    {
        // Generate the poly particles
        for (int i = 0; i < spawnCount; i++)
        {
            // Spawn a particle
            GameObject particle = Instantiate(polyParticlePrefab, new Vector3(transform.position.x + Random.Range(-1 * spawnAreaRadius, spawnAreaRadius), transform.position.y,
                transform.position.z + Random.Range(-1 * spawnAreaRadius, spawnAreaRadius)), new Quaternion());
            // Launch the particle upwards and outwards
            particle.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1 * horizontalLaunchForce, horizontalLaunchForce), Random.Range(verticalLaunchForce / 4, verticalLaunchForce),
                Random.Range(-1 * horizontalLaunchForce, horizontalLaunchForce)));
        }

        // The generator's job is complete, so it's time to die
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
