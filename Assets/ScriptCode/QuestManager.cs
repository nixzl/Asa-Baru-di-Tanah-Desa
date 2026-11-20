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
    public ResourceCategory targetResource = ResourceCategory.Wood;
    public string targetBuildingName = "House";
    public int targetAmount = 20;
    [HideInInspector] public int currentAmount = 0;

    [Header("Time Limit (Optional)")]
    public bool hasTimeLimit = false;
    public float timeLimitSeconds = 120f;
    [HideInInspector] public float remainingTime = 0f;
    [HideInInspector] public bool isFailed = false;

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
    public RectTransform sidebarContentRect;
    public Transform mainQuestListContainer;
    public Transform sideQuestListContainer;
    public GameObject questButtonPrefab;

    [Header("Dropdown Animation")]
    public float dropdownSlideSpeed = 12f;
    public bool startDropdownsOpen = true;

    [Header("Right Detail Panel")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questRewardText;
    public TextMeshProUGUI questStatusText;
    public TextMeshProUGUI questProgressText;
    public TextMeshProUGUI questTimerText; // Optional: leave empty to show timer inside Status Text
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    private bool isMainDropdownOpen;
    private bool isSideDropdownOpen;
    private float mainCurrentHeight;
    private float sideCurrentHeight;

    private RectTransform mainListRect;
    private RectTransform sideListRect;
    private LayoutElement mainListLayout;
    private LayoutElement sideListLayout;
    private Color defaultStatusColor = Color.white;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (questStatusText != null) defaultStatusColor = questStatusText.color;
    }

    void Start()
    {
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

        mainCurrentHeight = isMainDropdownOpen ? GetUnconstrainedHeight(mainListRect, mainListLayout) : 0f;
        sideCurrentHeight = isSideDropdownOpen ? GetUnconstrainedHeight(sideListRect, sideListLayout) : 0f;

        if (mainListLayout != null) mainListLayout.preferredHeight = mainCurrentHeight;
        if (sideListLayout != null) sideListLayout.preferredHeight = sideCurrentHeight;
        RebuildSidebarLayout();

        if (quests.Count > 0) SelectQuest(quests[0]);
    }

    void Update()
    {
        UpdateQuestTimers();
        UpdateDropdownAnimations();
    }

    private void UpdateQuestTimers()
    {
        bool anyQuestFailedThisFrame = false;

        foreach (Quest q in quests)
        {
            if (q.hasTimeLimit && !q.isCompleted && !q.isClaimed && !q.isFailed)
            {
                q.remainingTime -= Time.deltaTime;

                if (q.remainingTime <= 0f)
                {
                    q.remainingTime = 0f;
                    q.isFailed = true;
                    anyQuestFailedThisFrame = true;
                    Debug.Log($"Misi Gagal (Waktu Habis!): {q.questTitle}");
                }
            }
        }

        if (anyQuestFailedThisFrame)
        {
            RefreshQuestLists();
            UpdateDetailUI();
        }
        else if (selectedQuest != null && selectedQuest.hasTimeLimit && !selectedQuest.isCompleted && !selectedQuest.isFailed)
        {
            // Keep countdown ticking live while viewing a timed quest
            UpdateDetailUI();
        }
    }

    private void UpdateDropdownAnimations()
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

    // --- QUEST ACCEPTANCE ---
    public bool HasQuest(Quest quest)
    {
        return quests.Contains(quest);
    }

    public void AcceptQuest(Quest newQuest)
    {
        if (newQuest == null) return;

        // Allow re-accepting if the player previously failed this timed quest
        if (quests.Contains(newQuest) && !newQuest.isFailed) return;

        newQuest.currentAmount = 0;
        newQuest.isCompleted = false;
        newQuest.isClaimed = false;
        newQuest.isFailed = false;
        newQuest.remainingTime = newQuest.hasTimeLimit ? newQuest.timeLimitSeconds : 0f;

        if (!quests.Contains(newQuest))
        {
            quests.Add(newQuest);
        }

        RefreshQuestLists();
        SelectQuest(newQuest);
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
            child.SetParent(null);
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
        if (selectedQuest.questType == QuestType.DeliverToNPC && !selectedQuest.isCompleted && !selectedQuest.isFailed && ResourceManager.Instance != null)
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
            string rewards = $"Hadiah: +{selectedQuest.reputationReward} Rep";
            if (selectedQuest.rockReward > 0) rewards += $"  +{selectedQuest.rockReward} Batu";
            if (selectedQuest.woodReward > 0) rewards += $"  +{selectedQuest.woodReward} Kayu";
            if (selectedQuest.wheatReward > 0) rewards += $"  +{selectedQuest.wheatReward} Gandum";
            questRewardText.text = rewards;
        }

        string timerTag = "";
        if (selectedQuest.hasTimeLimit)
        {
            string formatted = FormatTime(selectedQuest.remainingTime);
            if (questTimerText != null)
            {
                questTimerText.gameObject.SetActive(true);
                questTimerText.text = selectedQuest.isFailed ? "00:00" : formatted;
            }
            else if (!selectedQuest.isCompleted && !selectedQuest.isFailed)
            {
                timerTag = $"  /  Waktu: {formatted}";
            }
        }
        else if (questTimerText != null)
        {
            questTimerText.gameObject.SetActive(false);
        }

        if (questStatusText != null) questStatusText.color = defaultStatusColor;

        if (selectedQuest.isFailed)
        {
            if (questStatusText != null)
            {
                questStatusText.text = "GAGAL (WAKTU HABIS!)";
                questStatusText.color = Color.red;
            }
            if (actionButton != null) actionButton.interactable = false;
            if (actionButtonText != null) actionButtonText.text = "GAGAL";
        }
        else if (selectedQuest.isClaimed)
        {
            if (questStatusText != null) questStatusText.text = "SELESAI";
            if (actionButton != null) actionButton.interactable = false;
            if (actionButtonText != null) actionButtonText.text = "Done";
        }
        else if (selectedQuest.isCompleted)
        {
            if (questStatusText != null) questStatusText.text = "Siap Untuk diklaim";
            if (actionButton != null) actionButton.interactable = true;
            if (actionButtonText != null) actionButtonText.text = "Oke";
        }
        else if (selectedQuest.questType == QuestType.DeliverToNPC)
        {
            bool canDeliver = ResourceManager.Instance != null && ResourceManager.Instance.HasResource(selectedQuest.targetResource, selectedQuest.targetAmount);
            if (questStatusText != null)
                questStatusText.text = (canDeliver ? "Siap Dikirim" : $"Butuh {selectedQuest.targetAmount} {selectedQuest.targetResource}") + timerTag;
            if (actionButton != null) actionButton.interactable = canDeliver;
            if (actionButtonText != null) actionButtonText.text = "Berikan";
        }
        else
        {
            if (questStatusText != null) questStatusText.text = "Dikerjakan" + timerTag;
            if (actionButton != null) actionButton.interactable = false;
            if (actionButtonText != null) actionButtonText.text = "Oke";
        }
    }

    public static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, seconds));
        int mins = totalSeconds / 60;
        int secs = totalSeconds % 60;
        return $"{mins:00}:{secs:00}";
    }

    // --- BUTTON ACTION ("Oke" / "Give" / "Done") ---
    public void OnActionButtonClicked()
    {
        if (selectedQuest == null || selectedQuest.isClaimed || selectedQuest.isFailed) return;

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
        if (quest == null || quest.isFailed || ResourceManager.Instance == null) return false;

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
            if (!q.isCompleted && !q.isFailed && q.questType == QuestType.CollectResource && q.targetResource == resource)
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
            if (!q.isCompleted && !q.isFailed && q.questType == QuestType.BuildBuilding &&
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