using UnityEngine;

public class ResourceGenerator : MonoBehaviour, Interact
{
    [Header("Production Settings")]
    public ResourceCategory resourceType = ResourceCategory.Wheat;
    public int minWorkersRequired = 1;
    public int baseYieldPerMinute = 5;

    [Header("Optional VFX")]
    public GameObject floatingTextPrefab;
    public Vector3 textOffset = new Vector3(0f, 2f, 0f);

    [HideInInspector] public bool isWorking = false;
    [HideInInspector] public int assignedWorkers = 0;
    [HideInInspector] public float remainingSeconds = 0f;
    [HideInInspector] public float totalWorkSeconds = 0f;

    // Triggered when the player clicks the building and walks into interaction range
    public void Interact()
    {
        if (BuildingUIManager.Instance != null)
        {
            BuildingUIManager.Instance.OpenPanel(this);
        }
    }

    void Update()
    {
        if (!isWorking) return;

        remainingSeconds -= Time.deltaTime;

        if (remainingSeconds <= 0f)
        {
            CompleteProduction();
        }
    }

    public void AssignWork(int workers, int minutes)
    {
        if (isWorking || ResourceManager.Instance == null) return;

        int actualAssigned = 0;
        for (int i = 0; i < workers; i++)
        {
            if (ResourceManager.Instance.AssignWorker())
                actualAssigned++;
        }

        if (actualAssigned < minWorkersRequired)
        {
            // Refund if not enough workers were available
            for (int i = 0; i < actualAssigned; i++)
                ResourceManager.Instance.UnassignWorker();
            return;
        }

        assignedWorkers = actualAssigned;
        totalWorkSeconds = minutes * 60f;
        remainingSeconds = totalWorkSeconds;
        isWorking = true;

        if (BuildingUIManager.Instance != null)
            BuildingUIManager.Instance.RegisterActiveBuilding(this);
    }

    // Adjust worker count while production is already running
    public void UpdateWorkersMidProduction(int newWorkerCount)
    {
        if (!isWorking || ResourceManager.Instance == null) return;

        // If dropped below minimum required, recall everyone and stop production
        if (newWorkerCount < minWorkersRequired)
        {
            UnassignAllWorkersMidProduction();
            return;
        }

        int difference = newWorkerCount - assignedWorkers;

        if (difference < 0)
        {
            int toUnassign = Mathf.Abs(difference);
            for (int i = 0; i < toUnassign; i++)
            {
                ResourceManager.Instance.UnassignWorker();
                assignedWorkers--;
            }
        }
        else if (difference > 0)
        {
            for (int i = 0; i < difference; i++)
            {
                if (ResourceManager.Instance.AssignWorker())
                    assignedWorkers++;
            }
        }
    }

    // Immediately unassign all workers and cancel active production
    public void UnassignAllWorkersMidProduction()
    {
        if (!isWorking) return;

        if (ResourceManager.Instance != null)
        {
            for (int i = 0; i < assignedWorkers; i++)
            {
                ResourceManager.Instance.UnassignWorker();
            }
        }

        assignedWorkers = 0;
        remainingSeconds = 0f;
        totalWorkSeconds = 0f;
        isWorking = false;

        if (BuildingUIManager.Instance != null)
            BuildingUIManager.Instance.UnregisterActiveBuilding(this);
    }

    private void CompleteProduction()
    {
        float minutesWorked = totalWorkSeconds / 60f;
        int producedAmount = Mathf.Max(1, Mathf.RoundToInt(baseYieldPerMinute * minutesWorked * assignedWorkers));

        if (ResourceManager.Instance != null)
        {
            switch (resourceType)
            {
                case ResourceCategory.Stone: ResourceManager.Instance.AddRock(producedAmount); break;
                case ResourceCategory.Wood:  ResourceManager.Instance.AddWood(producedAmount); break;
                case ResourceCategory.Wheat: ResourceManager.Instance.AddWheat(producedAmount); break;
            }

            // Return workers to available pool
            for (int i = 0; i < assignedWorkers; i++)
            {
                ResourceManager.Instance.UnassignWorker();
            }
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddResourceCollected(resourceType, producedAmount);
        }

        if (floatingTextPrefab != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, transform.position + textOffset, Quaternion.identity);
            Floattext ft = textObj.GetComponent<Floattext>();
            if (ft != null) ft.Setup($"+{producedAmount} {resourceType}", Color.green);
        }

        assignedWorkers = 0;
        remainingSeconds = 0f;
        totalWorkSeconds = 0f;
        isWorking = false;

        if (BuildingUIManager.Instance != null)
            BuildingUIManager.Instance.UnregisterActiveBuilding(this);
    }
}