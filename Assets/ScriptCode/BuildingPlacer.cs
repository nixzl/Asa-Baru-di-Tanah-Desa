using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    public static BuildingPlacer Instance;
    public float lastPlacementTime = 0f;
    public BuildingData pendingBuilding;
    private GameObject previewBuilding; 
    private float currentRotation = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartPlacement(BuildingData buildingToPlace)
    {
        if (previewBuilding != null) Destroy(previewBuilding);

        pendingBuilding = buildingToPlace;
        currentRotation = 0f; 

        previewBuilding = Instantiate(buildingToPlace.buildingPrefab);
        SetLayerRecursively(previewBuilding, LayerMask.NameToLayer("Ignore Raycast"));
    }

    public void CancelPlacement()
{
    if (previewBuilding != null) Destroy(previewBuilding);
    pendingBuilding = null;
}

    void Update()
    {
        if (pendingBuilding != null && previewBuilding != null)
        {
            if (Input.GetKeyDown(KeyCode.R)) currentRotation += 90f; 
            currentRotation += Input.mouseScrollDelta.y * 15f; 

            if (Input.GetMouseButtonDown(1))
            {
                Destroy(previewBuilding);
                pendingBuilding = null;
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            // Limit raycast to ONLY hit the floor, preventing the building from climbing walls
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                previewBuilding.transform.position = hit.point;
                previewBuilding.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    BuildingValidator validator = previewBuilding.GetComponent<BuildingValidator>();
                    
                    // Block placement if the building is glowing red
                    if (validator != null && !validator.isValid)
                    {
                        Debug.Log("Cannot build here! Area is blocked.");
                        return;
                    }

                    if (ResourceManager.Instance.CanAfford(pendingBuilding.rockCost, pendingBuilding.woodCost))
                    {
                        ResourceManager.Instance.SpendResources(pendingBuilding.rockCost, pendingBuilding.woodCost);
                        lastPlacementTime = Time.time;

                        // Finalize placement: Restore default layers and physics
                        SetLayerRecursively(previewBuilding, LayerMask.NameToLayer("Default"));
                        
                        Collider[] colliders = previewBuilding.GetComponentsInChildren<Collider>();
                        foreach (Collider col in colliders) col.isTrigger = false; 
                        
                        // Remove preview mechanics so it becomes a normal static building
                        Destroy(previewBuilding.GetComponent<Rigidbody>());
                        Destroy(validator);

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