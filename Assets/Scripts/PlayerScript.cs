using UnityEngine;

/// <summary>
/// Main player script to handle movement and interactions
/// </summary>
// Includes sections based on the 2D platformer player script by Sebastian Lague
[RequireComponent (typeof (PlayerController))]
public class PlayerScript : MonoBehaviour
{
    // Movement variables
    float moveSpeed = 6;
    float gravity = -20;
    Vector3 velocity;

    /// <summary>
    /// The PlayerController to use for controlling the player
    /// </summary>
    PlayerController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    // FixedUpdate is called every fixed framerate frame
    // NOTE: Adjust Settings > Time > Fixed Timestep for high-framerate physics (0.01666666 for 60Hz)
    void FixedUpdate()
    {
        // Get the directional input from the player (without smoothing)
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Apply horizontal movement from player input
        velocity.x = input.x * moveSpeed;

        // Apply gravity
        velocity.y += gravity * Time.fixedDeltaTime;

        // Attempt to move the player (and perform collision detection)
        controller.Move(velocity * Time.fixedDeltaTime);
    }
}
