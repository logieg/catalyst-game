using UnityEngine;

/// <summary>
/// 2D player controller script to handle raycasts and platformer movement
/// </summary>
// Based on the 2D platformer controller by Sebastian Lague
public class PlayerController : RaycastController
{
    // The maximum angle of slope the player can climb or descend
    public float maxSlopeAngle = 38.0f;
    public float maxDescendAngle = 38.0f;

    /// <summary>
    /// A struct containing the collision info in all four directions around the player
    /// </summary>
    public CollisionInfo collisions;

    // The directional input from the player
    [HideInInspector]
    public Vector2 playerInput;


    // Start is called before the first frame update
    public override void Start()
    {
        // RaycastController setup
        base.Start();

        // Set initial face direction
        collisions.faceDirection = 1;
    }

    /// <summary>
    /// No-input overload method for Move(velocity, input, onPlatform)
    /// </summary>
    public void Move(Vector3 velocity, bool onPlatform = false)
    {
        Move(velocity, Vector2.zero, onPlatform);
    }

    /// <summary>
    /// Attempt to move the player based on an intended velocity vector, with raycast-based collision handling
    /// </summary>
    /// <param name="velocity">The intended velocity vector for player movement</param>
    /// <param name="input">The directional input vector from the player</param>
    /// <param name="onPlatform">Set to true if the player is on a moving platform to ensure jumping is enabled</param>
    public void Move(Vector3 velocity, Vector2 input, bool onPlatform = false)
    {
        // Update raycasting, collision, and input information
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.velocityOld = velocity;
        playerInput = input;

        // Update face direction
        if (velocity.x != 0)
            collisions.faceDirection = (int)Mathf.Sign(velocity.x);

        // Try handling slope descending if player is moving down
        if (velocity.y < 0)
            DescendSlope(ref velocity);

        // Handle collisions and update the velocity if needed
        HorizontalCollisions(ref velocity);
        if (velocity.y != 0)
            VerticalCollisions(ref velocity);

        // Apply the movement
        transform.Translate(velocity);

        // Ensure jumping is enabled when on a platform
        if (onPlatform)
            collisions.below = true;
    }

    /// <summary>
    /// Handle horizontal collisions and update the velocity vector if necessary
    /// </summary>
    /// <param name="velocity">A reference to the intended player velocity vector</param>
    void HorizontalCollisions(ref Vector3 velocity)
    {
        // Get movement direction and ray length based on the velocity vector
        float directionX = collisions.faceDirection;
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        // If barely moving, set ray length to detect a wall in contact with the player (for wall jumping/sliding)
        if (Mathf.Abs(velocity.x) < skinWidth)
            rayLength = skinWidth * 2;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            // Calculate the raycast origin based on the movement direction and ray spacing
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);

            // Cast a collision ray using the collision mask
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            // (Debug) Visually draw the horizontal raycasting rays
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                // Skip collision handling if inside another collider (allow player to move out of it)
                if (hit.distance == 0)
                    continue;

                // Handle climbing a slope (within maximum slope angle)
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    // Switch from descending one slope to climbing another
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }

                    // Snap to slope if climbing a new slope
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }

                    // Perform the velocity adjustments for the slope climb
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                // Handle ordinary horizontal collisions (non-slope)
                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    // Adjust X velocity to be exactly the distance to the hit object
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    // Adjust ray length so it cannot hit another object further away
                    rayLength = hit.distance;

                    // Handle case where player collides with a wall on a slope
                    if (collisions.climbingSlope)
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);

                    // Set collision info
                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
            }
        }
    }
    
    /// <summary>
     /// Handle vertical collisions and update the velocity vector if necessary
     /// </summary>
     /// <param name="velocity">A reference to the intended player velocity vector</param>
    void VerticalCollisions(ref Vector3 velocity)
    {
        // Get movement direction and ray length based on the velocity vector
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            // Calculate the raycast origin based on the movement direction, ray spacing, and predicted X movement
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            // Cast a collision ray using the collision mask
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            // (Debug) Visually draw the vertical raycasting rays
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                // Allow player to pass through one-way platforms (by skipping collision and velocity calculations)
                if (hit.collider.CompareTag("OneWay"))
                {
                    // Pass through platform if moving up or falling through
                    if (directionY == 1 || collisions.fallingThroughPlatform)
                        continue;

                    // Begin falling through platform when input is down
                    if (playerInput.y < -0.6f)
                    {
                        collisions.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", 0.4f);
                        continue;
                    }
                }

                // Adjust Y velocity to be exactly the distance to the hit object
                velocity.y = (hit.distance - skinWidth) * directionY;
                // Adjust ray length so it cannot hit another object further away
                rayLength = hit.distance;

                // Handle case where player collides with a ceiling on a slope
                if (collisions.climbingSlope)
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);

                // Set collision info
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }

        // Handle case where a slope intersects with another slope of a different angle
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // Check if about to climb a new slope
                if (slopeAngle != collisions.slopeAngle)
                {
                    // Snap to new slope, not old
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    /// <summary>
    /// Handle climbing a slope and update the velocity vector accordingly
    /// </summary>
    /// <param name="velocity">A reference to the intended player velocity vector</param>
    /// <param name="slopeAngle">The angle of the slope that was encountered</param>
    void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        // Check that a jump (or other vertical movement) doesn't override the vertical component of the slope climb
        if (velocity.y <= climbVelocityY)
        {
            // Adjust movement velocity to climb the slope
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

            // Update collision/slope info
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    /// <summary>
    /// Handle descending a slope and update the velocity vector accordingly
    /// </summary>
    /// <param name="velocity">A reference to the intended player velocity vector</param>
    void DescendSlope(ref Vector3 velocity)
    {
        // Cast a ray downward to detect a slope
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            // Check that slope angle is descendable
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
                // Check that slope is in the same direction as X movement
                if (Mathf.Sign(hit.normal.x) == directionX)
                    // Check that distance to slope is close enough to descend
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        // Adjust movement velocity to descend the slope
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        // Update collision/slope info
                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
        }
    }

    /// <summary>
    /// Invokable method to reset the collisions.fallingThroughPlatform flag to false
    /// </summary>
    void ResetFallingThroughPlatform()
    {
        collisions.fallingThroughPlatform = false;
    }

    /// <summary>
    /// Struct containing collision information (directional and slope collisions)
    /// </summary>
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool climbingSlope, descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;
        public int faceDirection;
        public bool fallingThroughPlatform;
        public void Reset() {
            above = below = left = right = climbingSlope = descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
