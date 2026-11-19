using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum QuestCategory { MisiUtama, MisiSampingan }
public enum QuestType { CollectResource, BuildBuilding, DeliverToNPC }

[System.Serializable]
public class Quest
{
    public string questTitle = "Gather Wood";
    public QuestCategory category = QuestCategory.MisiUtama;
    public QuestType questType = QuestType.CollectResource;

    [TextArea] public string description = "Chop down trees to gather 20 wood.";

    [Header("Target Settings")]
    public ResourceCategory targetResource = ResourceCategory.Wood; // Used for CollectResource & DeliverToNPC
    public string targetBuildingName = "House"; // Must match BuildingData.buildingName for BuildBuilding
    public int targetAmount = 20;
    [HideInInspector] public int currentAmount = 0;

    [Header("Rewards")]
    public float reputationReward = 50f;
    public int rockReward = 10;
    public int woodReward = 10;
    public int wheatReward = 0;

    [HideInInspector] public bool isCompleted = false;
    [HideInInspector] public bool isClaimed = false;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("All Quests")]
    public List<Quest> quests = new List<Quest>();
    private Quest selectedQuest;

    [Header("Left Sidebar Dropdowns")]
    public RectTransform sidebarContentRect;   // Parent container holding both category buttons & lists
    public Transform mainQuestListContainer;   // Child container directly under "Misi Utama" button
    public Transform sideQuestListContainer;   // Child container directly under "Misi Sampingan" button
    public GameObject questButtonPrefab;       // Prefab with QuestListButton attached

    [Header("Dropdown Animation")]
    public float dropdownSlideSpeed = 12f;
    public bool startDropdownsOpen = true;

    [Header("Right Detail Panel")]
    public TextMeshProUGUI questTitleText;       // Top title text
    public TextMeshProUGUI questDescriptionText; // Middle description text
    public TextMeshProUGUI questRewardText;      // Reward text
    public TextMeshProUGUI questStatusText;      // Bottom-left status text
    public TextMeshProUGUI questProgressText;    // Bottom progress text (e.g., "0/5")
    public Button actionButton;                  // Green "Oke" button
    public TextMeshProUGUI actionButtonText;     // Text inside "Oke" button

    private bool isMainDropdownOpen;
    private bool isSideDropdownOpen;
    private float mainCurrentHeight;
    private float sideCurrentHeight;

    private RectTransform mainListRect;
    private RectTransform sideListRect;
    private LayoutElement mainListLayout;
    private LayoutElement sideListLayout;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (quests.Count == 0)
        {
            SetupDefaultTestQuests();
        }

        // Cache RectTransforms and LayoutElements for smooth height animation
        if (mainQuestListContainer != null)
        {
            mainListRect = mainQuestListContainer.GetComponent<RectTransform>();
            mainListLayout = mainQuestListContainer.GetComponent<LayoutElement>();
            if (mainListLayout == null) mainListLayout = mainQuestListContainer.gameObject.AddComponent<LayoutElement>();
        }

        if (sideQuestListContainer != null)
        {
            sideListRect = sideQuestListContainer.GetComponent<RectTransform>();
            sideListLayout = sideQuestListContainer.GetComponent<LayoutElement>();
            if (sideListLayout == null) sideListLayout = sideQuestListContainer.gameObject.AddComponent<LayoutElement>();
        }

        isMainDropdownOpen = startDropdownsOpen;
        isSideDropdownOpen = startDropdownsOpen;

        RefreshQuestLists();

        // Snap to initial open/closed heights without animating on scene start
        mainCurrentHeight = isMainDropdownOpen ? GetUnconstrainedHeight(mainListRect, mainListLayout) : 0f;
        sideCurrentHeight = isSideDropdownOpen ? GetUnconstrainedHeight(sideListRect, sideListLayout) : 0f;

        if (mainListLayout != null) mainListLayout.preferredHeight = mainCurrentHeight;
        if (sideListLayout != null) sideListLayout.preferredHeight = sideCurrentHeight;
        RebuildSidebarLayout();

        if (quests.Count > 0) SelectQuest(quests[0]);
    }

    void Update()
    {
        bool needsLayoutRebuild = false;

        if (mainListRect != null && mainListLayout != null)
        {
            float targetMain = isMainDropdownOpen ? GetUnconstrainedHeight(mainListRect, mainListLayout) : 0f;
            if (Mathf.Abs(mainCurrentHeight - targetMain) > 0.1f)
            {
                mainCurrentHeight = Mathf.Lerp(mainCurrentHeight, targetMain, Time.unscaledDeltaTime * dropdownSlideSpeed);
                if (Mathf.Abs(mainCurrentHeight - targetMain) <= 0.5f) mainCurrentHeight = targetMain;

                mainListLayout.preferredHeight = mainCurrentHeight;
                needsLayoutRebuild = true;
            }
        }

        if (sideListRect != null && sideListLayout != null)
        {
            float targetSide = isSideDropdownOpen ? GetUnconstrainedHeight(sideListRect, sideListLayout) : 0f;
            if (Mathf.Abs(sideCurrentHeight - targetSide) > 0.1f)
            {
                sideCurrentHeight = Mathf.Lerp(sideCurrentHeight, targetSide, Time.unscaledDeltaTime * dropdownSlideSpeed);
                if (Mathf.Abs(sideCurrentHeight - targetSide) <= 0.5f) sideCurrentHeight = targetSide;

                sideListLayout.preferredHeight = sideCurrentHeight;
                needsLayoutRebuild = true;
            }
        }

        if (needsLayoutRebuild)
        {
            RebuildSidebarLayout();
        }
    }

    private void SetupDefaultTestQuests()
    {
        quests.Add(new Quest
        {
            questTitle = "Lumberjack",
            category = QuestCategory.MisiUtama,
            questType = QuestType.CollectResource,
            targetResource = ResourceCategory.Wood,
            targetAmount = 20,
            description = "Collect 20 Wood from trees around the map.",
            reputationReward = 40f,
            rockReward = 15
        });

        quests.Add(new Quest
        {
            questTitle = "Expand Village",
            category = QuestCategory.MisiUtama,
            questType = QuestType.BuildBuilding,
            targetBuildingName = "House",
            targetAmount = 2,
            description = "Construct 2 Houses to increase village workers.",
            reputationReward = 60f,
            woodReward = 25
        });

        quests.Add(new Quest
        {
            questTitle = "Supply the Villager",
            category = QuestCategory.MisiSampingan,
            questType = QuestType.DeliverToNPC,
            targetResource = ResourceCategory.Wheat,
            targetAmount = 15,
            description = "Deliver 15 Wheat to the NPC (talk to the NPC or press Give).",
            reputationReward = 50f,
            rockReward = 20,
            woodReward = 20
        });
    }

    // --- DROPDOWN TOGGLES ---
    public void ToggleMainQuestDropdown()
    {
        isMainDropdownOpen = !isMainDropdownOpen;
    }

    public void ToggleSideQuestDropdown()
    {
        isSideDropdownOpen = !isSideDropdownOpen;
    }

    private float GetUnconstrainedHeight(RectTransform listRect, LayoutElement layoutElem)
    {
        if (listRect == null || layoutElem == null) return 0f;

        float previousHeight = layoutElem.preferredHeight;
        layoutElem.preferredHeight = -1f;
        LayoutRebuilder.ForceRebuildLayoutImmediate(listRect);
        float naturalHeight = LayoutUtility.GetPreferredHeight(listRect);
        layoutElem.preferredHeight = previousHeight;

        return naturalHeight;
    }

    private void RebuildSidebarLayout()
    {
        if (sidebarContentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(sidebarContentRect);
        }
    }

    // --- POPULATE BUTTON LISTS ---
    public void RefreshQuestLists()
    {
        ClearContainer(mainQuestListContainer);
        ClearContainer(sideQuestListContainer);

        foreach (Quest q in quests)
        {
            Transform targetParent = (q.category == QuestCategory.MisiUtama)
                ? mainQuestListContainer
                : sideQuestListContainer;

            if (targetParent != null && questButtonPrefab != null)
            {
                GameObject btnObj = Instantiate(questButtonPrefab, targetParent);
                QuestListButton listBtn = btnObj.GetComponent<QuestListButton>();
                if (listBtn != null) listBtn.Setup(q);
            }
        }

        RebuildSidebarLayout();
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            child.SetParent(null); // Detach immediately so LayoutUtility doesn't double-count before end of frame
            Destroy(child.gameObject);
        }
    }

    // --- SELECT & DISPLAY QUEST DETAILS ---
    public void SelectQuest(Quest quest)
    {
        selectedQuest = quest;
        UpdateDetailUI();
    }

    public void UpdateDetailUI()
    {
        if (selectedQuest == null) return;

        int displayProgress = selectedQuest.currentAmount;
        if (selectedQuest.questType == QuestType.DeliverToNPC && !selectedQuest.isCompleted && ResourceManager.Instance != null)
        {
            switch (selectedQuest.targetResource)
            {
                case ResourceCategory.Stone: displayProgress = Mathf.Min(ResourceManager.Instance.currentRock, selectedQuest.targetAmount); break;
                case ResourceCategory.Wood:  displayProgress = Mathf.Min(ResourceManager.Instance.currentWood, selectedQuest.targetAmount); break;
                case ResourceCategory.Wheat: displayProgress = Mathf.Min(ResourceManager.Instance.currentWheat, selectedQuest.targetAmount); break;
            }
        }

        if (questTitleText != null) questTitleText.text = selectedQuest.questTitle;
        if (questDescriptionText != null) questDescriptionText.text = selectedQuest.description;
        if (questProgressText != null) questProgressText.text = $"{displayProgress}/{selectedQuest.targetAmount}";

        if (questRewardText != null)
        {
            string rewards = $"Reward: +{selectedQuest.reputationReward} Rep";
            if (selectedQuest.rockReward > 0) rewards += $"  +{selectedQuest.rockReward} Batu";
            if (selectedQuest.woodReward > 0) rewards += $"  +{selectedQuest.woodReward} Kayu";
            if (selectedQuest.wheatReward > 0) rewards += $" +{selectedQuest.wheatReward} Gandum";
            questRewardText.text = rewards;
        }

        if (selectedQuest.isClaimed)
        {
            if (questStatusText != null) questStatusText.text = "Selesai";
            if (actionButton != null) actionButton.interactable = false;
            if (actionButtonText != null) actionButtonText.text = "Selesai";
        }
        else if (selectedQuest.isCompleted)
        {
            if (questStatusText != null) questStatusText.text = "Siap diterima";
            if (actionButton != null) actionButton.interactable = true;
            if (actionButtonText != null) actionButtonText.text = "Oke";
        }
        else if (selectedQuest.questType == QuestType.DeliverToNPC)
        {
            bool canDeliver = ResourceManager.Instance != null && ResourceManager.Instance.HasResource(selectedQuest.targetResource, selectedQuest.targetAmount);
            if (questStatusText != null) questStatusText.text = canDeliver ? "Siap Dikirim" : $"Butuh {selectedQuest.targetAmount} {selectedQuest.targetResource}";
            if (actionButton != null) actionButton.interactable = canDeliver;
            if (actionButtonText != null) actionButtonText.text = "Give";
        }
        else
        {
            if (questStatusText != null) questStatusText.text = "Dalam Pengerjaan";
            if (actionButton != null) actionButton.interactable = false;
            if (actionButtonText != null) actionButtonText.text = "Oke";
        }
    }

    // --- BUTTON ACTION ("Oke" / "Give" / "Done") ---
    public void OnActionButtonClicked()
    {
        if (selectedQuest == null || selectedQuest.isClaimed) return;

        if (selectedQuest.questType == QuestType.DeliverToNPC && !selectedQuest.isCompleted)
        {
            if (!TryDeliverQuestResources(selectedQuest)) return;
        }

        if (selectedQuest.isCompleted && !selectedQuest.isClaimed)
        {
            if (ReputationManager.Instance != null)
                ReputationManager.Instance.AddReputation(selectedQuest.reputationReward);

            if (ResourceManager.Instance != null)
            {
                if (selectedQuest.rockReward > 0) ResourceManager.Instance.AddRock(selectedQuest.rockReward);
                if (selectedQuest.woodReward > 0) ResourceManager.Instance.AddWood(selectedQuest.woodReward);
                if (selectedQuest.wheatReward > 0) ResourceManager.Instance.AddWheat(selectedQuest.wheatReward);
            }

            selectedQuest.isClaimed = true;
            RefreshQuestLists();
            UpdateDetailUI();
        }
    }

    public bool TryDeliverQuestResources(Quest quest)
    {
        if (ResourceManager.Instance == null) return false;

        if (ResourceManager.Instance.SpendSingleResource(quest.targetResource, quest.targetAmount))
        {
            quest.currentAmount = quest.targetAmount;
            quest.isCompleted = true;
            RefreshQuestLists();
            UpdateDetailUI();
            return true;
        }
        return false;
    }

    // --- PROGRESS HOOKS FOR GAMEPLAY EVENTS ---
    public void AddResourceCollected(ResourceCategory resource, int amount)
    {
        bool changed = false;
        foreach (Quest q in quests)
        {
            if (!q.isCompleted && q.questType == QuestType.CollectResource && q.targetResource == resource)
            {
                q.currentAmount = Mathf.Min(q.currentAmount + amount, q.targetAmount);
                if (q.currentAmount >= q.targetAmount) q.isCompleted = true;
                changed = true;
            }
        }

        if (changed)
        {
            RefreshQuestLists();
            UpdateDetailUI();
        }
    }

    public void OnBuildingConstructed(string buildingName)
    {
        bool changed = false;
        foreach (Quest q in quests)
        {
            if (!q.isCompleted && q.questType == QuestType.BuildBuilding &&
                string.Equals(q.targetBuildingName, buildingName, System.StringComparison.OrdinalIgnoreCase))
            {
                q.currentAmount = Mathf.Min(q.currentAmount + 1, q.targetAmount);
                if (q.currentAmount >= q.targetAmount) q.isCompleted = true;
                changed = true;
            }
        }

        if (changed)
        {
            RefreshQuestLists();
            UpdateDetailUI();
        }
    }
}