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
    public float maxJumpHeight = 3.0f;              // Maximum jump height in units
    public float minJumpHeight = 1.0f;              // Minimum jump height in units
    public float timeToJumpApex = 0.4f;             // Time to reach the jump apex
    public float accelerationTimeAirborne = 0.08f;  // Time to change horizontal directions (in air)
    public float accelerationTimeGrounded = 0.02f;  // Time to change horizontal directions (on the ground)

    public Vector2 wallJumpClimb;                   // Velocity vector for a wall jump (climb jump)
    public Vector2 wallJumpOff;                     // Velocity vector for a wall jump (jump off)
    public Vector2 wallJumpLeap;                    // Velocity vector for a wall jump (leap jump)
    public float wallSlideSpeedMax = 2.2f;          // Maximum downward speed while wall sliding
    public float wallStickTime = 0.2f;              // Time to stick to a wall when wall sliding and input is away from the wall

    // Internal movement variables
    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    bool jumping = false;
    Vector3 velocity;
    float velocityXSmoothing;
    float timeToWallUnstick;

    /// <summary>
    /// The PlayerController to use for controlling the player
    /// </summary>
    PlayerController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();

        // Calculate gravity and jump velocities
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2.0f);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    // FixedUpdate is called every fixed framerate frame
    // NOTE: Adjust Settings > Time > Fixed Timestep for high-framerate physics (0.01666666 for 60Hz)
    void FixedUpdate()
    {
        // Get the directional input from the player (without smoothing)
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Apply horizontal movement from player input
        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
            (controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne));

        // Wall sliding behavior
        int wallDirX = controller.collisions.left ? -1 : 1;
        bool wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;

            // Clamp downward velocity to wall sliding speed
            if (velocity.y < -wallSlideSpeedMax)
                velocity.y = -wallSlideSpeedMax;

            // Stick to wall when wall sliding
            if (timeToWallUnstick > 0)
            {
                velocity.x = 0;
                velocityXSmoothing = 0;

                // Count down only when input is away from the wall
                if (wallDirX != (int)Mathf.Sign(input.x))
                    timeToWallUnstick -= Time.fixedDeltaTime;
                else
                    timeToWallUnstick = wallStickTime;
            }
            else
                timeToWallUnstick = wallStickTime;
        }

        // Apply jump from player input
        if (Input.GetAxisRaw("Jump") > 0.3f)
        {
            // Wall jump
            if (wallSliding)
            {
                jumping = true;
                if (Mathf.Abs(input.x) < 0.2f)
                {
                    // Wall jump: Jump off
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else if (wallDirX == (int)Mathf.Sign(input.x))
                {
                    // Wall jump: Climb jump (input towards wall)
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else
                {
                    // Wall jump: Leap jump (input away from wall)
                    velocity.x = -wallDirX * wallJumpLeap.x;
                    velocity.y = wallJumpLeap.y;
                }
            }

            // Regular jump from ground
            else if (controller.collisions.below)
            {
                jumping = true;
                velocity.y = maxJumpVelocity;
            }
        }

        // Variable jumping (only handled if no jump input and player was jumping)
        else if (jumping) {
            jumping = false;
            if (velocity.y > minJumpVelocity)
                velocity.y = minJumpVelocity;
        }

        // Apply gravity
        velocity.y += gravity * Time.fixedDeltaTime;

        // Attempt to move the player (and perform collision detection)
        controller.Move(velocity * Time.fixedDeltaTime, input);

        // Reset vertical velocity if a vertical collision occurs
        if (controller.collisions.above || controller.collisions.below)
            velocity.y = 0;
    }
}
