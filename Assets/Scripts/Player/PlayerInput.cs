﻿using UnityEngine;

/// <summary>
/// 2D player input class to handle game input from the user
/// </summary>
// Based on the 2D player input script by Sebastian Lague
[RequireComponent (typeof (PlayerScript))]
public class PlayerInput : MonoBehaviour
{
    public AudioClip jumpSound;
    private bool jumpSoundPlayed;

    PlayerScript player;

    bool jumping;
    bool jumpBlocked;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        // Block jumping if the game is paused or dialogue is open
        if (GameManager.GetInstance().paused || GameManager.GetInstance().dialogueBox.isOpen)
            jumpBlocked = true;

        // Get the directional input from the player (without smoothing)
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);

        // Handle jump input down
        if (Input.GetButton("Jump"))
        {
            if (!jumping && !jumpBlocked)
                jumping = player.OnJumpInputDown();

            // SFX!
            if (jumping && !jumpSoundPlayed)
            {
                SoundEffectScript.PlaySoundEffect(jumpSound, 0.55f, transform.position);
                jumpSoundPlayed = true;
            }
        }

        // Handle jump input released (only during a jump)
        else if (jumping)
        {
            jumping = false;
            jumpSoundPlayed = false;
            player.OnJumpInputUp();
        }

        // Unblock jumping
        if (Input.GetButtonUp("Jump") || Input.GetButtonUp("Pause") || Input.GetButtonUp("Interact"))
            jumpBlocked = false;

        // Update animator variables
        Animator animator = GetComponent<Animator>();
        animator.SetFloat("HInput", directionalInput.x);
        animator.SetBool("CanMove", !GameManager.GetInstance().paused && !GameManager.GetInstance().dialogueBox.isOpen);
    }
}
