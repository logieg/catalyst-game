using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles a simple enemy type (ex. ratbeast) that uses a raycast to detect line-of-sight to the player within a certain radius and immediately chases them
public class EnemyScript : MonoBehaviour
{
    private Rigidbody body;

    public GameObject player;               // So it knows what to look for and what to chase
    public int attackPower = 1;             // The power at which the enemy hits the player
    public float chaseSpeed = 2.5f;         // The speed at which it chases the player
    public float detectionRange = 10.0f;    // The radius of the area where the player can be detected by a line-of-sight check
    public bool persistentChasing = false;  // Whether the enemy continues chasing the player when out of detection range or out of sight
    private bool chasing = false;           // Whether the enemy is currently in the chasing state

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() { }

    private void FixedUpdate()
    {
        // Find vector between player and enemy
        Vector3 distanceToPlayer = player.transform.position - transform.position;

        // Check if player is in range for a line-of-sight raycast
        if (distanceToPlayer.magnitude <= detectionRange)
        {
            RaycastHit lineOfSightHit;
            if (Physics.Raycast(transform.position, distanceToPlayer.normalized, out lineOfSightHit, detectionRange))
            {
                if (lineOfSightHit.collider.gameObject.CompareTag("Player"))
                    chasing = true;
                else if (!persistentChasing)
                    chasing = false;    // If the player is not directly in sight and the enemy is not a persistent chaser, give up chasing
            }
        }
        else if (!persistentChasing)
            chasing = false;            // If the player is out of the detection range and the enemy is not a persistent chaser, give up chasing

        // Apply movement-based mechanics in FixedUpdate for consistency between editor and build
        if (chasing)
        {
            // Rotate to look at the player
            transform.rotation = Quaternion.LookRotation(Vector3.up, -1 * distanceToPlayer.normalized); // Strange rotation parameters because of the way the model imported

            // Set the movement velocity based on the chase speed and direction to the player
            Vector3 moveVelocity = new Vector3(distanceToPlayer.normalized.x * chaseSpeed, 0, distanceToPlayer.normalized.z * chaseSpeed);
            if (moveVelocity.magnitude > chaseSpeed)
                moveVelocity = moveVelocity.normalized * chaseSpeed;    // Clamp the diagonal speed to the chase speed
            moveVelocity.y = body.velocity.y;                           // Add in the current rigidbody Y component so it still falls off things properly
            if (body.velocity.magnitude > chaseSpeed)
                moveVelocity = body.velocity;                           // If the current actual velocity is greater than the max speed, use that velocity instead

            // Apply the movement velocity
            body.velocity = moveVelocity;
        }
    }

}
