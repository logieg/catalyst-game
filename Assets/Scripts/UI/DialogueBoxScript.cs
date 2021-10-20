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

    /// <summary>
    /// Whether the dialogue box is currently open or not
    /// </summary>
    [HideInInspector]
    public bool isOpen { get; private set; }

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
    }
}
