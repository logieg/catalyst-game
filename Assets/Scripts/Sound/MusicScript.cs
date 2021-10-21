using UnityEngine;

// Simple music script!
public class MusicScript : MonoBehaviour
{
    public AudioClip songToPlay;
    public float volume = 0.7f;
    public bool loop = true;

    void Start()
    {
        AudioSource music = gameObject.AddComponent<AudioSource>();
        music.volume = volume;
        music.loop = loop;
        music.spatialBlend = 0.0f;
        music.clip = songToPlay;
        music.Play();
    }

    // TODO - dontdestroyonload the music manager, transition between tracks
}
