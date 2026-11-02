using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PausePanel : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject settingsPanel; 
    private bool isPaused = false;

    void Start()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    void Update()
    {
        // Unity checks for this key press every frame
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                CloseSettingsPanel();
                ClosePausePanel();
            }
            else
            {
                OpenPausePanel();
            }
        }
    }

    public void OpenPausePanel()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Freeze game
        isPaused = true;
    }

    public void ClosePausePanel()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
        isPaused = false;
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
        pausePanel.SetActive(false);
        Time.timeScale = 0f; // Freeze game
    }

    public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Freeze game
    }

    // You can link this to a "Main Menu" button on your Pause Panel
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Crucial: Unfreeze time before changing scenes
        SceneManager.LoadScene("MainMenu"); // Replace with your actual Main Menu scene name if different
    }
}