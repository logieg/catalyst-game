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

    // The list of pending passenger movements (constructed each tick by CalculatePassengerMovement)
    List<PassengerMovement> passengerMovement;

    // A cache of passenger PlayerControllers to avoid calling GetComponent() multiple times per tick (optimization)
    Dictionary<Transform, PlayerController> passengerDictionary = new Dictionary<Transform, PlayerController>();


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

        // Calculate pending movement for passengers
        CalculatePassengerMovement(velocity);

        // Apply passenger and platform movement
        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }

    /// <summary>
    /// Apply all pending passenger movements calculated by CalculatePassengerMovement()
    /// </summary>
    /// <param name="beforePlatformMovement">Whether the MovePassengers call occurs before platform movement is applied</param>
    void MovePassengers(bool beforePlatformMovement)
    {
        foreach (PassengerMovement passenger in passengerMovement)
        {
            // Cache the PlayerController component for a new passenger (optimization)
            if (!passengerDictionary.ContainsKey(passenger.transform))
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<PlayerController>());

            // Apply the pending passenger movement by calling the passenger's Move() method (ensuring collisions are handled)
            if (passenger.moveBeforePlatform == beforePlatformMovement)
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.onPlatform);
        }
    }

    /// <summary>
    /// Calculate the necessary movement for any passengers on the platform (ex. the player) to keep them on the platform
    /// </summary>
    /// <param name="velocity">The intended velocity vector for platform movement</param>
    void CalculatePassengerMovement(Vector3 velocity)
    {
        // The set of passengers that have been moved this tick, to prevent passengers from being moved twice
        HashSet<Transform> movedPassengers = new HashSet<Transform>();

        // The list of pending passenger movements (to be calculated in this method)
        passengerMovement = new List<PassengerMovement>();

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

                        // Push the passenger on the platform by adding a pending movement entry
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
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

                        // Push the passenger on the platform by adding a pending movement entry
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth;   // Forces player to do vertical collision checks while being pushed
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
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

                        // Push the passenger with the platform's movement by adding a pending movement entry
                        float pushX = velocity.x;
                        float pushY = velocity.y;
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Struct to hold information about pending passenger movement
    /// </summary>
    struct PassengerMovement
    {
        public Transform transform;         // The passenger's transform
        public Vector3 velocity;            // The passenger's intended movement
        public bool onPlatform;             // Whether the passenger is currently on the moving platform
        public bool moveBeforePlatform;     // Whether the passenger needs to move before or after the platform moves

        public PassengerMovement(Transform transform, Vector3 velocity, bool onPlatform, bool moveBeforePlatform)
        {
            this.transform = transform;
            this.velocity = velocity;
            this.onPlatform = onPlatform;
            this.moveBeforePlatform = moveBeforePlatform;
        }
    }
}
