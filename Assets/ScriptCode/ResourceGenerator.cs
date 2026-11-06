using UnityEngine;

public class ResourceGenerator : MonoBehaviour, Interact
{
    public enum ResourceType { Rock, Wood }

    public int minWorkersRequired = 1; // Minimum workers needed to start generating resources
    
    [Header("Settings")]
    public ResourceType resourceType;
    public int minAmount = 5;
    public int maxAmount = 15;
    
    [Header("VFX")]
    public GameObject floatingTextPrefab;
    public Transform textSpawnPoint; // A spot floating slightly above the roof

    [Header("Current Status")]
    public int assignedWorkers = 0;
    public float workTimeRemaining = 0f; // Tracked in seconds
    private float generateTimer = 0f;

    public void Interact()
    {
        // Open the UI Panel when the player walks up and touches the building
        BuildingUIManager.Instance.OpenPanel(this);
    }

    void Update()
    {
        if (assignedWorkers > 0 && workTimeRemaining > 0)
        {
            workTimeRemaining -= Time.deltaTime;
            generateTimer += Time.deltaTime;

            // Every 30 seconds, generate resources
            if (generateTimer >= 30f) 
            {
                GenerateResources();
                generateTimer = 0f;
            }

            // When the total assigned time runs out
            if (workTimeRemaining <= 0)
            {
                StopWorking();
            }
        }
    }

    public void AssignWork(int workers, int minutes)
    {
        for (int i = 0; i < workers; i++) ResourceManager.Instance.AssignWorker();
        
        assignedWorkers += workers;
        workTimeRemaining += (minutes * 60f); // Convert minutes to seconds
        
        SpawnFloatingText($"+{workers} Worker(s)", Color.yellow);
    }

    private void GenerateResources()
    {
        // Multiply the resources gathered by the number of workers assigned
        int amountToGive = Random.Range(minAmount, maxAmount + 1) * assignedWorkers;

        if (resourceType == ResourceType.Rock) ResourceManager.Instance.AddRock(amountToGive);
        else if (resourceType == ResourceType.Wood) ResourceManager.Instance.AddWood(amountToGive);

        SpawnFloatingText($"+{amountToGive} {resourceType}", Color.green);
    }

    private void StopWorking()
    {
        for (int i = 0; i < assignedWorkers; i++) ResourceManager.Instance.UnassignWorker();
        
        if (assignedWorkers > 0) SpawnFloatingText($"-{assignedWorkers} Worker(s) Finished", Color.red);
        
        assignedWorkers = 0;
        workTimeRemaining = 0f;
        generateTimer = 0f;
    }

    public void SpawnFloatingText(string message, Color color)
    {
        if (floatingTextPrefab != null && textSpawnPoint != null)
        {
            GameObject textObj = Instantiate(floatingTextPrefab, textSpawnPoint.position, Quaternion.identity);
            textObj.GetComponent<Floattext>().Setup(message, color);
        }
    }
}