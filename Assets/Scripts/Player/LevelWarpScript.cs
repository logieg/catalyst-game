using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Warps the player to a different level and saves the destination position in the game manager
/// </summary>
public class LevelWarpScript : MonoBehaviour
{
    public string destinationName;
    public bool changePositionOnLoad;
    public Vector2 destinationPosition;
    public bool faceLeft;

    void Update() { }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Save the destination position if needed
            if (changePositionOnLoad)
            {
                GameManager gm = GameManager.GetInstance();
                gm.warpPosition = destinationPosition;
                gm.warpFaceDirection = faceLeft ? -1 : 1;
                gm.warpPositionPending = true;
            }
            
            // Warp to the new level
            SceneManager.LoadScene(destinationName);
        }
    }
}
