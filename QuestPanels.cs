using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanels : MonoBehaviour
{
    public GameObject panel;
    public GameObject panel2;
    public float waitDuration = 4f;
    public float panelActiveDuration = 5f;
    public Button uiButton;
    private bool panel2Shown = false;
    void Start()
    {
        Invoke("ShowPanel", waitDuration);
        uiButton.onClick.AddListener(ShowPanel2);
    }
    

    // Method to show the panel
    void ShowPanel()
    {
        panel.SetActive(true);
        FindAnyObjectByType<AudioManager>().Play("health");
        Invoke("DeactivatePanel", panelActiveDuration);
    }
    void ShowPanel2()
    {
        if (!panel2Shown)
        {
            panel2.SetActive(true);
            FindAnyObjectByType<AudioManager>().Play("health");
            Invoke("DeactivatePanel2", panelActiveDuration);

            panel2Shown = true; 
        }
    }

    // Method to deactivate the panel
    void DeactivatePanel()
    {
        panel.SetActive(false);
    }
    void DeactivatePanel2()
    {
        panel2.SetActive(false);
    }
}
