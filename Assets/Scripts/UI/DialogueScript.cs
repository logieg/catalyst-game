using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles dialogue functionality for a specific dialogue file
/// <br/><br/>
/// In a dialogue file:<br/>
/// '#' denotes a section tag<br/>
/// '~' sets a dialogue completion flag<br/>
/// '~-' unsets a dialogue completion flag<br/>
/// '@' denotes the next section tag to change to when done
/// </summary>
public class DialogueScript : MonoBehaviour
{
    // The global dialogue box retrieved from the game manager
    private DialogueBoxScript dialogueBox;

    [Tooltip("The text file containing all the lines of dialogue for this instance of the script")]
    public TextAsset dialogueFile;

    // Section/position locators for the currently open dialogue text
    private string currentSectionTag = "";
    private int currentPos;

    // The set of all dialogue lines in the dictionary, organized into tagged sections
    private Dictionary<string, List<string>> sections = new Dictionary<string, List<string>>();

    // The set of lines currently being shown in the dialogue box
    private List<string> currentLines;

    private bool dialogueReady;

    private UnityEngine.Events.UnityAction callback = null;

    // Initialize
    void Start()
    {
        // Load all dialogue lines from the dictionary file
        string[] lines = dialogueFile.text.Split('\n');

        // Build tagged dialogue sections from the lines
        string sectionTag = "";
        foreach (string l in lines)
        {
            string line = l.TrimEnd(); // Clean trailing whitespace chars
            if (line.StartsWith("#"))
            {
                sectionTag = line.Substring(1);
                sections.Add(sectionTag, new List<string>());
            }
            else if (line != "")
            {
                // Swap manual newlines with real linebreaks while adding
                sections[sectionTag].Add(line.Replace("\\n", "\n"));
            }
        }

        StartCoroutine(LateStart());
    }

    // LateStart is called the frame after Start
    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(0.001f);

        // Retrieve the global dialogue box from the game manager
        dialogueBox = GameManager.GetInstance().dialogueBox;
    }

    void Update()
    {
        // Check for continue key and handle continuing or closing the dialogue
        if (dialogueReady && (Input.GetButtonDown("Interact") || Input.GetButtonDown("Jump")) && dialogueBox.isOpen && !GameManager.GetInstance().paused)
        {
            currentPos++;
            if (currentPos > currentLines.Count - 1)
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
                dialogueBox.SetText(currentLines[currentPos]);
            }
        }

        // Skip the first frame of dialogue being open to avoid input issues
        if (dialogueBox != null && dialogueBox.isOpen)
            dialogueReady = true;

        // TODO - Timer-based single-character output (typing effect)
    }

    /// <summary>
    /// Open the dialogue box and show the specified section of lines from the dictionary
    /// </summary>
    /// <param name="sectionTag">The tag for the section of lines in the dictionary to show</param>
    /// <param name="doneAction">Optional callback action for when the dialogue is done</param>
    /// <returns>The tag of the next section to change to, if known</returns>
    public string ShowDialogue(string sectionTag, UnityEngine.Events.UnityAction doneAction = null)
    {
        // Update locators
        currentSectionTag = sectionTag;
        currentPos = 0;

        // Get the set of current lines
        currentLines = new List<string>();

        // Search for dialogue commands
        string nextTag = "";
        foreach (string line in sections[currentSectionTag])
        {
            // Unset dialogue flag
            if (line.StartsWith("~-"))
            {
                GameManager.GetInstance().dialogueFlags.Remove(line.Substring(2));
            }
            // Set dialogue flag
            else if (line.StartsWith("~"))
            {
                GameManager.GetInstance().dialogueFlags.Add(line.Substring(1));
            }
            // Next section tag
            else if (line.StartsWith("@"))
            {
                nextTag = line.Substring(1);
            }
            // Not a command, add to displayable lines
            else
            {
                currentLines.Add(line);
            }
        }

        // Open the dialogue box
        dialogueBox.SetText(currentLines[currentPos]);
        dialogueBox.SetVisible(true);

        // Freeze time while dialogue box is open
        Time.timeScale = 0.0f;

        callback = doneAction;

        return nextTag;
    }
}
