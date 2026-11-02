using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMov : MonoBehaviour
{
    private UnityEngine.AI.NavMeshAgent agent;
    public float interactionRange = 2f;

    private Interact pendingInteractable;
    private Transform pendingTargetTransform;

    void Start()
    {
        // Grabs the NavMeshAgent component off your character
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    void Update()
    {
        // 1. Handle Clicking
        if (Input.GetMouseButtonDown(0))
        {
            // Prevent raycasting if the mouse is currently hovering over a UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; // Stop running the rest of this code
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Interact interactable = hit.collider.GetComponent<Interact>();
                
                if (interactable != null)
                {
                    pendingInteractable = interactable;
                    pendingTargetTransform = hit.transform;
                    agent.SetDestination(hit.transform.position);
                }
                else
                {
                    pendingInteractable = null;
                    pendingTargetTransform = null;
                    agent.SetDestination(hit.point);
                }
            }
        }

        // 2. Handle Auto-Interaction
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