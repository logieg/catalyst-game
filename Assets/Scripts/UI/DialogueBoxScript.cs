using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles management of a dialogue box UI component and links it to the global game manager
/// <br/><br/>
/// Requires a CanvasGroup component on the same object, and a Text component on the first child
/// </summary>
[RequireComponent (typeof (CanvasGroup))]
public class DialogueBoxScript : MonoBehaviour
{
    private CanvasGroup dialogueGroup; // For changing dialogue box visibility
    private Text dialogueText;

    [Tooltip("The sound to play when starting or progressing dialogue")]
    public AudioClip dialogueSelectSound;

    /// <summary>
    /// Whether the dialogue box is currently open or not
    /// </summary>
    [HideInInspector]
    public bool isOpen { get; private set; }

    /// <summary>
    /// Whether the dialogue box is able to be opened (to prevent immediate reopening upon close)
    /// </summary>
    [HideInInspector]
    public bool canOpen { get; private set; }

    void Start()
    {
        // Make dialogue box globally accessible
        GameManager.GetInstance().dialogueBox = this;

        dialogueGroup = GetComponent<CanvasGroup>();

        // Get the text component from the top child
        dialogueText = gameObject.transform.GetChild(0).GetComponent<Text>();

        // Make sure the dialogue box starts closed
        SetVisible(false);
    }

    void Update() { }

    void LateUpdate()
    {
        if (!isOpen)
            canOpen = true;
    }

    /// <summary>
    /// Set the dialogue box's inner text
    /// </summary>
    public void SetText(string text)
    {
        dialogueText.text = text;
    }

    /// <summary>
    /// Set the dialogue box's visibility/open state
    /// </summary>
    public void SetVisible(bool visible)
    {
        dialogueGroup.alpha = visible ? 1 : 0;
        isOpen = visible ? true : false;
        canOpen = false;
    }

    /// <summary>
    /// Play the pre-configured dialogue select sound (if set)
    /// </summary>
    public void PlaySelectSound()
    {
        if (dialogueSelectSound != null)
        {
            GameManager.PlayFlatSfx(dialogueSelectSound, 0.25f);
        }
    }
}
