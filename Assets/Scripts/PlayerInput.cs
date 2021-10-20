using UnityEngine;

/// <summary>
/// 2D player input class to handle game input from the user
/// </summary>
// Based on the 2D player input script by Sebastian Lague
[RequireComponent (typeof (PlayerScript))]
public class PlayerInput : MonoBehaviour
{
    PlayerScript player;

    bool jumping;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get the directional input from the player (without smoothing)
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);

        // Handle jump input down
        if (Input.GetAxisRaw("Jump") > 0.3f)
        {
            if (!jumping)
                jumping = player.OnJumpInputDown();
        }

        // Handle jump input released (only during a jump)
        else if (jumping)
        {
            jumping = false;
            player.OnJumpInputUp();
        }
    }
}
