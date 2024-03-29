﻿using UnityEngine;

/// <summary>
/// Generalized raycasting controller to handle multi-directional raycasting (ex. for raycast-based physics)
/// </summary>
// Based on the 2D platformer controller by Sebastian Lague
[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    /// <summary>
    /// The layer mask to use for object collisions
    /// </summary>
    public LayerMask collisionMask;

    /// <summary>
    /// The width of the object's "skin" (padding for raycasting)
    /// </summary>
    public const float skinWidth = .015f;

    /// <summary>
    /// The space between each ray, used to calculate ray spacing and ray counts
    /// </summary>
    public float distanceBetweenRays = .25f;

    // The number of rays in each direction
    [HideInInspector]
    public int horizontalRayCount = 4;
    [HideInInspector]
    public int verticalRayCount = 4;

    // The spacing between rays in each direction along the object's collider
    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    /// <summary>
    /// The physics collider to use for the object
    /// </summary>
    [HideInInspector]
    public new BoxCollider2D collider;

    /// <summary>
    /// A struct containing the raycast origin points based on the collider bounds
    /// </summary>
    public RaycastOrigins raycastOrigins;


    // Awake is called when the script is loaded, before Start
    public virtual void Awake()
    {
        // Get the collider of the object
        collider = GetComponent<BoxCollider2D>();
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        // Setup for raycasting
        CalculateRaySpacing();
    }

    /// <summary>
    /// Get collider bounds and shrink inward to create "skin" (padding for raycasting)
    /// </summary>
    /// <returns>The object's collider Bounds struct that has been shrunk to create "skin" padding</returns>
    public Bounds GetRaycastBounds()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);
        return bounds;
    }

    /// <summary>
    /// Update the raycast origin points based on the collider bounds
    /// </summary>
    public void UpdateRaycastOrigins()
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
    public void CalculateRaySpacing()
    {
        // Get bounds for raycasting
        Bounds bounds = GetRaycastBounds();
        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        // Calculate the ray counts based on the distance between rays
        horizontalRayCount = Mathf.RoundToInt(boundsHeight / distanceBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / distanceBetweenRays);

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
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
