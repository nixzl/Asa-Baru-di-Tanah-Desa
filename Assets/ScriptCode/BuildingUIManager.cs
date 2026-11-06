using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingUIManager : MonoBehaviour
{
    public static BuildingUIManager Instance;
    
    public GameObject uIPanel;
    public TextMeshProUGUI statusText;
    public TMP_InputField minuteInput; // For typing numbers
    public Button startButton;         // To disable if requirements aren't met
    
    private ResourceGenerator currentBuilding;
    private int pendingWorkers = 0;
    private int pendingMinutes = 0;

    void Awake()
    {
        uIPanel.SetActive(false); // Ensure the panel is hidden at the start
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OpenPanel(ResourceGenerator building)
    {
        currentBuilding = building;
        pendingWorkers = building.minWorkersRequired; // Default to the minimum needed
        
        // Cap it if we don't have enough idle workers
        pendingWorkers = Mathf.Min(pendingWorkers, ResourceManager.Instance.availableWorkers);
        
        pendingMinutes = 0;
        minuteInput.text = ""; // Clear the typing box
        
        UpdateUI();
        uIPanel.SetActive(true);
        Time.timeScale = 0f; // Freezes game physics and movement
    }

    public void AddWorker()
    {
        if (ResourceManager.Instance.availableWorkers > pendingWorkers) pendingWorkers++;
        UpdateUI();
    }

    public void RemoveWorker()
    {
        if (pendingWorkers > 0) pendingWorkers--;
        UpdateUI();
    }

    // Called automatically when the player types in the Input Field
    public void OnMinuteInputChanged(string input)
    {
        if (int.TryParse(input, out int result)) pendingMinutes = result;
        else pendingMinutes = 0;
        
        UpdateUI();
    }

    public void StartWork()
    {
        if (currentBuilding != null && startButton.interactable)
        {
            currentBuilding.AssignWork(pendingWorkers, pendingMinutes);
        }
        ClosePanel();
    }

    public void Cancel()
    {
        ClosePanel();
    }

    public void ClosePanel()
    {
        uIPanel.SetActive(false);
        Time.timeScale = 1f;
        currentBuilding = null;
    }

    private void UpdateUI()
    {
        statusText.text = $"Idle Workers: {ResourceManager.Instance.availableWorkers - pendingWorkers}\n" +
                          $"Requires at least {currentBuilding.minWorkersRequired} workers\n\n" +
                          $"Assigning: {pendingWorkers} Workers";

        // Enforce logic: Block the Start button if requirements aren't met
        bool hasEnoughWorkers = pendingWorkers >= currentBuilding.minWorkersRequired;
        bool hasValidTime = pendingMinutes > 0;
        
        startButton.interactable = hasEnoughWorkers && hasValidTime;
    }
}