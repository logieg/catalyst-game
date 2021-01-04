using UnityEngine;

/// <summary>
/// Main player script to handle movement and interactions
/// </summary>
// Includes sections based on the 2D platformer player script by Sebastian Lague
[RequireComponent (typeof (PlayerController))]
public class PlayerScript : MonoBehaviour
{
    /// <summary>
    /// The PlayerController to use for controlling the player
    /// </summary>
    PlayerController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
