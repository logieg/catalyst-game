using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger script to show a section of dialogue on an event (proximity or interaction)
/// </summary>
[RequireComponent (typeof (BoxCollider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("The dialogue script that links to a dialogue file")]
    public DialogueScript dialogueScript;

    [Tooltip("The tag for the dialogue section to show")]
    public string sectionTag;

    [Tooltip("(Optional) The dialogue flag that must be set to enable this interaction")]
    public string conditionalFlag = "";

    public enum TriggerType { Proximity, Interact };
    [Tooltip("Proximity: Upon entering trigger collider / Interact: Player must press the interact key")]
    public TriggerType type = TriggerType.Interact;

    [Tooltip("Whether the dialogue can be triggered again if the player re-enters the trigger")]
    public bool canBeTriggeredAgain = false;
    private bool triggered = false;
    private bool playerInRange = false;

    [Tooltip("The marker to show when the player can interact with this trigger")]
    public GameObject interactMarkerPrefab;
    private GameObject interactMarker;

    [Tooltip("(Optional) A list of state transitions for what dialogue sections to show when certain flags are set")]
    public List<Transition> conditionalTransitions;

    void Update()
    {
        // Interact - Activate dialogue upon interact key press
        if (playerInRange && type == TriggerType.Interact && Input.GetButtonDown("Interact"))
        {
            if (GameManager.GetInstance().dialogueBox.canOpen && MeetsTriggerConditions())
            {
                ShowDialogue();
            }
        }

        // Check for conditional state updates
        List<string> flags = GameManager.GetInstance().dialogueFlags;
        foreach (Transition transition in conditionalTransitions)
        {
            if (flags.Contains(transition.conditionFlag))
                ChangeLines(transition.newSectionTag, canBeTriggeredAgain);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;

            if (MeetsTriggerConditions())
            {
                // Proximity - Activate dialogue upon trigger entry
                if (type == TriggerType.Proximity)
                {
                    ShowDialogue();
                }
                else if (type == TriggerType.Interact)
                {
                    ShowInteractMarker();
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = false;

            HideInteractMarker();
        }
    }

    // Checks if the trigger is triggerable and meets all conditions
    private bool MeetsTriggerConditions()
    {
        return (!triggered || canBeTriggeredAgain)
            && (conditionalFlag == "" || GameManager.GetInstance().dialogueFlags.Contains(conditionalFlag));
    }

    // Show the dialogue, update trigger state, and change to next section if known
    private void ShowDialogue()
    {
        string nextTag = dialogueScript.ShowDialogue(sectionTag);
        triggered = true;
        if (!canBeTriggeredAgain)
            HideInteractMarker();
        if (nextTag != "")
            ChangeLines(nextTag, canBeTriggeredAgain);
    }

    /// <summary>
    /// Changes the lines for this dialogue trigger and resets the triggered state
    /// </summary>
    public void ChangeLines(string sectionTag, bool canBeTriggeredAgain)
    {
        this.sectionTag = sectionTag;
        triggered = false;
        this.canBeTriggeredAgain = canBeTriggeredAgain;
    }

    /// <summary>
    /// Show an interaction marker above the trigger area
    /// </summary>
    void ShowInteractMarker()
    {
        if (interactMarkerPrefab != null)
        {
            interactMarker = Instantiate(interactMarkerPrefab);
            interactMarker.transform.parent = gameObject.transform;
            interactMarker.transform.localPosition = new Vector2(0, GetComponent<BoxCollider2D>().size.y / 2.0f + 0.15f);
        }
    }

    /// <summary>
    /// Hide the interaction marker above the trigger area
    /// </summary>
    void HideInteractMarker()
    {
        if (interactMarker != null)
        {
            Destroy(interactMarker);
            interactMarker = null;
        }
    }

    // State transition to change dialogue when a condition flag is met
    [System.Serializable]
    public class Transition
    {
        public string conditionFlag;
        public string newSectionTag;
    }
}
