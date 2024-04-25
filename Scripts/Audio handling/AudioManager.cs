using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // Singleton instance

    public Sound[] sounds;

    public string defaultMusicName; // Default music to play
    private string currentMusicName; // Track the currently playing music

    void Awake()
    {
        // Singleton pattern: Ensure only one instance of AudioManager exists
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate AudioManager
            return;
        }

        // Add AudioSources for each sound
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        // Delay starting the default music
        Invoke("PlayDefaultMusic", 4.3f);
    }

    // Play sound by name
    public void Play(string name)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        // Check if the sound being played is the running sound
        if (name == "running" || name == "sprinting" || name == "swim" || name == "theme")
        {
            // Enable looping for specific sounds
            s.source.loop = true;
        }
        else
        {
            // Disable looping for other sounds
            s.source.loop = false;
        }
        s.source.Play();
        currentMusicName = name; // Update the currently playing music
    }

    public void Stop(string name)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    // Method to play default music
    public void PlayDefaultMusic()
    {
        Play(defaultMusicName);
    }

    // Method to change music for a specific scene
    public void ChangeMusicForScene(string musicName)
    {
        // Check if the music has actually changed to avoid stopping and playing the same music
        if (currentMusicName != musicName)
        {
            // Stop the current music
            foreach (Sound s in sounds)
            {
                if (s.source.isPlaying)
                {
                    s.source.Stop();
                }
            }

            // Play the new music
            Play(musicName);
        }
    }
}