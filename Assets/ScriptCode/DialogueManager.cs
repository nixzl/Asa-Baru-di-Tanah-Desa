using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Dialogue UI References")]
    public GameObject dialoguePanel;
    public Image npcPortraitImage; 
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueBodyText;

    [Header("Buttons")]
    public Button nextOrAcceptButton;
    public TextMeshProUGUI nextOrAcceptButtonText;
    public Button declineButton;

    [Header("Button Labels")]
    public string nextLabel = "Lanjut";
    public string acceptLabel = "Terima";
    public string declineLabel = "Tolak";
    public string closeLabel = "Tutup";

    private Queue<string> sentences = new Queue<string>();
    private Quest pendingQuestOffer;
    private Action onDialogueFinished;
    private bool isOfferingQuest = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (declineButton != null)
        {
            TextMeshProUGUI declineTxt = declineButton.GetComponentInChildren<TextMeshProUGUI>();
            if (declineTxt != null) declineTxt.text = declineLabel;
        }
    }

    public void StartDialogue(string npcName, string[] lines, Sprite npcPortrait = null, Quest questToOffer = null, Action onComplete = null)
    {
        if (dialoguePanel == null) return;

        pendingQuestOffer = questToOffer;
        onDialogueFinished = onComplete;
        isOfferingQuest = (questToOffer != null);

        if (npcNameText != null) npcNameText.text = npcName;

        // Show or hide the NPC portrait image
        if (npcPortraitImage != null)
        {
            if (npcPortrait != null)
            {
                npcPortraitImage.sprite = npcPortrait;
                npcPortraitImage.gameObject.SetActive(true);
            }
            else
            {
                npcPortraitImage.gameObject.SetActive(false);
            }
        }

        sentences.Clear();
        if (lines != null && lines.Length > 0)
        {
            foreach (string line in lines)
            {
                sentences.Enqueue(line);
            }
        }
        else
        {
            sentences.Enqueue("Halo! Bisakah kamu membantuku?");
        }

        dialoguePanel.SetActive(true);
        Time.timeScale = 0f;

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            if (isOfferingQuest && pendingQuestOffer != null && QuestManager.Instance != null)
            {
                QuestManager.Instance.AcceptQuest(pendingQuestOffer);
            }

            onDialogueFinished?.Invoke();
            EndDialogue();
            return;
        }

        string currentLine = sentences.Dequeue();
        if (dialogueBodyText != null) dialogueBodyText.text = currentLine;

        bool isLastLine = (sentences.Count == 0);

        if (isLastLine && isOfferingQuest)
        {
            if (nextOrAcceptButtonText != null) nextOrAcceptButtonText.text = acceptLabel;
            if (declineButton != null) declineButton.gameObject.SetActive(true);
        }
        else if (isLastLine)
        {
            if (nextOrAcceptButtonText != null) nextOrAcceptButtonText.text = closeLabel;
            if (declineButton != null) declineButton.gameObject.SetActive(false);
        }
        else
        {
            if (nextOrAcceptButtonText != null) nextOrAcceptButtonText.text = nextLabel;
            if (declineButton != null) declineButton.gameObject.SetActive(false);
        }
    }

    public void DeclineQuestAndClose()
    {
        EndDialogue();
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        pendingQuestOffer = null;
        onDialogueFinished = null;
        isOfferingQuest = false;
        Time.timeScale = 1f;
    }
}