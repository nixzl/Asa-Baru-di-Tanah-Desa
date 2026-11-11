using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance;

    [Header("Reputation Stats")]
    public int currentLevel = 1;
    public float currentReputation = 0f;
    public float reputationToNextLevel = 100f; // Amount needed for Level 2

    [Header("UI Elements")]
    public Image progressBarFill; // Drag your UI Bar Image here
    public TextMeshProUGUI reputationText; // Drag your Level Text here

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    // Call this from a Quest/Mission script when a task is completed:
    // ReputationManager.Instance.AddReputation(50f);
    public void AddReputation(float amount)
    {
        currentReputation += amount;

        // Level Up logic: While we have enough rep, increase level and reset bar
        while (currentReputation >= reputationToNextLevel)
        {
            currentReputation -= reputationToNextLevel;
            currentLevel++;
            
            // Increase the requirement for the next level by 50%
            reputationToNextLevel *= 1.5f; 
            
            Debug.Log("Level Up! You are now Level " + currentLevel);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        // Updates the visual fill of the progress bar (from 0.0 to 1.0)
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = currentReputation / reputationToNextLevel;
        }

        // Updates the text inside to show "Lv. 1 (50/100)"
        if (reputationText != null)
        {
            reputationText.text = $"Lv. {currentLevel} ({Mathf.FloorToInt(currentReputation)} / {Mathf.FloorToInt(reputationToNextLevel)})";
        }
    }
}