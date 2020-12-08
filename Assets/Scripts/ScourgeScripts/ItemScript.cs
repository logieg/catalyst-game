using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script for item behavior and type checking
public class ItemScript : MonoBehaviour
{
    public enum Type { HealthMaxBoost, HealthBoost, RockbreakerSword, Torch };
    public Type itemType;
    private bool collected = false;
    private float velocity = 0.0f,
        velocityMax = 0.2f,
        slowRate = 0.08f;
    public AudioClip collectSound;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    private void FixedUpdate()
    {
        if (collected)
        {
            if (velocity > 0.001f)
            {
                transform.Translate(new Vector3(0.0f, 0.0f, -1 * velocity), Space.Self);
                velocity *= (1 - slowRate);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void Collect()
    {
        collected = true;
        velocity = velocityMax;
        GetComponent<CapsuleCollider>().enabled = false;
        if (collectSound != null)
            SoundEffectScript.PlaySoundEffect(transform.position, collectSound);
    }
}
