using UnityEngine;
using TMPro;

public enum ObjectiveStage
{
    DefeatEnemies,
    TalkToBlacksmith,
    GoToDoor,
    KillBoss,
    ReturnToBlacksmith
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text objectiveText;

    public ObjectiveStage CurrentStage { get; private set; }

    public int EnemiesAlive => enemiesAlive;
    public int enemiesAlive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetStage(ObjectiveStage.DefeatEnemies);
    }

    public void RegisterEnemy()
    {
        enemiesAlive++;
        Debug.Log($"REGISTER ENEMY → alive now: {enemiesAlive}");
    }

    public void EnemyDied()
    {
        enemiesAlive--;
        if (enemiesAlive < 0) enemiesAlive = 0;

        Debug.Log($"ENEMY DIED → alive now: {enemiesAlive}");

        if (enemiesAlive == 0 && CurrentStage == ObjectiveStage.DefeatEnemies)
        {
            Debug.Log("ALL ENEMIES DEAD → advancing to TalkToBlacksmith");
            SetStage(ObjectiveStage.TalkToBlacksmith);
        }
    }

    public void SetStage(ObjectiveStage newStage)
    {
        CurrentStage = newStage;

        switch (newStage)
        {
            case ObjectiveStage.DefeatEnemies:
                SetObjectiveText("Defeat all the enemies.");
                break;

            case ObjectiveStage.TalkToBlacksmith:
                SetObjectiveText("Talk to the blacksmith. (SPACE)");
                break;

            case ObjectiveStage.GoToDoor:
                SetObjectiveText("Go to the Barracks door.");
                break;

            case ObjectiveStage.KillBoss:
                SetObjectiveText("Defeat the captain in the barracks.");
                break;

            case ObjectiveStage.ReturnToBlacksmith:
                SetObjectiveText("Return to the blacksmith.");
                break;
        }

        Debug.Log($"OBJECTIVE STAGE → {CurrentStage}");
    }

    private void SetObjectiveText(string text)
    {
        if (objectiveText != null)
            objectiveText.text = text;
    }

    public bool CanTalkToBlacksmith()
    {
        return CurrentStage == ObjectiveStage.TalkToBlacksmith;
    }
}