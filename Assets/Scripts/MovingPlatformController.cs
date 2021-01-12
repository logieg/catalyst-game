using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moving platform controller script to handle platforms that move and carry passengers (ex. the player)
/// </summary>
// Based on the 2D moving platform controller by Sebastian Lague
public class MovingPlatformController : RaycastController
{
    /// <summary>
    /// The layer mask to use for detecting passengers
    /// </summary>
    public LayerMask passengerMask;

    /// <summary>
    /// Vector3 representing the intended movement behavior of the moving platform
    /// </summary>
    public Vector3 move;

    // Start is called before the first frame update
    public override void Start()
    {
        // RaycastController setup
        base.Start();
    }

    // FixedUpdate is called every fixed framerate frame
    void FixedUpdate()
    {
        // Update raycasting information
        UpdateRaycastOrigins();

        // Adjust movement to per-tick velocity
        Vector3 velocity = move * Time.fixedDeltaTime;

        // Handle moving any passengers, then apply platform movement
        MovePassengers(velocity);
        transform.Translate(velocity);
    }

    /// <summary>
    /// Handle movement of any passengers on the platform (ex. the player) to keep them on the platform
    /// </summary>
    /// <param name="velocity">The intended velocity vector for platform movement</param>
    void MovePassengers(Vector3 velocity)
    {
        // The set of passengers that have been moved this tick, to prevent passengers from being moved twice
        HashSet<Transform> movedPassengers = new HashSet<Transform>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                // Calculate the raycast origin based on the movement direction and ray spacing
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);

                // Cast a collision ray using the passenger collision mask
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);

                        // Push the passenger on the platform
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;
                        hit.transform.Translate(new Vector3(pushX, pushY));
                    }
                }
            }
        }

        // Horizontally moving platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                // Calculate the raycast origin based on the movement direction and ray spacing
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);

                // Cast a collision ray using the passenger collision mask
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);

                        // Push the passenger on the platform
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = 0;
                        hit.transform.Translate(new Vector3(pushX, pushY));
                    }
                }
            }
        }

        // Make passenger stick to the surface of a horizontal or downward platform
        if ((velocity.y == 0 && velocity.x != 0) || directionY == -1)
        {
            float rayLength = skinWidth * 2;    // Passenger must be touching the platform

            for (int i = 0; i < verticalRayCount; i++)
            {
                // Cast a collision ray up using the passenger collision mask and ray spacing
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);

                        // Push the passenger with the platform's movement
                        float pushX = velocity.x;
                        float pushY = velocity.y;
                        hit.transform.Translate(new Vector3(pushX, pushY));
                    }
                }
            }
        }
    }
}
