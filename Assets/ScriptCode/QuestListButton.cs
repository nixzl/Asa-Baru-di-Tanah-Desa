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
    public Color activeTextColor = new Color(0.2f, 0.15f, 0.1f, 1f);
    public Color completedTextColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    private Quest assignedQuest;

    public void Setup(Quest quest)
    {
        assignedQuest = quest;

        if (titleText != null)
        {
            titleText.text = quest.isClaimed ? $"{quest.questTitle} (Done)" : quest.questTitle;
            titleText.color = quest.isClaimed ? completedTextColor : activeTextColor;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = quest.isClaimed ? completedBgColor : activeBgColor;
        }

        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => QuestManager.Instance.SelectQuest(assignedQuest));
        }

        // Force the button RectTransform to immediately fit the updated text size
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }
}