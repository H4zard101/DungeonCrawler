using UnityEngine;

public class TeleportDoor : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.Space;

    [Header("Teleport")]
    public Transform targetSpawnPoint;

    [Header("Objective Gating (optional)")]
    [Tooltip("If true, door is only usable when CurrentStage is between min and max.")]
    public bool requireSpecificStage = false;
    public ObjectiveStage minAllowedStage = ObjectiveStage.DefeatEnemies;
    public ObjectiveStage maxAllowedStage = ObjectiveStage.ReturnToBlacksmith;

    [Header("Objective Change (optional)")]
    [Tooltip("If true, using this door will change the current objective stage.")]
    public bool setStageOnUse = false;
    public ObjectiveStage nextStage;

    private bool playerInRange;
    private Transform player;
    private Rigidbody2D playerRb;
    private PlayerMovement playerMovement;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerRb = p.GetComponent<Rigidbody2D>();
            playerMovement = p.GetComponent<PlayerMovement>();
        }
    }

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
        if (!playerInRange || player == null)
            return;

        // Don't use doors while dialogue is open
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsShowingDialogue)
            return;

        // Optional objective gating: only allow if CurrentStage is between min and max (inclusive)
        if (requireSpecificStage && ObjectiveManager.Instance != null)
        {
            var s = ObjectiveManager.Instance.CurrentStage;
            if (s < minAllowedStage || s > maxAllowedStage)
                return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            UseDoor();
        }
    }

    private void UseDoor()
    {
        if (targetSpawnPoint == null)
        {
            Debug.LogWarning($"TeleportDoor on {name} has no targetSpawnPoint assigned.");
            return;
        }

        // Briefly lock movement to avoid sliding during teleport
        if (playerMovement != null)
            playerMovement.SetCanMove(false);

        if (playerRb != null)
            playerRb.velocity = Vector2.zero;

        player.position = targetSpawnPoint.position;

        if (playerMovement != null)
            playerMovement.SetCanMove(true);

        // Optional: advance objective
        if (setStageOnUse && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetStage(nextStage);
        }
    }
}