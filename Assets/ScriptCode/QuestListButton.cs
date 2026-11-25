using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestListButton : MonoBehaviour
{
    public Button button;
    public Image backgroundImage;
    public TextMeshProUGUI titleText;

    [Header("Colors")]
    public Color activeBgColor = Color.white;
    public Color completedBgColor = new Color(0.65f, 0.65f, 0.65f, 0.85f);
    public Color failedBgColor = new Color(0.85f, 0.55f, 0.55f, 0.85f);
    public Color activeTextColor = new Color(0.2f, 0.15f, 0.1f, 1f);
    public Color completedTextColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    public Color failedTextColor = new Color(0.6f, 0.1f, 0.1f, 1f);

    private Quest assignedQuest;

    public void Setup(Quest quest)
    {
        assignedQuest = quest;
        RefreshVisuals();

        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => QuestManager.Instance.SelectQuest(assignedQuest));
        }

        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }

    void Update()
    {
        if (assignedQuest != null && assignedQuest.hasTimeLimit && !assignedQuest.isCompleted && !assignedQuest.isFailed)
        {
            RefreshVisuals();
        }
    }

    private void RefreshVisuals()
    {
        if (assignedQuest == null) return;

        if (titleText != null)
        {
            if (assignedQuest.isFailed)
            {
                titleText.text = $"{assignedQuest.questTitle} (GAGAL)";
                titleText.color = failedTextColor;
            }
            else if (assignedQuest.isClaimed)
            {
                titleText.text = $"{assignedQuest.questTitle} (SELESAI)";
                titleText.color = completedTextColor;
            }
            else if (assignedQuest.hasTimeLimit && !assignedQuest.isCompleted)
            {
                titleText.text = $"{assignedQuest.questTitle} [{QuestManager.FormatTime(assignedQuest.remainingTime)}]";
                titleText.color = activeTextColor;
            }
            else
            {
                titleText.text = assignedQuest.questTitle;
                titleText.color = activeTextColor;
            }
        }

        if (backgroundImage != null)
        {
            if (assignedQuest.isFailed) backgroundImage.color = failedBgColor;
            else if (assignedQuest.isClaimed) backgroundImage.color = completedBgColor;
            else backgroundImage.color = activeBgColor;
        }
    }
}