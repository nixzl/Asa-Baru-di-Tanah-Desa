using UnityEngine;
using System.Collections.Generic;

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