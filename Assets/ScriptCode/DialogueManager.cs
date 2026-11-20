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
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueBodyText;

    [Header("Buttons")]
    public Button nextOrAcceptButton;
    public TextMeshProUGUI nextOrAcceptButtonText;
    public Button declineButton;

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
    }

    public void StartDialogue(string npcName, string[] lines, Quest questToOffer = null, Action onComplete = null)
    {
        if (dialoguePanel == null) return;

        pendingQuestOffer = questToOffer;
        onDialogueFinished = onComplete;
        isOfferingQuest = (questToOffer != null);

        if (npcNameText != null) npcNameText.text = npcName;

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
            sentences.Enqueue("Hello! Can you help me with something?");
        }

        dialoguePanel.SetActive(true);
        Time.timeScale = 0f; // Pause movement and world timers while talking

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // If no more sentences remain, handle the button click as Accept or Close
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
            // Final line of a quest offer: show Accept & Decline
            if (nextOrAcceptButtonText != null) nextOrAcceptButtonText.text = "Terima";
            if (declineButton != null) declineButton.gameObject.SetActive(true);
        }
        else if (isLastLine)
        {
            // Final line of normal conversation
            if (nextOrAcceptButtonText != null) nextOrAcceptButtonText.text = "Minggat";
            if (declineButton != null) declineButton.gameObject.SetActive(false);
        }
        else
        {
            // More lines remaining
            if (nextOrAcceptButtonText != null) nextOrAcceptButtonText.text = "Selanjutnya";
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