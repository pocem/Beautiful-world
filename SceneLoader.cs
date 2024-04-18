using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DentedPixel;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public Button howToPlayButton;
    public Button quitButton;
    public Button playButton;
    public Button backButton;
    public GameObject barParent;
    public GameObject loadingBar;

    void Start()
    {
        if (barParent != null && loadingBar != null)
        {
            loadingBar.SetActive(false);
            barParent.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Bar GameObject is not assigned!");
        }
    }
    public void PlayButtonClicked()
    {
        if (barParent != null && loadingBar != null)
        {
            barParent.SetActive(true); 
            loadingBar.SetActive(true); 
        }
        else
        {
            Debug.LogWarning("Bar GameObject is not assigned!");
            return;
        }
        StartCoroutine(LoadNextSceneAsync("Intro cut scene"));
    }
    private IEnumerator LoadNextSceneAsync(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f); 
            LeanTween.scaleX(loadingBar, progress, 0.1f); 

            yield return null;
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void GoToSettings()
    {
        SceneManager.LoadScene("How to play");
    }
    public void GoBack()
    {
        SceneManager.LoadScene("MENU");
    }
    public void RestartGame()
    {
        SceneManager.LoadScene("Game Scene");
    }
}