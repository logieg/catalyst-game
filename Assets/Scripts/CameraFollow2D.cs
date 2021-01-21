using UnityEngine;

/// <summary>
/// 2D camera follow script to track a player with look-ahead and camera movement smoothing
/// </summary>
// Based on the 2D camera follow script by Sebastian Lague
public class CameraFollow2D : MonoBehaviour
{
    /// <summary>
    /// The player that the camera will follow
    /// </summary>
    public PlayerController followTarget;

    /// <summary>
    /// The vertical offset of the camera
    /// </summary>
    public float verticalOffset;

    /// <summary>
    /// The distance to look ahead on the X axis
    /// </summary>
    public float lookAheadDistanceX;
    /// <summary>
    /// The amount of time taken for horizontal smoothing
    /// </summary>
    public float lookSmoothTimeX;
    /// <summary>
    /// The amount of time taken for vertical smoothing
    /// </summary>
    public float lookSmoothTimeY;

    /// <summary>
    /// The size of the focus area, within which the player can move without camera motion
    /// </summary>
    public Vector2 focusAreaSize;

    // Information for the active focus area
    FocusArea focusArea;

    // Internal look-ahead and smoothing variables
    float currentLookAheadX;
    float targetLookAheadX;
    float lookAheadDirectionX;
    float smoothLookVelocityX;
    float smoothLookVelocityY;
    bool lookAheadStopped;


    // Start is called before the first frame update
    void Start()
    {
        // Set up an active focus area based on the follow target bounds
        focusArea = new FocusArea(followTarget.collider.bounds, focusAreaSize);
    }

    // LateUpdate is called once per frame after all Updates and FixedUpdates
    void LateUpdate()
    {
        // Update the active focus area based on the current follow target bounds
        focusArea.Update(followTarget.collider.bounds);

        // Get the position of the camera focus (including vertical offset)
        Vector2 focusPosition = focusArea.center + Vector2.up * verticalOffset;

        // Check if the focus area is moving
        if (focusArea.velocity.x != 0)
        {
            lookAheadDirectionX = Mathf.Sign(focusArea.velocity.x);

            // If player is currently pushing against focus area, update the look-ahead target
            if (Mathf.Sign(followTarget.playerInput.x) == Mathf.Sign(focusArea.velocity.x) && followTarget.playerInput.x != 0)
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirectionX * lookAheadDistanceX;
            }
            // If player is not pushing against focus area, stop the look ahead part-way
            else if (!lookAheadStopped)
            {
                lookAheadStopped = true;
                targetLookAheadX = currentLookAheadX + (lookAheadDirectionX * lookAheadDistanceX - currentLookAheadX) / 4f;
            }
        }

        // Apply horizontal camera smoothing
        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);
        focusPosition += Vector2.right * currentLookAheadX;

        // Apply vertical camera smoothing
        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothLookVelocityY, lookSmoothTimeY);

        // Update the camera's position with the new focus position
        transform.position = (Vector3)focusPosition + Vector3.forward * -10;  // Add -10 in the Z direction to ensure camera is on the top layer
    }

    // Draw the focus area as a semi-transparent box in the editor using debug gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.15f);
        Gizmos.DrawCube(focusArea.center, focusAreaSize);
    }

    /// <summary>
    /// Struct to store information about a camera's focus area, within which the player can move without camera motion
    /// </summary>
    struct FocusArea
    {
        public Vector2 center;
        float left, right;
        float top, bottom;
        public Vector2 velocity;

        // Constructor for initial setup
        public FocusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;
            top = targetBounds.min.y + size.y;
            bottom = targetBounds.min.y;

            velocity = Vector2.zero;
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
        }

        /// <summary>
        /// Update the focus area information using the target's bounds and previous information
        /// </summary>
        public void Update(Bounds targetBounds)
        {
            // Update horizontal area info
            float shiftX = 0;
            if (targetBounds.min.x < left)
                shiftX = targetBounds.min.x - left;
            else if (targetBounds.max.x > right)
                shiftX = targetBounds.max.x - right;
            left += shiftX;
            right += shiftX;

            // Update vertical area info
            float shiftY = 0;
            if (targetBounds.min.y < bottom)
                shiftY = targetBounds.min.y - bottom;
            else if (targetBounds.max.y > top)
                shiftY = targetBounds.max.y - top;
            top += shiftY;
            bottom += shiftY;

            // Update area center and velocity
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
        }
    }
}
