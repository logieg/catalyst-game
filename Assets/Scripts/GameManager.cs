using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages game state and global functionality
/// </summary>
public class GameManager : MonoBehaviour
{
    // The global GameManager instance across levels (singleton)
    private static GameManager _instance = null;
    
    // Game state variables
    [HideInInspector]
    public bool paused = false;
    [HideInInspector]
    public bool playerAlive = true;

    // Globally-accessible resources
    [HideInInspector]
    public DialogueBoxScript dialogueBox;

    private void Awake()
    {
        // Destroy any duplicate GameManager a level might load with
        if (_instance != null && _instance != this)
            Destroy(gameObject);
        else {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start() { }

    void Update() { }

    /// <summary>
    /// Get the currently active GameManager instance
    /// </summary>
    public static GameManager GetInstance()
    {
        return _instance;
    }

}
