using UnityEngine;

public class BlacksmithDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public string speakerName = "BLACKSMITH";

    [TextArea(2, 5)]
    public string[] firstDialogueLines;      // initial quest dialogue

    [TextArea(2, 5)]
    public string[] afterBossDialogueLines;  // thank-you / ending dialogue

    public Sprite portraitSprite;

    public KeyCode interactKey = KeyCode.Space;

    [Header("Positioning")]
    [Tooltip("Where the blacksmith should stand AFTER the boss is dead.")]
    public Transform afterBossPosition;

    private bool playerInRange;
    private bool movedAfterBoss;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void Update()
    {
        

        if (!playerInRange)
            return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsShowingDialogue)
            return;

        if (ObjectiveManager.Instance == null)
            return;

        var stage = ObjectiveManager.Instance.CurrentStage;

        // Only talk during the two relevant stages
        bool canTalkNow =
            stage == ObjectiveStage.TalkToBlacksmith ||
            stage == ObjectiveStage.ReturnToBlacksmith;

        if (!canTalkNow)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            StartDialogueForCurrentStage(stage);
        }
    }

    private void StartDialogueForCurrentStage(ObjectiveStage stage)
    {
        if (DialogueManager.Instance == null) return;

        if (stage == ObjectiveStage.TalkToBlacksmith)
        {
            DialogueManager.Instance.StartDialogue(
                speakerName,
                firstDialogueLines,
                portraitSprite,
                OnFirstDialogueFinished);
        }
        else if (stage == ObjectiveStage.ReturnToBlacksmith)
        {
            DialogueManager.Instance.StartDialogue(
                speakerName,
                afterBossDialogueLines,
                portraitSprite,
                OnSecondDialogueFinished);
        }
    }

    // After first conversation → unlock the door
    private void OnFirstDialogueFinished()
    {
        if (ObjectiveManager.Instance != null &&
            ObjectiveManager.Instance.CurrentStage == ObjectiveStage.TalkToBlacksmith)
        {
            ObjectiveManager.Instance.SetStage(ObjectiveStage.GoToDoor);
        }
    }

    // After second conversation → play ending cutscene
    private void OnSecondDialogueFinished()
    {
        // Move blacksmith to the door AFTER the thank-you dialogue
        if (!movedAfterBoss && afterBossPosition != null)
        {
            transform.position = afterBossPosition.position;
            movedAfterBoss = true;
        }

        // Now play ending cutscene
        if (EndGameCutscene.Instance != null)
        {
            EndGameCutscene.Instance.PlayCutscene();
        }
    }
}