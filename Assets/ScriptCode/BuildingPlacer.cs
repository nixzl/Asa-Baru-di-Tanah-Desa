using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Placement Settings")]
    public GameObject instructionTextUI;
    public LayerMask groundLayer;
    public GameObject buildMarkerPrefab; // Optional ground highlight prefab

    public static BuildingPlacer Instance;
    public float lastPlacementTime = 0f;
    public BuildingData pendingBuilding;
    private GameObject previewBuilding;
    private float currentRotation = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Fallback to the "Ground" layer if Ground Layer isn't assigned in the Inspector
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
    }

    public void StartPlacement(BuildingData buildingToPlace)
    {
        if (previewBuilding != null) Destroy(previewBuilding);

        pendingBuilding = buildingToPlace;
        currentRotation = 0f;

        previewBuilding = Instantiate(buildingToPlace.buildingPrefab);
        SetLayerRecursively(previewBuilding, LayerMask.NameToLayer("Ignore Raycast"));

        foreach (Collider col in previewBuilding.GetComponentsInChildren<Collider>())
            col.isTrigger = true;

        Rigidbody rb = previewBuilding.GetComponent<Rigidbody>();
        if (rb == null) rb = previewBuilding.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        BuildingValidator validator = previewBuilding.GetComponent<BuildingValidator>();
        if (validator == null) validator = previewBuilding.AddComponent<BuildingValidator>();
        validator.BeginBuildPhase(groundLayer, buildMarkerPrefab);

        if (instructionTextUI != null) instructionTextUI.SetActive(true);
    }

    public void CancelPlacement()
    {
        if (previewBuilding != null) Destroy(previewBuilding);
        pendingBuilding = null;
        if (instructionTextUI != null) instructionTextUI.SetActive(false);
    }

    void Update()
    {
        if (pendingBuilding != null && previewBuilding != null)
        {
            if (Input.GetKeyDown(KeyCode.R)) currentRotation += 90f;
            currentRotation += Input.mouseScrollDelta.y * 15f;

            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Limit raycast to ONLY hit the floor, preventing the building from climbing walls
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                previewBuilding.transform.position = hit.point;
                previewBuilding.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    BuildingValidator validator = previewBuilding.GetComponent<BuildingValidator>();

                    // Block placement if the building is glowing red
                    if (validator != null && !validator.IsValidPlacement)
                    {
                        Debug.Log("Cannot place building here: area is blocked!");
                        return;
                    }

                    if (ResourceManager.Instance.CanAfford(pendingBuilding.rockCost, pendingBuilding.woodCost))
                    {
                        ResourceManager.Instance.SpendResources(pendingBuilding.rockCost, pendingBuilding.woodCost);
                        ResourceManager.Instance.AddPopulation(pendingBuilding.workersProvided);

                        if (QuestManager.Instance != null)
                        {
                            QuestManager.Instance.OnBuildingConstructed(pendingBuilding.buildingName);
                        }
                        lastPlacementTime = Time.time;

                        if (validator != null)
                        {
                            validator.RestoreAndCleanup();
                            Destroy(validator);
                        }

                        if (instructionTextUI != null) instructionTextUI.SetActive(false);

                        // Finalize placement: Restore default layers and physics
                        SetLayerRecursively(previewBuilding, LayerMask.NameToLayer("Default"));

                        Collider[] colliders = previewBuilding.GetComponentsInChildren<Collider>();
                        foreach (Collider col in colliders) col.isTrigger = false;

                        // Remove preview mechanics so it becomes a normal static building
                        Destroy(previewBuilding.GetComponent<Rigidbody>());

                        previewBuilding = null;
                        pendingBuilding = null;
                    }
                }
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}