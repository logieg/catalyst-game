using UnityEngine;

/// <summary>
/// Player controller script to handle raycasts and platformer movement
/// </summary>
// Based on the 2D platformer controller by Sebastian Lague
[RequireComponent (typeof (BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    // The layer mask to use for object collisions
    public LayerMask collisionMask;

    // The width of the player's "skin" (padding for raycasting)
    const float skinWidth = .015f;

    // The maximum angle of slope the player can climb or descend
    public float maxSlopeAngle = 38.0f;
    public float maxDescendAngle = 38.0f;

    // The number of rays in each direction
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    // The spacing between rays in each direction along the player collider
    float horizontalRaySpacing;
    float verticalRaySpacing;

    /// <summary>
    /// The physics collider to use for the player
    /// </summary>
    BoxCollider2D playerCollider;
    /// <summary>
    /// A struct containing the raycast origin points based on the collider bounds
    /// </summary>
    RaycastOrigins raycastOrigins;
    /// <summary>
    /// A struct containing the collision info in all four directions around the player
    /// </summary>
    public CollisionInfo collisions;

    // Start is called before the first frame update
    void Start()
    {
        // Setup for raycasting
        playerCollider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    /// <summary>
    /// Attempt to move the player based on an intended velocity vector, with raycast-based collision handling
    /// </summary>
    /// <param name="velocity">The intended velocity vector for player movement</param>
    public void Move(Vector3 velocity)
    {
        // Update raycasting and collision information
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.velocityOld = velocity;

        // Try handling slope descending if player is moving down
        if (velocity.y < 0)
            DescendSlope(ref velocity);

        // Handle collisions and update the velocity if needed
        if (velocity.x != 0)
            HorizontalCollisions(ref velocity);
        if (velocity.y != 0)
            VerticalCollisions(ref velocity);

        // Apply the movement
        transform.Translate(velocity);
    }

    /// <summary>
    /// Handle horizontal collisions and update the velocity vector if necessary
    /// </summary>
    /// <param name="velocity">A reference to the intended player velocity vector</param>
    void HorizontalCollisions(ref Vector3 velocity)
    {
        // Get movement direction and ray length based on the velocity vector
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

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
    /// Get collider bounds and shrink inward to create "skin" (padding for raycasting)
    /// </summary>
    /// <returns>The player's collider Bounds struct that has been shrunk to create "skin" padding</returns>
    Bounds GetRaycastBounds()
    {
        Bounds bounds = playerCollider.bounds;
        bounds.Expand(skinWidth * -2);
        return bounds;
    }

    /// <summary>
    /// Update the raycast origin points based on the collider bounds
    /// </summary>
    void UpdateRaycastOrigins()
    {
        Bounds bounds = GetRaycastBounds();

        // Set raycast origins
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    /// <summary>
    /// Calculate and set the spacing between rays in each direction
    /// </summary>
    void CalculateRaySpacing()
    {
        Bounds bounds = GetRaycastBounds();

        // Clamp the ray counts (cannot equal 0 or 1)
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        // Calculate the ray spacing from the adjusted bounds and ray counts
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    /// <summary>
    /// Struct containing Vector2 raycast origin points
    /// </summary>
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
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
        public void Reset() {
            above = below = left = right = climbingSlope = descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
