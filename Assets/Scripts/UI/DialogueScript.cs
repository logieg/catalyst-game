using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles dialogue functionality for a specific dialogue file
/// </summary>
public class DialogueScript : MonoBehaviour
{
    // The global dialogue box retrieved from the game manager
    private DialogueBoxScript dialogueBox;

    [Tooltip("The text file containing all the lines of dialogue for this instance of the script")]
    public TextAsset dialogueDictionary;

    // The start, end, and current points for the currently open dialogue text
    private int startPos, endPos, currentPos;
    // The set of all dialogue lines in the dictionary
    private string[] lines;
    // The set of lines currently being shown in the dialogue box
    // NOTE: Currently unused, but might be useful in the future
    private string[] currentLines;

    private bool dialogueReady;

    private UnityEngine.Events.UnityAction callback = null;

    // Start is called before the first frame update
    void Start()
    {
        // Load the dialogue lines from the dictionary file
        lines = dialogueDictionary.text.Split('\n');

        StartCoroutine(LateStart());
    }

    // LateStart is called the frame after Start
    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(0.001f);

        // Retrieve the global dialogue box from the game manager
        dialogueBox = GameManager.GetInstance().dialogueBox;
    }

    // Update is called once per frame
    void Update()
    {
        // Check for continue key and handle continuing or closing the dialogue
        if (dialogueReady && (Input.GetButtonDown("Interact") || Input.GetButtonDown("Jump")) && dialogueBox.isOpen && !GameManager.GetInstance().paused)
        {
            currentPos++;
            if (currentPos > endPos)
            {
                // Dialogue done; close the box and unfreeze time
                dialogueBox.SetVisible(false);
                dialogueReady = false;
                Time.timeScale = 1.0f;

                // Invoke a pending callback if one has been set
                if (callback != null)
                {
                    callback.Invoke();
                    callback = null;
                }
            }
            else
            {
                // Show the next line of dialogue
                dialogueBox.SetText(lines[currentPos]);
            }
        }

        // Skip the first frame of dialogue being open to avoid input issues
        if (dialogueBox != null && dialogueBox.isOpen)
            dialogueReady = true;

        // TODO - Timer-based single-character output (typing effect)
    }

    /// <summary>
    /// Open the dialogue box and show the specified range of messages from the dictionary
    /// </summary>
    /// <param name="start">The line number in the dictionary to start with</param>
    /// <param name="end">The line number in the dictionary to end with</param>
    /// <param name="doneAction">Optional callback action for when the dialogue is done</param>
    public void ShowDialogue(int start, int end, UnityEngine.Events.UnityAction doneAction = null)
    {
        // Update positions (indexes)
        startPos = start - 1;
        endPos = end - 1;
        currentPos = startPos;

        // Get the set of current lines
        currentLines = new string[end - start + 1];
        for (int i = 0; i < currentLines.Length; i++)
            currentLines[i] = lines[startPos + i];

        // Open the dialogue box
        dialogueBox.SetText(lines[currentPos]);
        dialogueBox.SetVisible(true);

        // Freeze time while dialogue box is open
        Time.timeScale = 0.0f;

        callback = doneAction;
    }
}
