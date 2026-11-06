using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMov : MonoBehaviour
{
    private NavMeshAgent agent;
    public float interactionRange = 2f;

    private Interact pendingInteractable;
    private Transform pendingTargetTransform;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // 1. FREEZE MOVEMENT: Stop movement while browsing menu or placing buildings
        bool isPlacingBuilding = BuildingPlacer.Instance != null && BuildingPlacer.Instance.pendingBuilding != null;
        bool isMenuOpen = BuildMenuManager.Instance != null && BuildMenuManager.Instance.isMenuOpen;

        if (isPlacingBuilding || isMenuOpen)
        {
            if (agent.hasPath) agent.ResetPath(); // Instantly stops movement
            return; // Skip rest of movement logic while building
        }
        if (BuildingPlacer.Instance != null && (Time.time - BuildingPlacer.Instance.lastPlacementTime < 0.2f))
         {
            return;
        }
        // 2. Handle Mouse Clicks for Movement & Interaction
        if (Input.GetMouseButtonDown(0))
        {
            // Prevent movement clicks if clicking UI elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the clicked object implements IInteractable (NPC, Collectible, Quest Board, etc.)
                Interact interactable = hit.collider.GetComponent<Interact>();
                
                if (interactable != null)
                {
                    pendingInteractable = interactable;
                    pendingTargetTransform = hit.transform;
                    agent.SetDestination(hit.transform.position);
                }
                else
                {
                    // Clicked regular ground
                    pendingInteractable = null;
                    pendingTargetTransform = null;
                    agent.SetDestination(hit.point);
                }
            }
        }

        // 3. Handle Auto-Interaction when arriving in range
        if (pendingInteractable != null && pendingTargetTransform != null)
        {
            float distance = Vector3.Distance(transform.position, pendingTargetTransform.position);
            
            if (distance <= interactionRange)
            {
                agent.ResetPath(); 
                pendingInteractable.Interact(); 
                
                pendingInteractable = null; 
                pendingTargetTransform = null;
            }
        }
    }
}