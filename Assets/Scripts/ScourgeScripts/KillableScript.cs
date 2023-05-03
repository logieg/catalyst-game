using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillableScript : MonoBehaviour
{
    public int health = 1;                      // How many hits it takes to kill this object
    public float hitDelay = 0.1f;               // The minimum delay between possible hits (to prevent a continuous collider from instantly killing a high-health object)
    private bool canBeHit = true;               // Whether the object can currently be hit
    public int powerRequirement = 1;            // The minimum attack power required to damage this object
    public GameObject polyParticleGenerator;    // The poly particle generator prefab to use when the object is killed
    public AudioClip hitSound;                  // The hit sound to play when damaged
    public AudioClip deathSound;                // The death sound to play when killed
    public bool preserveAfterDeath = false;     // Whether to keep the GameObject after death or not (if it isn't destroyed the object can handle its own death behavior)
    private bool alive = true;                  // Whether the object is currently alive or not (affects whether the script is enabled)

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    // Handle a hit to the killable object (only if it can currently be hit)
    public void Hit(int power)
    {
        // Cancel the whole function if the object can't be hit or the killablescript isn't active
        if (!canBeHit || !alive)
            return;

        // Disable hits to the object for a short time
        canBeHit = false;

        // If power requirement met, do damage based on attack power level (and power requirement functions as armor)
        if (power >= powerRequirement && health > 0)
            health -= (power - (powerRequirement - 1));

        // Handle death or recovery
        if (health <= 0)
        {
            // Kill the killable object (with fancy particle effects provided by the generator)
            alive = false;
            if (polyParticleGenerator != null)
            {
                GameObject particleFX = Instantiate(polyParticleGenerator);
                particleFX.transform.position = transform.position;
                particleFX.transform.rotation = transform.rotation;
            }
            // Play the death sound
            if (deathSound != null)
                SoundEffectScript.PlaySoundEffect(deathSound, 1.0f, transform.position);
            // If preserve isn't set, destroy the object completely
            if (!preserveAfterDeath)
                Destroy(gameObject);
        }
        else
        {
            // Not dead yet, so start recovery
            StartCoroutine(HitRecover());

            // Play the hit sound
            if (hitSound != null)
                SoundEffectScript.PlaySoundEffect(hitSound, 1.0f, transform.position);
        }
    }

    // Allows the object to be hit again after a short recovery delay
    public IEnumerator HitRecover()
    {
        yield return new WaitForSeconds(hitDelay);
        canBeHit = true;
    }

    public bool IsAlive()
    {
        return alive;
    }

    public void SetAlive(bool val)
    {
        alive = val;
    }
}
