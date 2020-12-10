using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages static game variables and globally-accessible operations
/// </summary>
public class GameManager : MonoBehaviour
{
    // The single global GameManager instance across levels (singleton)
    private static GameManager _instance = null;

    private void Awake()
    {
        // Destroy any duplicate GameManager a level might load with
        if (_instance != null && _instance != this)
            Destroy(gameObject);
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Get the currently active static GameManager instance
    /// </summary>
    /// <returns>The active static GameManager instance</returns>
    public static GameManager GetInstance()
    {
        return _instance;
    }

}
