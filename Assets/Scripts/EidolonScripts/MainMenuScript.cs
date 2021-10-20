using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;

    // Start is called before the first frame update
    void Start()
    {
        // Set up buttons
        startButton.onClick.AddListener(StartClicked);
        quitButton.onClick.AddListener(QuitClicked);
    }

    // Start the game
    void StartClicked()
    {
        SceneManager.LoadScene("PuzzleMap");
    }

    // Quit the game when Quit is clicked
    void QuitClicked()
    {
        // Player doesn't want to play anymore :c
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
