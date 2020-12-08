using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows an object to remotely react to a trigger using a predefined behavior (ex. a buzzsaw flying out when the player is in a certain area)
public class TriggerReactor : MonoBehaviour
{
    public enum BehaviorType { MOVELEFT, MOVERIGHT, MOVEUP, MOVEDOWN, APPEAR, DISAPPEAR, RAGDOLL };
    public BehaviorType behavior;   // The behavior type of this reactor
    public float delay = 0.0f;      // The delay before reacting
    private bool triggered;

    private Vector3 initialPos;     // The initial position vector before applying movement
    private bool doMove;            // Whether to keep moving the object
    public float moveSpeed;         // The speed to use for movement-based behaviors

    // Start is called before the first frame update
    void Start()
    {
        // Setup for reaction
        triggered = false;
        initialPos = transform.position;
        doMove = false;

        // Setup for appear behavior
        if (behavior == BehaviorType.APPEAR)
            transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        // Do the behavior if reactor has been triggered
        if (triggered)
            DoBehavior();

        // Out of bounds checking (to prevent movement processing long after object is gone)
        if (transform.position.x > 4000 || transform.position.x < -4000 || transform.position.y > 4000 || transform.position.y < -4000)
            Destroy(gameObject);
    }

    // Called externally by a trigger script (ex. AreaTriggerer)
    public void Activate()
    {
        if (delay > 0.0f)
            StartCoroutine(DelayedActivate());
        else
        {
            triggered = true;
            doMove = true;
        }
    }

    // Allows for a delayed reactor activation
    private IEnumerator DelayedActivate()
    {
        yield return new WaitForSeconds(delay);
        triggered = true;
        doMove = true;
    }

    // Function to apply behavior for this trigger reactor object
    private void DoBehavior()
    {
        // Movement-based behaviors
        if (behavior == BehaviorType.MOVELEFT && doMove)
        {
            transform.Translate(new Vector3(-moveSpeed, 0, 0) * Time.deltaTime);
        }
        else if (behavior == BehaviorType.MOVERIGHT && doMove)
        {
            transform.Translate(new Vector3(moveSpeed, 0, 0) * Time.deltaTime);
        }
        else if (behavior == BehaviorType.MOVEUP && doMove)
        {
            transform.Translate(new Vector3(0, moveSpeed, 0) * Time.deltaTime);
        }
        else if (behavior == BehaviorType.MOVEDOWN && doMove)
        {
            transform.Translate(new Vector3(0, -moveSpeed, 0) * Time.deltaTime);
        }

        // Appear and disappear behaviors
        else if (behavior == BehaviorType.APPEAR)
        {
            float growthRate = 20.0f * Time.deltaTime;
            if (transform.localScale.x < 1.0f)
                transform.localScale = new Vector3(transform.localScale.x + growthRate, transform.localScale.y + growthRate, transform.localScale.z);
            else
            {
                transform.localScale = new Vector3(1.0f, 1.0f, transform.localScale.z);
                triggered = false;
            }
        }
        else if (behavior == BehaviorType.DISAPPEAR)
        {
            float shrinkRate = 25.0f * Time.deltaTime;
            if (transform.localScale.x > 0.0f)
                transform.localScale = new Vector3(transform.localScale.x - shrinkRate, transform.localScale.y - shrinkRate, transform.localScale.z);
            else
            {
                transform.localScale = new Vector3(0.0f, 0.0f, transform.localScale.z);
                triggered = false;
            }
        }

        // Ragdoll behavior (physics/gravity suddenly enabled)
        else if (behavior == BehaviorType.RAGDOLL && GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Static)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            triggered = false; // For efficiency's sake (triggered is unnecessary after this finishes)
        }
    }
}
