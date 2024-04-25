using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Fade : MonoBehaviour
{
    // Duration of the fade-out animation in seconds
    public float fadeDuration = 2f;

    // Reference to the CanvasGroup component
    private CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        // Get the CanvasGroup component
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Method to trigger the fade-out animation
    public void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    // Coroutine for the fade-out animation
    IEnumerator FadeOut()
    {
        // Set the alpha of the CanvasGroup to 1
        canvasGroup.alpha = 1f;

        // Calculate the step based on the duration
        float step = 1 / fadeDuration;

        // Decrease alpha gradually until it reaches 0
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= step * Time.deltaTime;
            yield return null;
        }

        // Ensure alpha is exactly 0 at the end
        canvasGroup.alpha = 0f;

        // Optionally, you can deactivate the GameObject or perform any other action
        gameObject.SetActive(false);
    }
}