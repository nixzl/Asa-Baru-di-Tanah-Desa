using UnityEngine;
using System.Collections.Generic;

public class BuildMenuManager : MonoBehaviour
{
    public static BuildMenuManager Instance;

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
    }

    public void ToggleBuildMenu()
    {
        isMenuOpen = !isMenuOpen;
        buildMenuPanel.SetActive(isMenuOpen);

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