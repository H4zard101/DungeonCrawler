using System.Collections;
using UnityEngine;
using TMPro;

public class EndGameCutscene : MonoBehaviour
{
    public static EndGameCutscene Instance;

    [Header("Fade UI")]
    public CanvasGroup fadeGroup;      // Assign EndGamePanel CanvasGroup
    public TMP_Text endText;           // Text child of EndGamePanel

    [Header("Settings")]
    public float fadeDuration = 1.5f;
    public float textDelay = 0.5f;
    public string finalMessage = "THANKS FOR PLAYING";

    private bool playing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(false);
            fadeGroup.alpha = 0f;
        }
    }

    public void PlayCutscene()
    {
        if (playing || fadeGroup == null)
            return;

        StartCoroutine(CutsceneRoutine());
    }

    private IEnumerator CutsceneRoutine()
    {
        playing = true;

        // Disable player controls
        if (DialogueManager.Instance != null)
        {
            if (DialogueManager.Instance.playerMovement != null)
                DialogueManager.Instance.playerMovement.SetCanMove(false);

            if (DialogueManager.Instance.playerCombat != null)
                DialogueManager.Instance.playerCombat.canAttack = false;
        }

        fadeGroup.gameObject.SetActive(true);
        fadeGroup.alpha = 0f;

        // Fade to black
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        // Small delay before text
        yield return new WaitForSeconds(textDelay);

        if (endText != null)
            endText.text = finalMessage;
    }
}
