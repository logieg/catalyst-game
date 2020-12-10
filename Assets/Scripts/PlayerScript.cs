using UnityEngine;

/// <summary>
/// Main player script to handle movement and interactions
/// </summary>
public class PlayerScript : MonoBehaviour
{
    /// <summary>Horizontal player movement speed</summary>
    public float hSpeed = 6.0f;
    /// <summary>The acceleration due to gravity</summary>
    public float gravity = 0.3f;
    /// <summary>The movement acceleration when jumping</summary>
    public float jumpAcceleration = 10.0f;
    /// <summary>The calculated change in position that will be applied if nothing prevents the player's movement</summary>
    private Vector2 pendingMove;
    /// <summary>Whether the player is currently on the ground</summary>
    private bool grounded = false;

    // Prefabs and effects used by the player
    public GameObject bloodParticlePrefab;
    public AudioClip jumpSound;
    public AudioClip deathSound;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Quit the game with Escape key
        if (Input.GetKeyUp(KeyCode.Escape))     // TODO - replace this with pause menu functionality
            QuitGame();

        // Check for colliders above and below the player (and update grounded status)
        Collider2D cBelow = Physics2D.OverlapArea(new Vector2(transform.position.x - 0.12f, transform.position.y - 0.3f),
            new Vector2(transform.position.x + 0.12f, transform.position.y - 0.4f));
        Collider2D cAbove = Physics2D.OverlapArea(new Vector2(transform.position.x - 0.2f, transform.position.y + 0.3f),
            new Vector2(transform.position.x + 0.2f, transform.position.y + 0.4f));
        grounded = (cBelow != null);

    }

    // Called at a fixed rate along with physics updates
    private void FixedUpdate()
    {
        // Gravity
        if (grounded)
            pendingMove.y = 0;
        else
            pendingMove.y -= gravity * Time.fixedDeltaTime;

        // Jump movement
        if (Input.GetAxis("Vertical") > 0.5 && grounded)
        {
            // Apply jump movement
            pendingMove.y = Input.GetAxis("Vertical") * jumpAcceleration * Time.fixedDeltaTime;

            // Play jumping sound effect
            if (jumpSound != null)
                SoundEffectScript.PlaySoundEffect(transform.position, jumpSound);
        }

        // Horizontal movement
        pendingMove.x = Input.GetAxis("Horizontal") * hSpeed * Time.fixedDeltaTime;

        // Set the animator parameters
        GetComponent<Animator>().SetBool("Grounded", grounded);
        // ? GetComponent<Animator>().SetFloat("HVelocity", moveVelocity.x / hSpeed);
        // ? GetComponent<Animator>().SetFloat("VVelocity", moveVelocity.y / hSpeed);

        // Apply the pending movement
        Vector2 newPosition = transform.position;
        newPosition.x += pendingMove.x;
        newPosition.y += pendingMove.y;
        transform.position = newPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO - handle movement blocking from collision
        pendingMove.y = 0;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {

    }

    /// <summary>
    /// Quit the game (or stop playing if in the editor)
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Kill the player with a death sound and a small splash of pixel blood
    /// </summary>
    public void Kill()
    {
        // TODO - visual death effects (ex. death sprite)

        // Death sound effect
        if (deathSound != null)
            SoundEffectScript.PlaySoundEffect(transform.position, deathSound);

        // Make a pixel blood splash effect for a dramatic death
        ParticleEffectScript.Splash(bloodParticlePrefab, transform, 25, 0.1f, 75.0f, 75.0f, 300.0f);

        // TODO - respawn player and reset level
    }
}
