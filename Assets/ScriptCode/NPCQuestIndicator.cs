using UnityEngine;
using TMPro;

[RequireComponent(typeof(QuestNPC))]
public class NPCQuestIndicator : MonoBehaviour
{
    [Header("Position & Animation")]
    public Vector3 headOffset = new Vector3(0f, 2.4f, 0f);
    public float bobHeight = 0.15f;
    public float bobSpeed = 3f;
    public bool pulseWhenReady = true;

    [Header("Appearance (Auto-Generated if Left Empty)")]
    public TextMeshPro indicatorText;
    public float textSize = 7f;
    public Color availableColor = new Color(1f, 0.82f, 0.1f, 1f);   // Bright Gold "!"
    public Color readyTurnInColor = new Color(0.3f, 0.95f, 0.4f, 1f); // Bright Green "?"
    public Color inProgressColor = new Color(0.75f, 0.75f, 0.75f, 0.85f); // Muted Grey "?"

    private QuestNPC questNPC;
    private Transform indicatorTransform;
    private Camera mainCam;

    private enum IndicatorState { Hidden, Available, InProgress, ReadyToDeliver }
    private IndicatorState currentState = IndicatorState.Hidden;

    void Awake()
    {
        questNPC = GetComponent<QuestNPC>();
        mainCam = Camera.main;

        // Automatically create a crisp 3D floating text marker if none is assigned in the Inspector
        if (indicatorText == null)
        {
            GameObject markerObj = new GameObject("QuestIndicatorMarker");
            markerObj.transform.SetParent(transform);
            markerObj.transform.localPosition = headOffset;

            indicatorText = markerObj.AddComponent<TextMeshPro>();
            indicatorText.alignment = TextAlignmentOptions.Center;
            indicatorText.fontSize = textSize;
            indicatorText.fontStyle = FontStyles.Bold;
            indicatorText.textWrappingMode = TextWrappingModes.NoWrap;

            // Add a subtle dark outline so the symbol stays readable against bright terrain
            indicatorText.outlineWidth = 0.25f;
            indicatorText.outlineColor = new Color32(25, 20, 15, 255);
        }

        indicatorTransform = indicatorText.transform;
    }

    void LateUpdate()
    {
        if (questNPC == null || indicatorTransform == null) return;

        UpdateIndicatorState();

        if (currentState == IndicatorState.Hidden) return;

        // 1. Smooth vertical bobbing above the NPC's head
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        indicatorTransform.localPosition = headOffset + new Vector3(0f, yOffset, 0f);

        // 2. Subtle scale pulse when the NPC has a new quest or is ready for delivery
        if (pulseWhenReady && (currentState == IndicatorState.Available || currentState == IndicatorState.ReadyToDeliver))
        {
            float pulse = 1f + Mathf.Sin(Time.time * bobSpeed * 1.5f) * 0.08f;
            indicatorTransform.localScale = Vector3.one * pulse;
        }
        else
        {
            indicatorTransform.localScale = Vector3.one;
        }

        // 3. Billboard toward the active camera so the symbol never faces sideways or backwards
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam != null)
        {
            indicatorTransform.rotation = mainCam.transform.rotation;
        }
    }

    private void UpdateIndicatorState()
    {
        
        Quest mission = questNPC.missionToGive;

        if (mission == null || QuestManager.Instance == null)
        {
            SetState(IndicatorState.Hidden);
            return;
        }

            // Case 1: Not yet accepted OR previously failed -> Show gold "!"
        if (!QuestManager.Instance.HasQuest(mission) || mission.isFailed)
        {
            SetState(IndicatorState.Available);
            return;
        }

        // Case 2: Already finished or claimed -> Hide indicator completely
        if (mission.isClaimed || mission.isCompleted)
        {
            SetState(IndicatorState.Hidden);
            return;
        }

        // Case 3: Delivery quest where the player now holds enough resources -> Show green "?"
        if (mission.questType == QuestType.DeliverToNPC && ResourceManager.Instance != null)
        {
            if (ResourceManager.Instance.HasResource(mission.targetResource, mission.targetAmount))
            {
                SetState(IndicatorState.ReadyToDeliver);
                return;
            }
        }

        // Case 4: Accepted and still working on it -> Show grey "?"
        SetState(IndicatorState.InProgress);
    }

    private void SetState(IndicatorState newState)
    {
        if (currentState == newState && indicatorTransform.gameObject.activeSelf == (newState != IndicatorState.Hidden))
            return;

        currentState = newState;

        if (newState == IndicatorState.Hidden)
        {
            indicatorTransform.gameObject.SetActive(false);
            return;
        }

        indicatorTransform.gameObject.SetActive(true);

        switch (newState)
        {
            case IndicatorState.Available:
                indicatorText.text = "!";
                indicatorText.color = availableColor;
                break;

            case IndicatorState.ReadyToDeliver:
                indicatorText.text = "?";
                indicatorText.color = readyTurnInColor;
                break;

            case IndicatorState.InProgress:
                indicatorText.text = "?";
                indicatorText.color = inProgressColor;
                break;
        }
    }
}