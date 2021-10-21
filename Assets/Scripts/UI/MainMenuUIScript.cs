using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the main menu UI
/// </summary>
public class MainMenuUIScript : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;
    public Slider volumeSlider;

    // Start is called before the first frame update
    void Start()
    {
        // Set up buttons
        startButton.onClick.AddListener(StartClicked);
        quitButton.onClick.AddListener(QuitClicked);
        volumeSlider.onValueChanged.AddListener(ChangeVolume);

        startButton.Select();
    }

    void Update() { }

    // Launch the player into the game
    void StartClicked()
    {
        SceneManager.LoadScene("Town");

        // TODO - load correct level/position from save
    }

    // Quit the game when Quit is clicked
    void QuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Change the global game volume
    void ChangeVolume(float vol)
    {
        AudioListener.volume = vol;
        GameManager.GetInstance().volume = vol;
    }
}
