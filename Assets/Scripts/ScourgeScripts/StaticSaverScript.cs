using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows certain information to be preserved between level loads, such as the spawn position of the player (for multiple spawnpoints)
public class StaticSaverScript : MonoBehaviour
{
    public GameObject playerObject;                         // The player object for which the information is saved
    public static Vector3 playerRespawnPos = Vector3.zero;  // The position to place the player on respawning (static to preserve between level loads)
    public static int maxHealth = 0;
    public static int health = 0;
    public static int swordPower = 1;
    public static float swordKnockback = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        // Move the player to the proper spawn point (if it has a non-default spawnpoint set)
        if (playerRespawnPos != Vector3.zero)
            playerObject.transform.position = playerRespawnPos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Change the respawn point from an external script
    public void ChangeSpawn(Vector3 newSpawn)
    {
        playerRespawnPos = newSpawn;
    }
}
