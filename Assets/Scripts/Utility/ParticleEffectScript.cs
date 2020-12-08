using UnityEngine;

/// <summary>
/// Procedural particle effect generator
/// </summary>
public class ParticleEffectScript : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// A splash effect where particles are spawned and sent in random directions (requires prefab to have a Rigidbody2D)
    /// </summary>
    /// <param name="prefab">The particle prefab to use</param>
    /// <param name="location">The transform location to spawn the particle effect at</param>
    /// <param name="count">The number of particles to spawn</param>
    /// <param name="spawnSpread">The spread range of spawn positions</param>
    /// <param name="horizontalSpread">The spread range of horizontal movement force</param>
    /// <param name="verticalMin">The minimum vertical movement force</param>
    /// <param name="verticalMax">The maximum vertical movement force</param>
    public static void Splash(GameObject prefab, Transform location, int count = 25, float spawnSpread = 0.1f, float horizontalSpread = 75.0f, float verticalMin = 75.0f, float verticalMax = 300.0f)
    {
        for (int i = 0; i < count; i++)
        {
            // Spawn a particle with the provided prefab and spawn spread
            GameObject particle = Instantiate(prefab,
                new Vector2(location.position.x + Random.Range(-spawnSpread, spawnSpread), location.position.y + Random.Range(-spawnSpread, spawnSpread)),
                new Quaternion());
            // Give it a random force to send it flying
            particle.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-horizontalSpread, horizontalSpread), Random.Range(verticalMin, verticalMax)));
        }
    }
}
