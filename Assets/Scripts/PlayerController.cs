using UnityEngine;

/// <summary>
/// Player controller script to handle raycasts and platformer movement
/// </summary>
// Based on the 2D platformer controller by Sebastian Lague
[RequireComponent (typeof (BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
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


    // Start is called before the first frame update
    void Start()
    {
        playerCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Update raycasting information
        UpdateRaycastOrigins();
        CalculateRaySpacing();

        // DBUG> draw the raycasting rays
        for (int i = 0; i < verticalRayCount; i++)
            Debug.DrawRay(raycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.red);
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
}
