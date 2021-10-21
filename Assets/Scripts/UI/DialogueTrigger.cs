using UnityEngine;

/// <summary>
/// Trigger script to show a section of dialogue on an event (proximity or interaction)
/// </summary>
[RequireComponent (typeof (BoxCollider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("The dialogue script that links to a dialogue file")]
    public DialogueScript dialogueScript;

    // The start and end line positions for the dialogue section
    public int startLine = 1;
    public int endLine = 2;

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

    void Update()
    {
        // Interact - Activate dialogue upon interact key press
        if (playerInRange && Input.GetButtonDown("Interact") && (!triggered || canBeTriggeredAgain))
        {
            if (!GameManager.GetInstance().dialogueBox.isOpen)
            {
                dialogueScript.ShowDialogue(startLine, endLine);
                triggered = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;

            if (!triggered || canBeTriggeredAgain)
            {
                // Proximity - Activate dialogue upon trigger entry
                if (type == TriggerType.Proximity)
                {
                    dialogueScript.ShowDialogue(startLine, endLine);
                    triggered = true;
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

    /// <summary>
    /// Changes the lines for this dialogue trigger and resets the triggered state
    /// </summary>
    public void ChangeLines(int start, int end, bool canBeTriggeredAgain)
    {
        startLine = start;
        endLine = end;
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
        }
    }
}
