using UnityEngine;

public class AudioDelay : MonoBehaviour
{
    public float delayInSeconds = 3f; // Adjust this value to set the delay

    private AudioSource audioSource;

    void Start()
    {
        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();

        // Invoke the PlayDelayedAudio method after the specified delay
        Invoke("PlayDelayedAudio", delayInSeconds);
    }

    void PlayDelayedAudio()
    {
        // Play the audio clip
        audioSource.Play();
    }
}