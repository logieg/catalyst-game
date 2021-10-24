using System.Collections;
using UnityEngine;

/// <summary>
/// A simple script to play a sound clip at a point in space
/// <br/><br/>
/// Intended usage: Create an empty GameObject, attach this script, set the sound clip, and Play()
/// <br/>
/// Or just call PlaySoundEffect() and it'll do the work for you ;)
/// </summary>
public class SoundEffectScript : MonoBehaviour
{
    public AudioClip clip;
    public AudioSource source;
    public float volume = 1.0f;
    private bool played = false;

    // Start is called before the first frame update
    void Start()
    {
        // Setup for playing the sound effect
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 0.8f;
    }

    // Update is called once per frame
    void Update()
    {
        // Self-destruct when done playing (because the gameObject should only last as long as the sound)
        if (played && !source.isPlaying)
            Destroy(gameObject);
    }

    // Play the sound
    public void Play()
    {
        StartCoroutine(PlayNextFrame());
    }

    // Play the sound after a momentary delay (to allow the AudioSource to be initialized properly)
    private IEnumerator PlayNextFrame()
    {
        yield return new WaitForSeconds(0.01f);
        source.volume = volume;
        if (clip != null)
            source.PlayOneShot(clip);
        played = true;
    }

    /// <summary>
    /// Static method to easily play a sound effect at the speficied location in world coordinates
    /// <br/>
    /// Note: Sound will only play when time isn't frozen
    /// </summary>
    /// <param name="position">The world location at which to play the sound effect</param>
    public static void PlaySoundEffect(AudioClip clip, float volume, Vector3 position)
    {
        GameObject soundObject = new GameObject("SoundEffect");
        soundObject.transform.position = position;
        SoundEffectScript effect = soundObject.AddComponent<SoundEffectScript>();
        effect.clip = clip;
        effect.volume = volume;
        effect.Play();
    }
}
