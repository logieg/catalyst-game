using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows a trigger to change the spawnpoint of the player to the center of the trigger
public class SpawnPointChange : MonoBehaviour
{
    public GameObject staticSaver;     // The object that has the StaticSaverScript and manages the respawn position

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        // Change the player spawn point to the transform position of this trigger
        staticSaver.GetComponent<StaticSaverScript>().ChangeSpawn(transform.position);
    }
}
