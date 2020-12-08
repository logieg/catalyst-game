using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Uses an area trigger to remotely call the TriggerReactor on each of the specified objects
public class AreaTriggerer : MonoBehaviour
{
    public bool isDeathTrigger = false;
    public GameObject[] objectsToTrigger;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only react to the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Remotely trigger each object in the provided list
            foreach (GameObject g in objectsToTrigger)
            {
                g.GetComponent<TriggerReactor>().Activate();
            }

            // If death trigger, murder the player
            if (isDeathTrigger)
                collision.gameObject.GetComponent<PlayerScript>().Kill();

            // Destroy non-death trigger because it has done its job (and otherwise the player would be "grounded" in midair within the trigger)
            else
                Destroy(gameObject);
        }
    }
}
