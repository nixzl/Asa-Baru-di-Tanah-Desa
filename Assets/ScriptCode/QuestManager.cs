using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum QuestCategory { MisiUtama, MisiSampingan }
public enum QuestType { CollectResource, BuildBuilding, DeliverToNPC }

[System.Serializable]
public class QuestObjective
{
    public QuestType objectiveType = QuestType.CollectResource;
    public ResourceCategory targetResource = ResourceCategory.Wood;
    public string targetBuildingName = "House";
    public int targetAmount = 10;
    [HideInInspector] public int currentAmount = 0;

    public bool IsObjectiveComplete()
    {
        if (objectiveType == QuestType.DeliverToNPC && ResourceManager.Instance != null)
        {
            return ResourceManager.Instance.HasResource(targetResource, targetAmount);
        }
        return currentAmount >= targetAmount;
    }

    public int GetDisplayAmount(bool questLockedState)
    {
        if (objectiveType == QuestType.DeliverToNPC && !questLockedState && ResourceManager.Instance != null)
        {
            switch (targetResource)
            {
                case ResourceCategory.Stone: return Mathf.Min(ResourceManager.Instance.currentRock, targetAmount);
                case ResourceCategory.Wood:  return Mathf.Min(ResourceManager.Instance.currentWood, targetAmount);
                case ResourceCategory.Wheat: return Mathf.Min(ResourceManager.Instance.currentWheat, targetAmount);
            }
        }
        return Mathf.Min(currentAmount, targetAmount);
    }

    public string GetProgressLine(bool questLockedState)
    {
        int display = GetDisplayAmount(questLockedState);
        switch (objectiveType)
        {
            case QuestType.CollectResource:
                return $"• Gather {targetResource}: {display}/{targetAmount}";
            case QuestType.BuildBuilding:
                return $"• Build {targetBuildingName}: {display}/{targetAmount}";
            case QuestType.DeliverToNPC:
                return $"• Deliver {targetResource}: {display}/{targetAmount}";
            default:
                return $"• Progress: {display}/{targetAmount}";
        }
    }
}

[System.Serializable]
public class Quest
{
    public string questTitle = "Gather Supplies";
    public QuestCategory category = QuestCategory.MisiUtama;

    [TextArea] public string description = "Gather Wood, Stone, and Wheat for the village.";

    [Header("Prerequisite (Optional)")]
    [Tooltip("Exact Quest Title of another mission that must be completed before this one can start or progress.")]
    public string prerequisiteQuestTitle = "";

    [Header("Objectives (Add 1 or Multiple Requests)")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

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

    [Header("Side Mission Despawn")]
    public bool despawnNPCWithTransition = false;

    [HideInInspector] public bool isCompleted = false;
    [HideInInspector] public bool isClaimed = false;
    [HideInInspector] public bool isViewed = false;
    [HideInInspector] public QuestNPC ownerNPC;

    public bool AreAllObjectivesComplete()
    {
        if (objectives == null || objectives.Count == 0) return false;

        foreach (QuestObjective obj in objectives)
        {
            if (!obj.IsObjectiveComplete()) return false;
        }
        return true;
    }

    public bool HasDeliveryObjective()
    {
        if (objectives == null) return false;

        foreach (QuestObjective obj in objectives)
        {
            if (obj.objectiveType == QuestType.DeliverToNPC) return true;
        }
        return false;
    }
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("All Quests")]
    public List<Quest> quests = new List<Quest>();
    private Quest selectedQuest;

    [Header("HUD Notification Badge")]
    public GameObject missionNotificationBadge;
    public GameObject missionPanelObject;

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
    public TextMeshProUGUI questTimerText;
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
        UpdateNotificationBadge();
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
                if (!IsPrerequisiteMet(q)) continue;

                q.remainingTime -= Time.deltaTime;

                if (q.remainingTime <= 0f)
                {
                    q.remainingTime = 0f;
                    q.isFailed = true;
                    anyQuestFailedThisFrame = true;
                }
            }
        }

        if (anyQuestFailedThisFrame)
        {
            RefreshQuestLists();
            UpdateDetailUI();
            UpdateNotificationBadge();
        }
        else if (selectedQuest != null && selectedQuest.hasTimeLimit && !selectedQuest.isCompleted && !selectedQuest.isFailed)
        {
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

    // --- PREREQUISITE CHECK ---
    public bool IsPrerequisiteMet(Quest quest)
    {
        if (quest == null || string.IsNullOrWhiteSpace(quest.prerequisiteQuestTitle))
            return true;

        string targetTitle = quest.prerequisiteQuestTitle.Trim();

        foreach (Quest q in quests)
        {
            if (string.Equals(q.questTitle.Trim(), targetTitle, System.StringComparison.OrdinalIgnoreCase))
            {
                return q.isCompleted || q.isClaimed;
            }
        }

        return false;
    }

    // --- NOTIFICATION BADGE ("!") ---
    public void UpdateNotificationBadge()
    {
        if (missionNotificationBadge == null) return;

        bool hasAlert = false;
        foreach (Quest q in quests)
        {
            if (q.isFailed) continue;

            if (!q.isViewed || (q.isCompleted && !q.isClaimed))
            {
                hasAlert = true;
                break;
            }
        }

        missionNotificationBadge.SetActive(hasAlert);
    }

    public void OnMissionPanelOpened()
    {
        foreach (Quest q in quests)
        {
            q.isViewed = true;
        }

        UpdateNotificationBadge();
        UpdateDetailUI();
    }

    // --- QUEST ACCEPTANCE ---
    public bool HasQuest(Quest quest)
    {
        return quests.Contains(quest);
    }

    public void AcceptQuest(Quest newQuest)
    {
        if (newQuest == null) return;
        if (!IsPrerequisiteMet(newQuest)) return;
        if (quests.Contains(newQuest) && !newQuest.isFailed) return;

        // Reset progress on all objectives inside this mission
        if (newQuest.objectives != null)
        {
            foreach (QuestObjective obj in newQuest.objectives)
            {
                obj.currentAmount = 0;
            }
        }

        newQuest.isCompleted = false;
        newQuest.isClaimed = false;
        newQuest.isFailed = false;
        newQuest.isViewed = false;
        newQuest.remainingTime = newQuest.hasTimeLimit ? newQuest.timeLimitSeconds : 0f;

        if (!quests.Contains(newQuest))
        {
            quests.Add(newQuest);
        }

        RefreshQuestLists();
        SelectQuest(newQuest, markAsViewed: false);
        UpdateNotificationBadge();
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
    public void SelectQuest(Quest quest, bool markAsViewed = true)
    {
        selectedQuest = quest;
        if (markAsViewed && selectedQuest != null)
        {
            selectedQuest.isViewed = true;
            UpdateNotificationBadge();
        }
        UpdateDetailUI();
    }

    public void UpdateDetailUI()
    {
        if (selectedQuest == null) return;

        bool prereqMet = IsPrerequisiteMet(selectedQuest);
        bool isLockedState = selectedQuest.isCompleted || selectedQuest.isFailed;

        if (questTitleText != null) questTitleText.text = selectedQuest.questTitle;
        if (questDescriptionText != null) questDescriptionText.text = selectedQuest.description;

        // Format progress for either a single objective or multiple simultaneous objectives
        if (questProgressText != null)
        {
            if (selectedQuest.objectives == null || selectedQuest.objectives.Count == 0)
            {
                questProgressText.text = "0/0";
            }
            else if (selectedQuest.objectives.Count == 1)
            {
                QuestObjective single = selectedQuest.objectives[0];
                int display = single.GetDisplayAmount(isLockedState);
                questProgressText.text = $"{display}/{single.targetAmount}";
            }
            else
            {
                List<string> lines = new List<string>();
                foreach (QuestObjective obj in selectedQuest.objectives)
                {
                    lines.Add(obj.GetProgressLine(isLockedState));
                }
                questProgressText.text = string.Join("\n", lines);
            }
        }

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
                timerTag = $"  |  Waktu: {formatted}";
            }
        }
        else if (questTimerText != null)
        {
            questTimerText.gameObject.SetActive(false);
        }

        if (questStatusText != null) questStatusText.color = defaultStatusColor;

        if (!prereqMet)
        {
            if (questStatusText != null)
            {
                questStatusText.text = $"Dibutuhkan: {selectedQuest.prerequisiteQuestTitle}";
                questStatusText.color = Color.red;
            }
            if (actionButton != null) actionButton.interactable = false;
            if (actionButtonText != null) actionButtonText.text = "Terkunci";
        }
        else if (selectedQuest.isFailed)
        {
            if (questStatusText != null)
            {
                questStatusText.text = "Gagal (Waktu Habis)";
                questStatusText.color = Color.red;
            }
            if (actionButton != null) actionButton.interactable = false;
            if (actionButtonText != null) actionButtonText.text = "Gagal";
        }
        else if (selectedQuest.isClaimed)
        {
            if (questStatusText != null) questStatusText.text = "Selesai";
            if (actionButton != null) actionButton.interactable = false;
            if (actionButtonText != null) actionButtonText.text = "Done";
        }
        else if (selectedQuest.isCompleted)
        {
            if (questStatusText != null) questStatusText.text = "Siap Diklaim";
            if (actionButton != null) actionButton.interactable = true;
            if (actionButtonText != null) actionButtonText.text = "Oke";
        }
        else if (selectedQuest.HasDeliveryObjective())
        {
            bool canDeliver = CanDeliverAllObjectives(selectedQuest);
            if (questStatusText != null)
                questStatusText.text = (canDeliver ? "Siap Dikirim" : "Kumpulkan semua bahan") + timerTag;
            if (actionButton != null) actionButton.interactable = canDeliver;
            if (actionButtonText != null) actionButtonText.text = "Give";
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
        if (selectedQuest == null || selectedQuest.isClaimed || selectedQuest.isFailed || !IsPrerequisiteMet(selectedQuest)) return;

        if (selectedQuest.HasDeliveryObjective() && !selectedQuest.isCompleted)
        {
            if (!TryDeliverQuestResources(selectedQuest)) return;
        }

        if (selectedQuest.isCompleted && !selectedQuest.isClaimed)
        {
            ClaimQuestRewards(selectedQuest);
        }
    }

    public void ClaimQuestRewards(Quest quest)
    {
        if (quest == null || quest.isClaimed) return;

        if (ReputationManager.Instance != null)
            ReputationManager.Instance.AddReputation(quest.reputationReward);

        if (ResourceManager.Instance != null)
        {
            if (quest.rockReward > 0) ResourceManager.Instance.AddRock(quest.rockReward);
            if (quest.woodReward > 0) ResourceManager.Instance.AddWood(quest.woodReward);
            if (quest.wheatReward > 0) ResourceManager.Instance.AddWheat(quest.wheatReward);
        }

        quest.isClaimed = true;
        RefreshQuestLists();
        UpdateDetailUI();
        UpdateNotificationBadge();

        if (quest.despawnNPCWithTransition && quest.ownerNPC != null && quest.ownerNPC.gameObject.activeInHierarchy)
        {
            QuestNPC npcToDespawn = quest.ownerNPC;

            GameObject panelToClose = missionPanelObject;
            if (panelToClose == null && actionButton != null)
            {
                Canvas rootCanvas = actionButton.GetComponentInParent<Canvas>();
                if (rootCanvas != null)
                {
                    rootCanvas = rootCanvas.rootCanvas;
                    Transform current = actionButton.transform;
                    while (current.parent != null && current.parent != rootCanvas.transform)
                    {
                        current = current.parent;
                    }
                    if (current.parent == rootCanvas.transform)
                    {
                        panelToClose = current.gameObject;
                    }
                }
            }

            if (panelToClose != null && panelToClose.activeSelf)
            {
                panelToClose.SetActive(false);
                Time.timeScale = 1f;
            }

            SceneTransitionManager.Instance.PlayTransition(() =>
            {
                if (npcToDespawn != null)
                {
                    npcToDespawn.gameObject.SetActive(false);
                }
            });
        }
    }

    public bool CanDeliverAllObjectives(Quest quest)
    {
        if (quest == null || quest.objectives == null || ResourceManager.Instance == null) return false;

        foreach (QuestObjective obj in quest.objectives)
        {
            if (!obj.IsObjectiveComplete()) return false;
        }
        return true;
    }

    public bool TryDeliverQuestResources(Quest quest)
    {
        if (quest == null || quest.isFailed || !IsPrerequisiteMet(quest) || !CanDeliverAllObjectives(quest))
            return false;

        // Deduct all DeliverToNPC resources together
        foreach (QuestObjective obj in quest.objectives)
        {
            if (obj.objectiveType == QuestType.DeliverToNPC)
            {
                ResourceManager.Instance.SpendSingleResource(obj.targetResource, obj.targetAmount);
                obj.currentAmount = obj.targetAmount;
            }
        }

        if (quest.AreAllObjectivesComplete())
        {
            quest.isCompleted = true;
        }

        RefreshQuestLists();
        UpdateDetailUI();
        UpdateNotificationBadge();
        return true;
    }

    // --- PROGRESS HOOKS FOR GAMEPLAY EVENTS ---
    public void AddResourceCollected(ResourceCategory resource, int amount)
    {
        bool changed = false;

        foreach (Quest q in quests)
        {
            if (q.isCompleted || q.isFailed || !IsPrerequisiteMet(q) || q.objectives == null) continue;

            foreach (QuestObjective obj in q.objectives)
            {
                if (obj.objectiveType == QuestType.CollectResource &&
                    obj.targetResource == resource &&
                    obj.currentAmount < obj.targetAmount)
                {
                    obj.currentAmount = Mathf.Min(obj.currentAmount + amount, obj.targetAmount);
                    changed = true;
                }
            }

            if (changed && q.AreAllObjectivesComplete())
            {
                q.isCompleted = true;
            }
        }

        if (changed)
        {
            RefreshQuestLists();
            UpdateDetailUI();
            UpdateNotificationBadge();
        }
    }

    public void OnBuildingConstructed(string buildingName)
    {
        bool changed = false;

        foreach (Quest q in quests)
        {
            if (q.isCompleted || q.isFailed || !IsPrerequisiteMet(q) || q.objectives == null) continue;

            foreach (QuestObjective obj in q.objectives)
            {
                if (obj.objectiveType == QuestType.BuildBuilding &&
                    string.Equals(obj.targetBuildingName, buildingName, System.StringComparison.OrdinalIgnoreCase) &&
                    obj.currentAmount < obj.targetAmount)
                {
                    obj.currentAmount = Mathf.Min(obj.currentAmount + 1, obj.targetAmount);
                    changed = true;
                }
            }

            if (changed && q.AreAllObjectivesComplete())
            {
                q.isCompleted = true;
            }
        }

        if (changed)
        {
            RefreshQuestLists();
            UpdateDetailUI();
            UpdateNotificationBadge();
        }
    }
}