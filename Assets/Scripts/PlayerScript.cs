using UnityEngine;

/// <summary>
/// Main player script to handle movement and interactions
/// </summary>
// Includes sections based on the 2D platformer player script by Sebastian Lague
[RequireComponent (typeof (PlayerController))]
public class PlayerScript : MonoBehaviour
{
    // Public movement variables
    public float moveSpeed = 6.5f;                  // Speed of horizontal movement
    public float jumpHeight = 3.0f;                 // Max jump height in units
    public float timeToJumpApex = 0.4f;             // Time to reach the jump apex
    public float accelerationTimeAirborne = 0.08f;  // Time to change horizontal directions (in air)
    public float accelerationTimeGrounded = 0.02f;  // Time to change horizontal directions (on the ground)

    // Internal movement variables
    float gravity;
    float jumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    /// <summary>
    /// The PlayerController to use for controlling the player
    /// </summary>
    PlayerController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();

        // Calculate gravity and jump velocity
        gravity = (-2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2.0f);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    // FixedUpdate is called every fixed framerate frame
    // NOTE: Adjust Settings > Time > Fixed Timestep for high-framerate physics (0.01666666 for 60Hz)
    void FixedUpdate()
    {
        // Reset vertical velocity if a vertical collision occurs
        if (controller.collisions.above || controller.collisions.below)
            velocity.y = 0;

        // Get the directional input from the player (without smoothing)
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Apply jump from player input
        if (Input.GetAxisRaw("Jump") > 0.3f && controller.collisions.below)
            velocity.y = jumpVelocity;

        // Apply horizontal movement from player input
        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
            (controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne));

        // Apply gravity
        velocity.y += gravity * Time.fixedDeltaTime;

        // Attempt to move the player (and perform collision detection)
        controller.Move(velocity * Time.fixedDeltaTime);
    }
}
