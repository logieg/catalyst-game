using UnityEngine;

/// <summary>
/// 2D player controller script to handle raycasts and platformer movement
/// </summary>
// Based on the 2D platformer controller by Sebastian Lague
public class PlayerController : RaycastController
{
    // The maximum angle of slope the player can climb or descend
    public float maxSlopeAngle = 38.0f;

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

        // Set initial face direction based on X scale sign
        collisions.faceDirection = gameObject.transform.localScale.x > 0 ? 1 : -1;
    }

    /// <summary>
    /// No-input overload method for Move(moveAmount, input, onPlatform)
    /// </summary>
    public void Move(Vector2 moveAmount, bool onPlatform = false)
    {
        Move(moveAmount, Vector2.zero, onPlatform);
    }

    /// <summary>
    /// Attempt to move the player based on an intended movement vector, with raycast-based collision handling
    /// </summary>
    /// <param name="moveAmount">The intended movement vector for player movement (already adjusted for delta time)</param>
    /// <param name="input">The directional input vector from the player</param>
    /// <param name="onPlatform">Set to true if the player is on a moving platform to ensure jumping is enabled</param>
    public void Move(Vector2 moveAmount, Vector2 input, bool onPlatform = false)
    {
        // Update raycasting, collision, and input information
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.moveAmountOld = moveAmount;
        playerInput = input;

        // Try handling slope descending if player is moving down
        if (moveAmount.y < 0)
            DescendSlope(ref moveAmount);

        // Update face direction
        if (moveAmount.x != 0)
            collisions.faceDirection = (int)Mathf.Sign(moveAmount.x);

        // Handle collisions and update the movement amount if needed
        HorizontalCollisions(ref moveAmount);
        if (moveAmount.y != 0)
            VerticalCollisions(ref moveAmount);

        // Apply the movement
        transform.Translate(moveAmount);

        // Ensure jumping is enabled when on a platform
        if (onPlatform)
            collisions.below = true;

        // Update animator variables
        Animator animator = GetComponent<Animator>();
        animator.SetFloat("HVelocity", moveAmount.x / Time.fixedDeltaTime);
        animator.SetFloat("VVelocity", moveAmount.y / Time.fixedDeltaTime);
        animator.SetBool("Grounded", collisions.below);
        animator.SetInteger("FaceDirection", collisions.faceDirection);
        // Flip the image if facing left
        if (collisions.faceDirection < 0)
            transform.localScale = new Vector3(-1.0f, transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(1.0f, transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Handle horizontal collisions and update the movement vector if necessary
    /// </summary>
    /// <param name="moveAmount">A reference to the intended player movement vector</param>
    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        // Get movement direction and ray length based on the movement vector
        float directionX = collisions.faceDirection;
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        // If barely moving, set ray length to detect a wall in contact with the player (for wall jumping/sliding)
        if (Mathf.Abs(moveAmount.x) < skinWidth)
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
                        moveAmount = collisions.moveAmountOld;
                    }

                    // Snap to slope if climbing a new slope
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }

                    // Perform the movement amount adjustments for the slope climb
                    ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }

                // Handle ordinary horizontal collisions (non-slope)
                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    // Adjust X movement amount to be exactly the distance to the hit object
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    // Adjust ray length so it cannot hit another object further away
                    rayLength = hit.distance;

                    // Handle case where player collides with a wall on a slope
                    if (collisions.climbingSlope)
                        moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);

                    // Set collision info
                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                    if (hit.collider.CompareTag("WallSlide"))
                        collisions.canWallSlide = true;
                }
            }
        }
    }
    
    /// <summary>
     /// Handle vertical collisions and update the movement vector if necessary
     /// </summary>
     /// <param name="moveAmount">A reference to the intended player movement vector</param>
    void VerticalCollisions(ref Vector2 moveAmount)
    {
        // Get movement direction and ray length based on the movement vector
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            // Calculate the raycast origin based on the movement direction, ray spacing, and predicted X movement
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);

            // Cast a collision ray using the collision mask
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            // (Debug) Visually draw the vertical raycasting rays
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                // Allow player to pass through one-way platforms (by skipping collision and movement calculations)
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

                // Adjust Y movement amount to be exactly the distance to the hit object
                moveAmount.y = (hit.distance - skinWidth) * directionY;
                // Adjust ray length so it cannot hit another object further away
                rayLength = hit.distance;

                // Handle case where player collides with a ceiling on a slope
                if (collisions.climbingSlope)
                    moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);

                // Set collision info
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }

        // Handle case where a slope intersects with another slope of a different angle
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // Check if about to climb a new slope
                if (slopeAngle != collisions.slopeAngle)
                {
                    // Snap to new slope, not old
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                    collisions.slopeNormal = hit.normal;
                }
            }
        }
    }

    /// <summary>
    /// Handle climbing a slope and update the movement vector accordingly
    /// </summary>
    /// <param name="moveAmount">A reference to the intended player movement vector</param>
    /// <param name="slopeAngle">The angle of the slope that was encountered</param>
    /// <param name="slopeNormal">The surface normal of the slope that was encountered</param>
    void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        // Check that a jump (or other vertical movement) doesn't override the vertical component of the slope climb
        if (moveAmount.y <= climbMoveAmountY)
        {
            // Adjust movement amount to climb the slope
            moveAmount.y = climbMoveAmountY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);

            // Update collision/slope info
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
            collisions.slopeNormal = slopeNormal;
        }
    }

    /// <summary>
    /// Handle descending a slope and update the movement vector accordingly
    /// </summary>
    /// <param name="moveAmount">A reference to the intended player movement vector</param>
    void DescendSlope(ref Vector2 moveAmount)
    {
        // Check for maximum slope to slide down
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);

        // Only slide down if one side is in the air while the other is on the slope (exclusive-or)
        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
            SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
        }

        if (!collisions.slidingDownSlope)
        {
            // Cast a ray downward to detect a slope
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if (hit)
            {
                // Check that slope angle is descendable
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                    // Check that slope is in the same direction as X movement
                    if (Mathf.Sign(hit.normal.x) == directionX)
                        // Check that distance to slope is close enough to descend
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            // Adjust movement amount to descend the slope
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendMoveAmountY;

                            // Update collision/slope info
                            collisions.slopeAngle = slopeAngle;
                            collisions.descendingSlope = true;
                            collisions.below = true;
                            collisions.slopeNormal = hit.normal;
                        }
            }
        }
    }

    /// <summary>
    /// Handle sliding down a slope steeper than the max slope angle
    /// </summary>
    /// <param name="hit">The raycast hit against the slope</param>
    /// <param name="moveAmount">A reference to the intended player movement vector</param>
    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                // Calculate the horizontal movement based on the slope angle and the surface normal (for direction)
                moveAmount.x = Mathf.Abs(moveAmount.y - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * hit.normal.x;

                // Update collision/slope info
                collisions.slopeAngle = slopeAngle;
                collisions.slidingDownSlope = true;
                collisions.slopeNormal = hit.normal;
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

        public bool climbingSlope, descendingSlope, slidingDownSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;

        public Vector2 moveAmountOld;
        public int faceDirection;
        public bool fallingThroughPlatform;
        public bool canWallSlide;

        public void Reset() {
            above = below = left = right = climbingSlope = descendingSlope = slidingDownSlope = canWallSlide = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
            slopeNormal = Vector2.zero;
        }
    }
}
