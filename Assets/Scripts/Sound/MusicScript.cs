using UnityEngine;

/// <summary>
/// Music manager script to handle playing background music, including fade in/out
/// </summary>
public class MusicScript : MonoBehaviour
{
    private AudioSource source;
    public AudioClip songToPlay;
    public float volume = 0.7f;
    public bool loop = true;

    private int fadeDir = 0; // 1 in, -1 out
    private float fadeTime;
    private float fadeEnd;
    private float fadeAmount;
    private bool pendingDestroy;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.volume = volume;
        source.loop = loop;
        source.spatialBlend = 0.0f;
        source.clip = songToPlay;
    }

    void FixedUpdate()
    {
        // Fade in
        if (fadeDir > 0)
        {
            if (fadeTime < fadeEnd)
            {
                volume += fadeAmount;
                fadeTime += Time.fixedDeltaTime;
            }
        }
        // Fade out
        else if (fadeDir < 0)
        {
            if (fadeTime < fadeEnd)
            {
                volume -= fadeAmount;
                fadeTime += Time.fixedDeltaTime;
            }
            else if (pendingDestroy)
                Destroy(gameObject);
        }

        source.volume = volume;
    }

    /// <summary>
    /// Start the music
    /// </summary>
    public void Play()
    {
        source.Play();
    }

    /// <summary>
    /// Fade the music in over a number of seconds
    /// </summary>
    public void FadeIn(float seconds, float targetVolume)
    {
        volume = 0.0f;
        fadeAmount = targetVolume / seconds * Time.fixedDeltaTime;
        fadeEnd = seconds;
        fadeTime = 0.0f;
        fadeDir = 1;
        Play();
    }

    /// <summary>
    /// Fade the music out over a number of seconds and optionally self-destruct when done
    /// </summary>
    public void FadeOut(float seconds, bool destroy)
    {
        fadeAmount = volume / seconds * Time.fixedDeltaTime;
        fadeEnd = seconds;
        fadeTime = 0.0f;
        fadeDir = -1;
        Play();
        if (destroy)
            pendingDestroy = true;
    }
}
