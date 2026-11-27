using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCMission
{
    public Quest mission;

    [Header("Dialogue for This Mission")]
    [TextArea(2, 4)]
    public string[] offerDialogue = new string[]
    {
        "Halo! Desa kita sedang membutuhkan bantuan.",
        "Bisakah kamu membantu menyelesaikan tugas ini?"
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
}

public class QuestNPC : MonoBehaviour, Interact
{
    [Header("NPC Identity")]
    public string npcName = "Kepala Desa";
    public Sprite npcPortrait;

    [Header("Missions Offered by This NPC (In Order)")]
    public List<NPCMission> missions = new List<NPCMission>();
    public bool disappearWithTransitionAfterAllMissions = true;

    [Header("Shared NPC Dialogue")]
    [TextArea(2, 4)]
    public string[] lockedPrerequisiteDialogue = new string[]
    {
        "Maaf, ada urusan lain di desa yang harus kamu selesaikan lebih dulu.",
        "Kembali lagi ke sini setelah tugas itu selesai ya!"
    };

    [TextArea(2, 4)]
    public string[] readyToClaimDialogue = new string[]
    {
        "Tugasmu sudah selesai! Buka panel Misi dan tekan 'Oke' untuk mengambil hadiahmu.",
        "Setelah itu bicara denganku lagi ya!"
    };

    [TextArea(2, 4)]
    public string[] allCompletedDialogue = new string[]
    {
        "Terima kasih banyak sudah menyelesaikan semua bantuanku!"
    };

    [Header("Floating Feedback (Optional)")]
    public GameObject floatingTextPrefab;
    public Vector3 textOffset = new Vector3(0f, 2.2f, 0f);

    // Exposes the currently active/next available mission for NPCQuestIndicator
    public Quest ActiveMission
    {
        get
        {
            NPCMission current = GetCurrentMissionEntry();
            if (current != null) return current.mission;
            if (missions != null && missions.Count > 0) return missions[missions.Count - 1].mission;
            return null;
        }
    }

    void Awake()
    {
        if (missions == null) return;

        for (int i = 0; i < missions.Count; i++)
        {
            Quest q = missions[i].mission;
            if (q == null) continue;

            q.ownerNPC = this;

            // Only despawn the NPC after their FINAL mission is claimed
            bool isFinalMission = (i == missions.Count - 1);
            if (disappearWithTransitionAfterAllMissions && isFinalMission && q.category == QuestCategory.MisiSampingan)
            {
                q.despawnNPCWithTransition = true;
            }
        }
    }

    private NPCMission GetCurrentMissionEntry()
    {
        if (missions == null) return null;

        foreach (NPCMission entry in missions)
        {
            if (entry != null && entry.mission != null && !entry.mission.isClaimed)
            {
                return entry;
            }
        }
        return null;
    }

    public void Interact()
    {
        if (DialogueManager.Instance == null || QuestManager.Instance == null) return;

        NPCMission currentEntry = GetCurrentMissionEntry();

        // All missions from this NPC have been completed and claimed
        if (currentEntry == null)
        {
            DialogueManager.Instance.StartDialogue(npcName, allCompletedDialogue, npcPortrait);
            return;
        }

        Quest mission = currentEntry.mission;

        // 0. Prerequisite mission is not completed yet
        if (!QuestManager.Instance.IsPrerequisiteMet(mission))
        {
            DialogueManager.Instance.StartDialogue(npcName, lockedPrerequisiteDialogue, npcPortrait);
            return;
        }

        // 1. Not accepted yet OR previously failed -> Offer/Retry via dialogue
        if (!QuestManager.Instance.HasQuest(mission) || mission.isFailed)
        {
            DialogueManager.Instance.StartDialogue(npcName, currentEntry.offerDialogue, npcPortrait, mission);
            return;
        }

        // 2. Mission is completed and waiting to be claimed in the Misi panel
        if (mission.isCompleted && !mission.isClaimed)
        {
            DialogueManager.Instance.StartDialogue(npcName, readyToClaimDialogue, npcPortrait);
            return;
        }

        // 3. Mission has DeliverToNPC objectives (can deliver multiple resources at once)
        if (mission.HasDeliveryObjective() && !mission.isCompleted)
        {
            if (QuestManager.Instance.CanDeliverAllObjectives(mission))
            {
                DialogueManager.Instance.StartDialogue(npcName, currentEntry.deliverySuccessDialogue, npcPortrait, null, () =>
                {
                    if (QuestManager.Instance.TryDeliverQuestResources(mission))
                    {
                        SpawnFeedback("Dikirim", Color.green);

                        if (mission.despawnNPCWithTransition)
                        {
                            QuestManager.Instance.ClaimQuestRewards(mission);
                        }
                    }
                });
            }
            else
            {
                DialogueManager.Instance.StartDialogue(npcName, currentEntry.inProgressDialogue, npcPortrait);
            }
            return;
        }

        // 4. Mission is still in progress
        DialogueManager.Instance.StartDialogue(npcName, currentEntry.inProgressDialogue, npcPortrait);
    }

    private void SpawnFeedback(string message, Color color)
    {
        if (floatingTextPrefab == null) return;
        GameObject obj = Instantiate(floatingTextPrefab, transform.position + textOffset, Quaternion.identity);
        Floattext ft = obj.GetComponent<Floattext>();
        if (ft != null) ft.Setup(message, color);
    }
}