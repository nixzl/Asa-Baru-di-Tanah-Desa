using System.Collections.Generic;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    [Header("Rock Types to Spawn")]
    public List<RockData> rockTypes = new List<RockData>();

    [Header("Spawn Settings")]
    public int rockCount = 15;
    public Vector3 spawnAreaSize = new Vector3(40f, 10f, 40f);
    public bool spawnOnStart = true;

    [Header("Placement Rules")]
    public LayerMask groundLayer;
    public LayerMask obstacleLayer; // Select Buildings, Roads, or other Rocks to prevent overlap
    public float checkRadius = 1.5f;
    public bool randomRotation = true;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnRocks();
        }
    }

    public void SpawnRocks()
    {
        if (rockTypes == null || rockTypes.Count == 0)
        {
            Debug.LogWarning("RockSpawner has no RockData assigned!");
            return;
        }

        int spawned = 0;
        int maxAttempts = rockCount * 10; // Prevents infinite loops if the map is too crowded
        int attempts = 0;

        while (spawned < rockCount && attempts < maxAttempts)
        {
            attempts++;

            // Pick a random X and Z inside the spawner box
            float randomX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float randomZ = Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f);
            Vector3 rayOrigin = transform.position + new Vector3(randomX, spawnAreaSize.y / 2f, randomZ);

            // Raycast downward to find the exact ground height of the map
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, spawnAreaSize.y + 20f, groundLayer))
            {
                // Skip this spot if a building or another rock is already too close
                if (Physics.CheckSphere(hit.point, checkRadius, obstacleLayer))
                {
                    continue;
                }

                // Pick a random RockData ScriptableObject from the list
                RockData selectedData = rockTypes[Random.Range(0, rockTypes.Count)];
                if (selectedData == null || selectedData.rockPrefab == null) continue;

                Quaternion spawnRot = randomRotation
                    ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                    : Quaternion.identity;

                GameObject rockObj = Instantiate(selectedData.rockPrefab, hit.point, spawnRot, transform);

                // Ensure CollectibleRock is attached and pass the ScriptableObject data
                CollectibleRock rockScript = rockObj.GetComponent<CollectibleRock>();
                if (rockScript == null) rockScript = rockObj.AddComponent<CollectibleRock>();
                rockScript.Initialize(selectedData);

                spawned++;
            }
        }
    }

    // Draws a green wireframe box in the Scene View so you can position and resize the spawn area
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.75f);
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}