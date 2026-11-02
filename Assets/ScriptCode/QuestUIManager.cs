using UnityEngine;

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance; 
    public GameObject questPanel; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (questPanel != null) questPanel.SetActive(false);
    }

    public void ShowPanel()
    {
        questPanel.SetActive(true);
        // Freeze the game
        Time.timeScale = 0f; 
    }

    public void HidePanel()
    {
        questPanel.SetActive(false);
        // Resume the game
        Time.timeScale = 1f; 
    }
}