using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to manage sprite-like frame-by-frame animation on a 3D plane by using UV offsets (specifically for the player sprite)
public class PlayerSpriteAnimation : MonoBehaviour
{
    public float animationInterval = 0.125f;    // The interval between frames of animation
    public float animationThreshold = 0.15f;    // The threshold of movement at which to begin animating (if both X and Z velocities are < threshold, it doesn't animate)
    public float xVelocity,                     // Left/right movement velocity
        zVelocity;                              // Up/down movement velocity
    private bool animating = false;             // Whether the animation is currently playing or not
    private float animationTimer = 0.0f;        // The timer to track time between frames
    private int currentFrame = 0;               // The current frame the animation is on
    private int frameOffset = 0;                // The offset determines which pair of frames will be used (for direction)
    private Vector2[] frames = {                // The set of all available sprite frames (where each is a UV offset in the texture)
        new Vector2(0.000f,0),                  // Offset = 0 (up)
        new Vector2(0.125f,0),
        new Vector2(0.250f,0),                  // Offset = 1 (right)
        new Vector2(0.375f,0),
        new Vector2(0.500f,0),                  // Offset = 2 (down)
        new Vector2(0.625f,0),
        new Vector2(0.750f,0),                  // Offset = 3 (left)
        new Vector2(0.875f,0)
    };
    private Material mat;                       // The material used for animation

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        // Check to see if animation should happen
        if (Mathf.Abs(xVelocity) < animationThreshold && Mathf.Abs(zVelocity) < animationThreshold)
            animating = false;
        else
            animating = true;

        // Handle sprite animation
        if (animating)
        {
            animationTimer += Time.deltaTime;
            if (animationTimer > animationInterval)
            {
                // Determine what direction/offset to use
                if (xVelocity > animationThreshold)
                    frameOffset = 1; // Right
                else if (xVelocity < -1 * animationThreshold)
                    frameOffset = 3; // Left
                else if (zVelocity < -1 * animationThreshold)
                    frameOffset = 2; // Down
                else
                    frameOffset = 0; // Up

                // Change the animation frame
                mat.mainTextureOffset = frames[frameOffset * 2 + currentFrame];
                currentFrame = (currentFrame == 0 ? 1 : 0); // Toggle current frame values

                // Reset the animation timer
                animationTimer = 0.0f;
            }
        }
    }

}
