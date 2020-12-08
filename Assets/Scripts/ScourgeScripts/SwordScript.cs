using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles collisions between the sword hitbox and other objects
public class SwordScript : MonoBehaviour
{
    public int power = 1;                   // The power level of the player's sword attacks (used for breaking certain things and calculating damage)
    public float knockbackForce = 450.0f;   // The force applied after a hit to a rigidbody-equipped killable object (such as an enemy)

    // Start is called before the first frame update
    void Start()
    {
        // Disable the renderer and collider to start with
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Handle sword hits
    private void OnTriggerEnter(Collider other)
    {
        // Ensure other object is killable
        if (other.gameObject.CompareTag("Killable"))
        {
            // Call hit function for object to handle its own damage
            other.gameObject.GetComponent<KillableScript>().Hit(power);

            // If the object is rigidbody-equipped, apply a knockback force in the opposite direction
            Rigidbody otherBody;
            if (other.gameObject.TryGetComponent(out otherBody))
            {
                // Parent transform should be the player collider, so direction is relative to the player
                Vector3 knockbackDirection = (other.gameObject.transform.position - transform.parent.position).normalized;
                // Remove Y component so that knockback is purely horizontal
                knockbackDirection.y = 0.0f;
                // Apply the knockback force
                otherBody.AddForce(knockbackDirection * knockbackForce);
            }
        }
    }
}
