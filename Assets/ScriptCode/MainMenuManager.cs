using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject creditPanel;

    private void Start()
    {
        creditPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }


    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void OpenCredits()
    {
        creditPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }
    public void CloseCredits()
    {
        creditPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        creditPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }


    public void CloseSettings()
    {
        creditPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }


    public void QuitGame()
    {
        Debug.Log("Keluar");
        Application.Quit(); 
    }
}