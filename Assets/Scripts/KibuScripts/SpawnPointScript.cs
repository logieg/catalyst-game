using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows the position of the player on level reload to be changed (for multiple spawnpoints)
public class SpawnPointScript : MonoBehaviour
{
    public static Vector2 playerRespawnPos = Vector2.zero;  // The position to place the player on respawning (static to preserve between level reloads)
    public GameObject playerObject;                         // The player object to move

    // Start is called before the first frame update
    void Start()
    {
        // Move the player to the proper spawn point (if it has a non-default spawnpoint set)
        if (playerRespawnPos != Vector2.zero)
            playerObject.transform.position = playerRespawnPos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Change the respawn point from an external script
    public void ChangeSpawn(Vector2 newSpawn)
    {
        playerRespawnPos = newSpawn;
    }
}
