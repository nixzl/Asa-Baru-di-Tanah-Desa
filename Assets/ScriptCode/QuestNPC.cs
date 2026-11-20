using UnityEngine;

public class QuestNPC : MonoBehaviour, Interact
{
    [Header("NPC Identity")]
    public string npcName = "Warga Desa";

    [Header("Mission Offered by This NPC")]
    public Quest missionToGive;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)]
    public string[] offerDialogue = new string[]
    {
        "Halo! Desa kita sedang membutuhkan bantuan.",
        "Bisakah kamu mengumpulkan kayu dari pohon di sekitar sini?"
    };

    [TextArea(2, 4)]
    public string[] inProgressDialogue = new string[]
    {
        "Bagaimana kabarnya? Tugas yang kuberikan belum selesai.",
        "Kembali lagi ke sini kalau sudah selesai ya!"
    };

    [TextArea(2, 4)]
    public string[] deliverySuccessDialogue = new string[]
    {
        "Wah, kamu sudah membawa semua bahan yang dibutuhkan!",
        "Terima kasih banyak atas bantuanmu!"
    };

    [TextArea(2, 4)]
    public string[] completedDialogue = new string[]
    {
        "Terima kasih sudah membantu desa kita sebelumnya!"
    };

    [Header("Floating Feedback (Optional)")]
    public GameObject floatingTextPrefab;
    public Vector3 textOffset = new Vector3(0f, 2.2f, 0f);

    public void Interact()
    {
        if (DialogueManager.Instance == null || QuestManager.Instance == null) return;

        // 1. Player has NOT accepted this NPC's mission yet -> Offer it via dialogue
        if (!QuestManager.Instance.HasQuest(missionToGive) || missionToGive.isFailed)
        {
            DialogueManager.Instance.StartDialogue(npcName, offerDialogue, missionToGive);
            return;
        }

        // 2. Mission is already completed or claimed
        if (missionToGive.isClaimed || (missionToGive.isCompleted && missionToGive.questType != QuestType.DeliverToNPC))
        {
            DialogueManager.Instance.StartDialogue(npcName, completedDialogue);
            return;
        }

        // 3. Mission is a resource delivery (Stone, Wood, or Wheat)
        if (missionToGive.questType == QuestType.DeliverToNPC && !missionToGive.isCompleted)
        {
            bool hasEnough = ResourceManager.Instance != null &&
                             ResourceManager.Instance.HasResource(missionToGive.targetResource, missionToGive.targetAmount);

            if (hasEnough)
            {
                DialogueManager.Instance.StartDialogue(npcName, deliverySuccessDialogue, null, () =>
                {
                    if (QuestManager.Instance.TryDeliverQuestResources(missionToGive))
                    {
                        SpawnFeedback($"-{missionToGive.targetAmount} {missionToGive.targetResource}", Color.green);
                    }
                });
            }
            else
            {
                DialogueManager.Instance.StartDialogue(npcName, inProgressDialogue);
            }
            return;
        }

        // 4. Mission is still in progress
        DialogueManager.Instance.StartDialogue(npcName, inProgressDialogue);
    }

    private void SpawnFeedback(string message, Color color)
    {
        if (floatingTextPrefab == null) return;
        GameObject obj = Instantiate(floatingTextPrefab, transform.position + textOffset, Quaternion.identity);
        Floattext ft = obj.GetComponent<Floattext>();
        if (ft != null) ft.Setup(message, color);
    }
}