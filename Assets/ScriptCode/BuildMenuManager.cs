using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BuildMenuManager : MonoBehaviour
{
    public static BuildMenuManager Instance;
    [Header("Animate for panel open/close")]
    public RectTransform buildMenuRect;
    public Vector2 menuClosedPos = new Vector2(0, -1000);
    public Vector2 menuOpenPos = new Vector2(0, 0);  

    [Header("Animation Settings")]
    public RectTransform actionButtonsGroup;
    public Vector2 buttonClosedPosition;
    public Vector2 buttonOpenPosition;
    public float animationDuration = 10f;

    [Header("Buidling Info")]
    public GameObject buildingInfoPanel;
    public TextMeshProUGUI infoTitleText;
    public TextMeshProUGUI infoDescriptionText;
    public TextMeshProUGUI infoRequirementText;

    [Header("UI References")]
    public GameObject buildMenuPanel;
    public Transform gridContainer; // The container holding the UI grid
    public GameObject buildButtonPrefab;

    [Header("Available Buildings")]
    public List<BuildingData> buildingList;

    public bool isMenuOpen { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        buildMenuPanel.SetActive(false);
        isMenuOpen = false;
        PopulateMenu();
    }

    void Update()
    {
        // Toggle menu open/close with the 'B' key
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMenu();
        }
        if (actionButtonsGroup != null)
        {
            Vector2 targetPosition = isMenuOpen ? buttonOpenPosition : buttonClosedPosition;
            actionButtonsGroup.anchoredPosition = Vector2.Lerp(actionButtonsGroup.anchoredPosition, targetPosition, Time.deltaTime * animationDuration);
        }
        if (buildMenuRect != null && buildMenuPanel.activeSelf)
        {
            Vector2 targetMenuPos = isMenuOpen ? menuOpenPos : menuClosedPos;
            buildMenuRect.anchoredPosition = Vector2.Lerp(buildMenuRect.anchoredPosition, targetMenuPos, Time.deltaTime * animationDuration);
            if (!isMenuOpen && Vector2.Distance(buildMenuRect.anchoredPosition, menuClosedPos) <5f)
            {
                buildMenuPanel.SetActive(false);
            }
        }
    }

   public void ShowBuildingInfo(BuildingData data)
    {
        if (buildingInfoPanel == null || data == null) return;

        buildingInfoPanel.SetActive(true);

        if (infoTitleText != null)
        infoTitleText.text = data.buildingName;

        if (infoDescriptionText != null)
        infoDescriptionText.text = string.IsNullOrWhiteSpace(data.description)
            ? "No description available."
            : data.description;

        if (infoRequirementText != null)
         {
            int currentPop = ResourceManager.Instance != null ? ResourceManager.Instance.totalWorkers : 0;
            int currentRep = ReputationManager.Instance != null ? ReputationManager.Instance.currentLevel : 1;

            bool popMet = currentPop >= data.requiredTotalPopulation;
            bool repMet = currentRep >= data.requiredTotalReputation;

        if (popMet && repMet)
            {
                infoRequirementText.text = data.workersProvided > 0
                    ? $"+{data.workersProvided} Pekerja"
                    : "";
                infoRequirementText.color = new Color(0.2f, 0.75f, 0.25f, 1f);
            }
        else
            {
                // Clearly list what is still required to unlock the building
                string reqText = "Membutuhkan:\n";
                if (data.requiredTotalPopulation > 0)
                    reqText += $"• Populasi: {currentPop}/{data.requiredTotalPopulation}\n";
                if (data.requiredTotalReputation > 0)
                    reqText += $"• Reputation Level: {currentRep}/{data.requiredTotalReputation}";

                infoRequirementText.text = reqText.TrimEnd();
                infoRequirementText.color = Color.red;
            }
        }
    }

public void HideBuildingInfo()
{
    if (buildingInfoPanel != null)
    {
        buildingInfoPanel.SetActive(false);
    }
} 

    public void ToggleBuildMenu()
    {
        isMenuOpen = !isMenuOpen;
        if (isMenuOpen)
        {
            buildMenuPanel.SetActive(true);
            if (buildMenuRect != null) buildMenuRect.anchoredPosition = menuClosedPos;
            PopulateMenu();
        }
        else
        {
            
        }
        if (isMenuOpen && BuildingPlacer.Instance != null && BuildingPlacer.Instance.pendingBuilding != null)
        {
            BuildingPlacer.Instance.CancelPlacement();
        }
    }

    private void PopulateMenu()
    {
        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        foreach (BuildingData data in buildingList)
        {
            GameObject newButton = Instantiate(buildButtonPrefab, gridContainer);
            BuildMenuButton buttonScript = newButton.GetComponent<BuildMenuButton>();
            if (buttonScript != null) buttonScript.Setup(data);
        }
    }

    public void SelectBuilding(BuildingData data)
    {
        buildMenuPanel.SetActive(false);
        isMenuOpen = false;
        BuildingPlacer.Instance.StartPlacement(data);
    }
}