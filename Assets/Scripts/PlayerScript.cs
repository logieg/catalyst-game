using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main player script to handle movement and interactions
/// </summary>
public class PlayerScript : MonoBehaviour
{
    public float speed = 6.5f,
        acceleration = 1.5f,
        damping = 0.1f,
        jumpForce = 30.0f;
    private Vector2 moveVelocity; // Local velocity change variable for movement control
    private bool grounded = false;

    public GameObject bloodParticlePrefab;
    public GameObject celebrationParticlePrefab;
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
        // Jump movement
        if (Input.GetButtonDown("Jump") && grounded)
        {
            // TODO - apply jump movement
            if (jumpSound != null)
                SoundEffectScript.PlaySoundEffect(transform.position, jumpSound); // Jumping sound effect
        }

        // Horizontal movement
        moveVelocity.x += Input.GetAxis("Horizontal");
        moveVelocity.x = Mathf.Clamp(moveVelocity.x, -speed, speed);

        // TODO - better movement

        // Set the animator parameters
        GetComponent<Animator>().SetBool("Grounded", grounded);
        GetComponent<Animator>().SetFloat("HVelocity", moveVelocity.x / speed);
        GetComponent<Animator>().SetFloat("VVelocity", moveVelocity.y / speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO - handle movement blocking from collision
    }

    /// <summary>
    /// Kill the player with a death sound and a small splash of pixel blood
    /// </summary>
    public void Kill()
    {
        // TODO - visual death effects

        // Death sound effect
        if (deathSound != null)
            SoundEffectScript.PlaySoundEffect(transform.position, deathSound);

        // Make a pixel blood splash effect for a dramatic death
        ParticleEffectScript.Splash(bloodParticlePrefab, transform, 25, 0.1f, 75.0f, 75.0f, 300.0f);

        // TODO - respawn player and reset level
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
}
