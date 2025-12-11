using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image portraitImage;
    public GameObject spacePrompt;

    [Header("Typing")]
    public float charactersPerSecond = 40f;

    [Header("Player")]
    public PlayerMovement playerMovement;
    public PlayerCombat playerCombat;

    private Queue<string> lines = new Queue<string>();

    private bool isShowing;
    private bool isTyping;
    private string currentLine;
    private float typeTimer;
    private int typeIndex;


    private System.Action onFinished;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        if(dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if(!isShowing)
        {
            return;
        }

        HandleTyping();
        HandleInput();
    }
    private void HandleTyping()
    {
        if(!isTyping)
        {
            return;
        }
        typeTimer += Time.deltaTime * charactersPerSecond;
        int newIndex = Mathf.FloorToInt(typeTimer);
        if(newIndex != typeIndex)
        {
            typeIndex = Mathf.Clamp(newIndex, 0, currentLine.Length);
            dialogueText.text = currentLine.Substring(0, typeIndex);
        }
        if(typeIndex >= currentLine.Length)
        {
            isTyping = false;
            dialogueText.text = currentLine;

            if (spacePrompt != null)
            {
                spacePrompt.SetActive(true);
            }
        }
    }

    private void HandleInput()
    {
        if(!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }
        if(isTyping)
        {
            isTyping = false;
            dialogueText.text = currentLine;
            typeIndex = currentLine.Length;

            if(spacePrompt != null)
            {
                spacePrompt.SetActive(true);
            }
        }
        else
        {
            ShowNextLine();
        }
    }
    public void StartDialogue(
       string speakerName,
       IEnumerable<string> dialogueLines,
       Sprite portraitSprite = null,
       System.Action onFinishedCallback = null)
    {
        lines.Clear();
        foreach (var line in dialogueLines)
            lines.Enqueue(line);

        if (nameText != null)
            nameText.text = speakerName;

        if (portraitImage != null && portraitSprite != null)
            portraitImage.sprite = portraitSprite;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (spacePrompt != null)
            spacePrompt.SetActive(false);

        isShowing = true;

        if (playerMovement != null)
            playerMovement.SetCanMove(false);

        if (playerCombat != null)
            playerCombat.canAttack = false;

        onFinished = onFinishedCallback;

        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if(lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = lines.Dequeue();

        typeIndex = 0;
        typeTimer = 0f;
        isTyping = true;

        if(dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }
        if(spacePrompt != null)
        {
            spacePrompt.SetActive(false);
        }
    }

    public void EndDialogue()
    {
        isShowing = false;
        isTyping = false;
        currentLine = null;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (spacePrompt != null)
            spacePrompt.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetCanMove(true);

        if (playerCombat != null)
            playerCombat.canAttack = true;

        // Call back to whoever started this dialogue
        if (onFinished != null)
        {
            var cb = onFinished;
            onFinished = null;
            cb();
        }
    }

    public bool IsShowingDialogue => isShowing;
}
