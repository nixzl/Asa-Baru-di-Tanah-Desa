using System.Collections;
using UnityEngine;

public class CollectibleRock : MonoBehaviour, Interact
{
    [Header("Data")]
    public RockData rockData;

    private Collider rockCollider;
    private Renderer[] rockRenderers;

    void Awake()
    {
        rockCollider = GetComponent<Collider>();
        rockRenderers = GetComponentsInChildren<Renderer>();
    }

    // Called by RockSpawner when spawning rocks or trees dynamically
    public void Initialize(RockData data)
    {
        rockData = data;
    }

    public void Interact()
    {
        if (rockData == null)
        {
            Debug.LogWarning("CollectibleRock is missing its RockData ScriptableObject!", this);
            return;
        }

        // 1. Calculate random yield
        int collectedAmount = Random.Range(rockData.minRockAmount, rockData.maxRockAmount + 1);

        // 2. Add to global resources based on the ScriptableObject's ResourceCategory
        if (ResourceManager.Instance != null)
        {
            switch (rockData.resourceType)
            {
                case ResourceCategory.Stone:
                    ResourceManager.Instance.AddRock(collectedAmount);
                    break;
                case ResourceCategory.Wood:
                    ResourceManager.Instance.AddWood(collectedAmount);
                    break;
                case ResourceCategory.Wheat:
                    ResourceManager.Instance.AddWheat(collectedAmount);
                    break;
            }
        }

        // 3. Update active CollectResource quests in QuestManager
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddResourceCollected(rockData.resourceType, collectedAmount);
        }

        // 4. Spawn floating text feedback
        if (rockData.floatingTextPrefab != null)
        {
            GameObject textObj = Instantiate(
                rockData.floatingTextPrefab,
                transform.position + rockData.textOffset,
                Quaternion.identity
            );

            Floattext floatText = textObj.GetComponent<Floattext>();
            if (floatText != null)
            {
                floatText.Setup($"+{collectedAmount} {rockData.resourceType}", Color.white);
            }
        }

        // 5. Respawn or destroy
        if (rockData.canRespawn)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator RespawnRoutine()
    {
        SetRockVisible(false);
        yield return new WaitForSeconds(rockData.respawnDelay);
        SetRockVisible(true);
    }

    private void SetRockVisible(bool state)
    {
        if (rockCollider != null) rockCollider.enabled = state;
        foreach (Renderer r in rockRenderers)
        {
            r.enabled = state;
        }
    }
}