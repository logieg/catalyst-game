using UnityEngine;

public class EnemyScriptEidolon : MonoBehaviour
{
    public float speed = 4;
    public float minX, maxX; // Min and max positions
    private int direction = -1; // Move direction (-1 = left, 1 = right)

    private Animator animator;

    private bool died = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Check for direction change
        if (transform.position.x < minX || transform.position.x > maxX)
            TurnAround();

        // Move
        transform.position = new Vector2(transform.position.x + speed * direction * Time.fixedDeltaTime, transform.position.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GolemController player;
        if (collision.gameObject.TryGetComponent<GolemController>(out player))
        {
            if (player.isAttacking)
            {
                // Player killed it!
                died = true;
                animator.SetBool("Dead", true);
                GetComponent<BoxCollider2D>().enabled = false;
                speed = 0;
            }
            else if (!died)
                // Player got kill'd by it!
                player.Die();
        }
    }

    // Do a flip
    void TurnAround()
    {
        direction *= -1;
        animator.SetInteger("Direction", direction);
    }

    // Commit death
    void Die()
    {
        Destroy(gameObject);
    }
    private class GolemController
    {
        public bool isAttacking = false;
        public void Die() { }
    }
}
