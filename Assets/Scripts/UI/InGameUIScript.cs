using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages in-game UI, including HUD elements and pause menu
/// </summary>
public class InGameUIScript : MonoBehaviour
{
    public CanvasGroup pauseGroup;
    [HideInInspector]
    public static bool isPaused;

    public Button continueButton;
    public Button quitButton;
    public Slider volumeSlider;

    void Start()
    {
        // Start unpaused
        pauseGroup.interactable = false;
        pauseGroup.alpha = 0;
        isPaused = false;

        // Set up buttons
        continueButton.onClick.AddListener(ContinueClicked);
        quitButton.onClick.AddListener(QuitClicked);
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            TogglePause();
        }
    }

    // Toggle the pause state
    void TogglePause()
    {
        if (!isPaused)
        {
            pauseGroup.interactable = true;
            pauseGroup.alpha = 1;
            Time.timeScale = 0.0f;
            isPaused = true;
            continueButton.Select();
            print("Game paused!");
        }
        else
        {
            pauseGroup.interactable = false;
            pauseGroup.alpha = 0;
            if (!GameManager.GetInstance().dialogueBox.isOpen)
                Time.timeScale = 1.0f;
            isPaused = false;
            EventSystem.current.SetSelectedGameObject(null); // To fix buttons staying selected after unpausing
        }

        // TODO - slide in/out animation

        // Update global pause state
        GameManager.GetInstance().paused = isPaused;
    }

    // Just unpause the game to continue
    void ContinueClicked()
    {
        TogglePause();
    }

    // Escape to the main menu when Quit is clicked
    void QuitClicked()
    {
        TogglePause();
        SceneManager.LoadScene("MainMenu");
    }

    // Change the global game volume
    void ChangeVolume(float vol)
    {
        AudioListener.volume = vol;
        GameManager.GetInstance().volume = vol;
    }
}
