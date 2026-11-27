using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingUIManager : MonoBehaviour
{
    public static BuildingUIManager Instance;

    [Header("Main Worker Assignment Panel")]
    public GameObject uIPanel;
    public TextMeshProUGUI availabilityText;    // Above divider: Pekerja Tersedia & Membutuhkan
    public TextMeshProUGUI assignedWorkersText; // Below divider: Menambahkan / Bekerja
    public TMP_InputField minuteInput;
    public Button startButton;
    public TextMeshProUGUI startButtonText;     // Optional: switches between "MULAI" and "SIMPAN"
    public Button unassignAllButton;            // Optional: button to recall all workers mid-production

    [Header("Top-Right Active Work HUD (Below Reputation Bar)")]
    public GameObject activeWorkHudPanel;
    public Image activeResourceIcon;
    public TextMeshProUGUI activeMinutesText;
    public Sprite stoneIcon;
    public Sprite woodIcon;
    public Sprite wheatIcon;

    private ResourceGenerator currentBuilding;
    private int pendingWorkers = 0;
    private int pendingMinutes = 0;
    private readonly List<ResourceGenerator> activeBuildings = new List<ResourceGenerator>();

    void Awake()
    {
        if (uIPanel != null) uIPanel.SetActive(false);
        if (activeWorkHudPanel != null) activeWorkHudPanel.SetActive(false);

        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        UpdateActiveWorkHUD();
    }

    public void OpenPanel(ResourceGenerator building)
    {
        currentBuilding = building;

        if (building.isWorking)
        {
            // Mid-production mode: load current assigned workers so the player can unassign them
            pendingWorkers = building.assignedWorkers;
            pendingMinutes = Mathf.CeilToInt(building.remainingSeconds / 60f);

            if (minuteInput != null)
            {
                minuteInput.text = pendingMinutes.ToString();
                minuteInput.interactable = false; // Lock duration while already running
            }
            if (unassignAllButton != null) unassignAllButton.gameObject.SetActive(true);
        }
        else
        {
            // Idle mode: assign new workers and duration
            pendingWorkers = Mathf.Min(building.minWorkersRequired, ResourceManager.Instance.availableWorkers);
            pendingMinutes = 0;

            if (minuteInput != null)
            {
                minuteInput.text = "";
                minuteInput.interactable = true;
            }
            if (unassignAllButton != null) unassignAllButton.gameObject.SetActive(false);
        }

        UpdateUI();
        uIPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void AddWorker()
    {
        int currentlyAssignedToThis = (currentBuilding != null && currentBuilding.isWorking) ? currentBuilding.assignedWorkers : 0;
        int maxAllowed = ResourceManager.Instance.availableWorkers + currentlyAssignedToThis;

        if (pendingWorkers < maxAllowed)
        {
            pendingWorkers++;
            UpdateUI();
        }
    }

    public void RemoveWorker()
    {
        if (pendingWorkers > 0)
        {
            pendingWorkers--;
            UpdateUI();
        }
    }

    public void OnMinuteInputChanged(string input)
    {
        if (int.TryParse(input, out int result)) pendingMinutes = result;
        else pendingMinutes = 0;

        UpdateUI();
    }

    public void StartWork()
    {
        if (currentBuilding == null) return;

        if (currentBuilding.isWorking)
        {
            // Apply worker changes mid-production (unassigns excess or stops if below minimum)
            currentBuilding.UpdateWorkersMidProduction(pendingWorkers);
        }
        else if (startButton.interactable)
        {
            currentBuilding.AssignWork(pendingWorkers, pendingMinutes);
        }

        ClosePanel();
    }

    // Hook this to a "Hentikan / Unassign" button (or set workers to 0 and click Confirm)
    public void UnassignAllNow()
    {
        if (currentBuilding != null && currentBuilding.isWorking)
        {
            currentBuilding.UnassignAllWorkersMidProduction();
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
        if (currentBuilding == null) return;

        int currentlyAssignedToThis = currentBuilding.isWorking ? currentBuilding.assignedWorkers : 0;
        int remainingAvailable = (ResourceManager.Instance.availableWorkers + currentlyAssignedToThis) - pendingWorkers;

        // 1. Upper text (above the horizontal line)
        if (availabilityText != null)
        {
            availabilityText.text = $"Pekerja Tersedia: {remainingAvailable}\n" +
                                    $"Membutuhkan {currentBuilding.minWorkersRequired} pekerja";
        }

        // 2. Lower text (below the horizontal line)
        if (assignedWorkersText != null)
        {
            if (currentBuilding.isWorking)
                assignedWorkersText.text = $"Pekerja Aktif: {pendingWorkers} pekerja";
            else
                assignedWorkersText.text = $"Menambahkan: {pendingWorkers} pekerja";
        }

        if (currentBuilding.isWorking)
        {
            bool willStopAll = pendingWorkers < currentBuilding.minWorkersRequired;
            if (startButtonText != null)
                startButtonText.text = willStopAll ? "HENTIKAN" : "SIMPAN";

            startButton.interactable = true;
        }
        else
        {
            if (startButtonText != null)
                startButtonText.text = "MULAI";

            bool hasEnoughWorkers = pendingWorkers >= currentBuilding.minWorkersRequired;
            bool hasValidTime = pendingMinutes > 0;
            startButton.interactable = hasEnoughWorkers && hasValidTime;
        }
    }

    // --- ACTIVE WORK HUD TRACKER ---
    public void RegisterActiveBuilding(ResourceGenerator building)
    {
        if (!activeBuildings.Contains(building))
            activeBuildings.Add(building);

        UpdateActiveWorkHUD();
    }

    public void UnregisterActiveBuilding(ResourceGenerator building)
    {
        activeBuildings.Remove(building);
        UpdateActiveWorkHUD();
    }

    private void UpdateActiveWorkHUD()
    {
        if (activeWorkHudPanel == null) return;

        activeBuildings.RemoveAll(b => b == null || !b.isWorking);

        if (activeBuildings.Count == 0)
        {
            if (activeWorkHudPanel.activeSelf) activeWorkHudPanel.SetActive(false);
            return;
        }

        if (!activeWorkHudPanel.activeSelf) activeWorkHudPanel.SetActive(true);

        // Display the building closest to finishing
        ResourceGenerator nextToFinish = activeBuildings[0];
        for (int i = 1; i < activeBuildings.Count; i++)
        {
            if (activeBuildings[i].remainingSeconds < nextToFinish.remainingSeconds)
                nextToFinish = activeBuildings[i];
        }

        if (activeResourceIcon != null)
        {
            switch (nextToFinish.resourceType)
            {
                case ResourceCategory.Stone: activeResourceIcon.sprite = stoneIcon; break;
                case ResourceCategory.Wood:  activeResourceIcon.sprite = woodIcon; break;
                case ResourceCategory.Wheat: activeResourceIcon.sprite = wheatIcon; break;
            }
        }

        if (activeMinutesText != null)
        {
            int totalSecs = Mathf.CeilToInt(Mathf.Max(0f, nextToFinish.remainingSeconds));
            int mins = totalSecs / 60;
            int secs = totalSecs % 60;
            activeMinutesText.text = $"{mins:00}:{secs:00}";
        }
    }
}