using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static bool isStartMenu;
    private static float gameVolume = 1;
    public GameObject resetButton;
    public GameObject quitButton;
    public Slider slider;

    public GameObject canvas;
    private bool menuVisible = true;
    
    // Start is called before the first frame update
    void Start()
    {
        isStartMenu = SceneManager.GetActiveScene().buildIndex == 0;
        if (isStartMenu)
        {
            resetButton.SetActive(false);
        }
        else
        {
            resetButton.SetActive(true);
            isStartMenu = false;
            toggleMenu();
        }
        slider.value = gameVolume;
        AudioListener.volume = gameVolume;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isStartMenu)
        {
            // Show or hide the pause menu in-game
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                toggleMenu();
            }
        }
    }
    public void playGame()
    {
        if (isStartMenu)
        {
            isStartMenu = false;
            SceneManager.LoadScene(1);
            // Load first level
        }
        else
        {
            toggleMenu();
        }
        // Make sure to check for value passing between level
    }
    public void changeVolume()
    {
        gameVolume = slider.value;
        AudioListener.volume = gameVolume;
        // Add functionality for the volume level changing
    }
    public void resetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Make sure to check for value passing between level
    }
    public void quitGame()
    {
        // Simulates the closing of the application
        //UnityEditor.EditorApplication.isPlaying = false;
        // Will make the application close
        Application.Quit();
    }
    public void toggleMenu()
    {
        // Show or hide the menu based on its current state
        if (menuVisible)
        {
            canvas.SetActive(false);
            menuVisible = false;
            Time.timeScale = 1;
        }
        else
        {
            canvas.SetActive(true);
            menuVisible = true;
            Time.timeScale = 0;
        }
    }

    
}
