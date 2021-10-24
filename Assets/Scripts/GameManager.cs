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
    public float volume = 1.0f;
    [HideInInspector]
    public bool paused = false;
    [HideInInspector]
    public bool playerAlive = true;
    [HideInInspector]
    public List<string> dialogueFlags = new List<string>();

    // Globally-accessible resources
    [HideInInspector]
    public DialogueBoxScript dialogueBox;
    [HideInInspector]
    public AudioSource flatSfxSource;
    [HideInInspector]
    public MusicScript musicManager;

    void Awake()
    {
        // Destroy any duplicate GameManager a level might load with
        if (_instance != null && _instance != this)
        {
            // Attempt to transfer music manager to main GameManager for a smooth music transition
            MusicScript musicManager = transform.GetComponentInChildren<MusicScript>();
            if (musicManager.songToPlay.name != _instance.musicManager.songToPlay.name)
            {
                musicManager.transform.parent = _instance.transform;
                musicManager.FadeIn(1.0f, musicManager.volume);
                _instance.musicManager.FadeOut(1.0f, true);
                _instance.musicManager = musicManager;
            }

            Destroy(gameObject);
        }
        else
        {
            // Start the music
            musicManager = transform.GetComponentInChildren<MusicScript>();
            musicManager.Play();

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        // Ensure level volume matches previous level
        AudioListener.volume = volume;

        // Set up the flat sound effect source
        GameObject flatSfx = new GameObject("FlatSfxSource");
        flatSfx.transform.parent = gameObject.transform;
        flatSfxSource = flatSfx.AddComponent<AudioSource>();
        flatSfxSource.spatialBlend = 0.0f;
    }

    void Update() { }

    /// <summary>
    /// Get the currently active GameManager instance
    /// </summary>
    public static GameManager GetInstance()
    {
        return _instance;
    }

    /// <summary>
    /// Play a one-shot flat sound effect
    /// </summary>
    public static void PlayFlatSfx(AudioClip clip, float volume = 1.0f)
    {
        _instance.flatSfxSource.PlayOneShot(clip, volume);
    }

}
