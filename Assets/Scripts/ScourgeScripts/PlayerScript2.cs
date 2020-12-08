using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 3D movement script for a jump-less top-down RPG game
public class PlayerScript2 : MonoBehaviour
{
    private Rigidbody body;
    private static KillableScript healthManager;

    public int maxHealth = 3;                       // The player's maximum and starting health
    public float speed = 7.0f;                      // Movement speed (horizontal)
    private Vector3 moveVelocity,                   // Local velocity change vector for movement control
        moveDirection;                              // Direction vector based on the movement velocity
    private bool canMove = true;                    // Whether the player is allowed to move or not

    public GameObject swordHitbox;                  // For hitting enemies and destructible objects
    public float swordReenableDelay = 0.45f;        // The delay between sword swings
    public GameObject swordSwipePrefab;             // The prefab with the sword swipe animation plane
    private bool canUseSword = true;                // Whether the player's sword is enabled

    public GameObject messageText;                  // The UI element to use when displaying messages (ex. item pickup)
    public GameObject deathText;                    // The UI element to enable when dead
    public GameObject spriteAnimationPlane;         // The plane (attached as a child) that shows the player's sprite animation and has the PlayerSpriteAnimation script
    private PlayerSpriteAnimation spriteAnimation;  // The actual animation script attached to the spriteAnimationPlane

    // Start is called before the first frame update
    void Start()
    {
        // General player script setup
        body = gameObject.GetComponent<Rigidbody>();
        healthManager = GetComponent<KillableScript>();
        moveDirection = Vector3.forward;
        spriteAnimation = spriteAnimationPlane.GetComponent<PlayerSpriteAnimation>();
        StartCoroutine(DisableMessageText(5.0f)); // Hide the initial quest message after a short delay

        // Restore/update saved values from the StaticSaver
        SwordScript sword = swordHitbox.GetComponent<SwordScript>();
        if (SceneManager.GetActiveScene().name == "Outside")
        {
            StaticSaverScript.maxHealth = maxHealth;
            StaticSaverScript.swordPower = sword.power;
            StaticSaverScript.swordKnockback = sword.knockbackForce;
            StaticSaverScript.health = maxHealth;
            healthManager.health = maxHealth;
        }
        else
        {
            maxHealth = StaticSaverScript.maxHealth;
            sword.power = StaticSaverScript.swordPower;
            sword.knockbackForce = StaticSaverScript.swordKnockback;
            healthManager.health = StaticSaverScript.health;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Allow quitting the game with Escape key
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        // Horizontal movement input
        moveVelocity.x = Input.GetAxisRaw("Horizontal") * speed;
        moveVelocity.y = 0.0f;
        moveVelocity.z = Input.GetAxisRaw("Vertical") * speed;

        // Ensure the player's horizontal speed vector doesn't exceed the speed (prevents higher speed when moving diagonally)
        if (moveVelocity.magnitude > speed)
            moveVelocity = moveVelocity.normalized * speed;

        // Update the direction vector (only if the magnitude is significant enough for a direction change)
        if (moveVelocity.magnitude > 0.1)
            moveDirection = moveVelocity.normalized;

        // If the magnitude of the current rigidbody velocity is greater than the max speed, use that velocity instead
        if (body.velocity.magnitude > speed)
            moveVelocity = body.velocity;

        // Handle sword-swinging mechanics
        if (Input.GetButtonDown("Fire1") && canUseSword)
        {
            canUseSword = false;
            swordHitbox.GetComponent<BoxCollider>().enabled = true;
            // Spawn a prefab plane with the sword swipe animation
            GameObject swipe = Instantiate(swordSwipePrefab);
            swipe.transform.position = swordHitbox.transform.position;
            swipe.transform.rotation = swordHitbox.transform.rotation;
            swipe.transform.SetParent(transform); // Because the capsule-collider object doesn't rotate, the animated swipe won't rotate either
            StartCoroutine(ReenableSword());
        }

        // Handle death condition
        if (!healthManager.IsAlive())
        {
            // Allow resetting the level (default key is "R")
            if (Input.GetButtonDown("Reset"))
                SceneManager.LoadScene("Outside");
        }

        // Set the animation parameters
        spriteAnimation.xVelocity = moveVelocity.x;
        spriteAnimation.zVelocity = moveVelocity.z;
    }

    private void FixedUpdate()
    {
        // Apply movement in FixedUpdate to prevent "jittering" caused by physics collisions after translating
        if (canMove)
            body.velocity = new Vector3(moveVelocity.x, body.velocity.y, moveVelocity.z);

        // Move the sword hitbox based on the current movement direction
        swordHitbox.transform.localPosition = moveDirection;
        swordHitbox.transform.localRotation = Quaternion.LookRotation(moveDirection);
    }

    private void OnCollisionExit(Collision collision)
    {
        // Apply a downward force at the end of stairs so the player doesn't fly off the end of the collider
        if (collision.collider.gameObject.CompareTag("Stairs"))
            body.AddForce(Vector3.down * 20.0f, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Destroy particles the player touches so they don't affect the player's physics (a piece of bush can no longer fling the player in the air)
        if (collision.collider.gameObject.CompareTag("PolyParticle"))
            Destroy(collision.collider.gameObject);

        // Collect any item the player touches
        if (collision.collider.gameObject.CompareTag("Item"))
        {
            ItemScript item = collision.collider.gameObject.GetComponent<ItemScript>();
            item.Collect();
            
            // Handle item-specific effects
            switch (item.itemType)
            {
                case ItemScript.Type.HealthBoost:
                    healthManager.health = maxHealth;
                    StaticSaverScript.health = maxHealth;
                    ShowMessage("[ITEM ACQUIRED]\n\nHealth restored!", 2.0f);
                    break;
                case ItemScript.Type.HealthMaxBoost:
                    maxHealth++;
                    healthManager.health = maxHealth;
                    StaticSaverScript.maxHealth = maxHealth;
                    ShowMessage("[ITEM ACQUIRED]\n\nMaximum health increased!", 2.8f);
                    break;
                case ItemScript.Type.RockbreakerSword:
                    SwordScript sword = swordHitbox.GetComponent<SwordScript>();
                    sword.power++;
                    sword.knockbackForce += 100.0f;
                    StaticSaverScript.swordPower = sword.power;
                    StaticSaverScript.swordKnockback = sword.knockbackForce;
                    ShowMessage("[ITEM ACQUIRED]\n\nRockbreaker Sword!\n\nMore powerful, and can break weak rocks.", 4.5f);
                    break;
                case ItemScript.Type.Torch:
                    Transform torch = collision.collider.gameObject.transform.GetChild(0); // Just steal the torch's point light
                    torch.SetParent(transform);
                    torch.localPosition = new Vector3(0.0f, 0.25f, 0.0f);
                    ShowMessage("[ITEM ACQUIRED]\n\nTorch!\n\nSee in the dark.", 2.5f);
                    break;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Collision with an enemy means player has been hit
        EnemyScript enemy;
        if (collision.collider.gameObject.TryGetComponent(out enemy))
        {
            healthManager.Hit(enemy.attackPower);
            StaticSaverScript.health = healthManager.health;

            // Check for death
            if (!healthManager.IsAlive())
            {
                // Disable most player functionality
                swordHitbox.SetActive(false);
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<MeshRenderer>().enabled = false;
                spriteAnimationPlane.SetActive(false);
                canMove = false;
                canUseSword = false;

                // Show the death text
                deathText.SetActive(true);
            }
        }
    }

    // Re-enable the player's sword ability after a delay (and re-disable the collider after it has a chance to act)
    private IEnumerator ReenableSword()
    {
        yield return new WaitForSeconds(0.05f);
        swordHitbox.GetComponent<BoxCollider>().enabled = false; // Only wait long enough for a collision to be detected before disabling the collider again
        yield return new WaitForSeconds(swordReenableDelay - 0.05f);
        canUseSword = true;
    }

    // So external scripts can view the player's health from the PlayerScript
    public int GetHealth()
    {
        return healthManager.health;
    }

    // Show a message for the specified amount of time on the UI element specified by messageText
    public void ShowMessage(string str, float time)
    {
        messageText.GetComponent<MessageTextScript>().SetText(str);
        messageText.SetActive(true);
        StartCoroutine(DisableMessageText(time));
    }

    private IEnumerator DisableMessageText(float delay)
    {
        yield return new WaitForSeconds(delay);
        messageText.SetActive(false);
    }

    // It's the endgame! (nothing really happens except for the message though)
    public void Victory()
    {
        Debug.Log("Victory!");
        ShowMessage("[QUEST COMPLETE]\n\nScourge Defeated!\n\nClear out more ratbeasts, or press ESC to quit.", 7.5f);

        // Give the player a secret boost in case they want to still fight stuff
        SwordScript sword = swordHitbox.GetComponent<SwordScript>();
        healthManager.health = maxHealth;
        sword.power++;
        sword.knockbackForce += 150.0f;
        StaticSaverScript.health = maxHealth;
        StaticSaverScript.swordPower = sword.power;
        StaticSaverScript.swordKnockback = sword.knockbackForce;

        // They did it.  The player finally beat the Scourge.  There's no more to see here. (...for now.)

    }

}
