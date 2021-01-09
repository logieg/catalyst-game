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
                // Adjust X velocity to be exactly the distance to the hit object
                velocity.x = (hit.distance - skinWidth) * directionX;
                // Adjust ray length so it cannot hit another object further away
                rayLength = hit.distance;

                // Set collision info
                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
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

                // Set collision info
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
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
    /// Struct containing boolean collision information (whether a collision is occurring in each direction)
    /// </summary>
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public void Reset() { above = below = left = right = false; }
    }
}
