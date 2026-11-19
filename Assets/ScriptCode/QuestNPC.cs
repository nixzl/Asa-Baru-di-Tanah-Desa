using UnityEngine;

public class QuestNPC : MonoBehaviour, Interact
{
    public GameObject floatingTextPrefab;
    public Vector3 textOffset = new Vector3(0f, 2f, 0f);

    public void Interact()
    {
        if (QuestManager.Instance == null) return;

        foreach (Quest q in QuestManager.Instance.quests)
        {
            if (!q.isCompleted && q.questType == QuestType.DeliverToNPC)
            {
                if (QuestManager.Instance.TryDeliverQuestResources(q))
                {
                    SpawnFeedback($"Delivered {q.targetAmount} {q.targetResource}!", Color.green);
                    return;
                }
                else
                {
                    SpawnFeedback($"Need {q.targetAmount} {q.targetResource}", Color.yellow);
                    return;
                }
            }
        }

        SpawnFeedback("No deliveries needed!", Color.white);
    }

    private void SpawnFeedback(string msg, Color color)
    {
        if (floatingTextPrefab == null) return;
        GameObject obj = Instantiate(floatingTextPrefab, transform.position + textOffset, Quaternion.identity);
        Floattext ft = obj.GetComponent<Floattext>();
        if (ft != null) ft.Setup(msg, color);
    }
}