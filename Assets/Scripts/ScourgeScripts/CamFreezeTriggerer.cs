using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Uses an area trigger to detatch the specified camera from the player (effectively freezing the view)
public class CamFreezeTriggerer : MonoBehaviour
{
    public GameObject cameraToFreeze;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    private void OnTriggerEnter(Collider collision)
    {
        // Only react to the player, and only if they have the freezable camera as a child
        if (collision.gameObject.CompareTag("Player") && cameraToFreeze.transform.IsChildOf(collision.gameObject.transform))
        {
            cameraToFreeze.transform.SetParent(null);
        }
    }
}
