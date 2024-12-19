using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "ScriptableObjects/Quest")]
public class Quest : ScriptableObject
{
    public string Id;
    public string Name;
    public QuestInfo QuestInfo;
    public bool QuestStatus { get; private set; } = false;
    public event System.Action<bool> StatusChanged;

    private void OnEnable()
    {
        if (QuestInfo != null)
        {
            QuestInfo.OnQuestStatusChanged += HandleQuestStatusChanged;
        }
        else
        {
            Debug.LogError("QuestInfo is not assigned.");
        }
    }

    private void OnDisable()
    {
        if (QuestInfo != null)
        {
            QuestInfo.OnQuestStatusChanged -= HandleQuestStatusChanged;
        }
        else
        {
            Debug.LogError("QuestInfo is not assigned.");
        }
    }

    private void HandleQuestStatusChanged(bool Status)
    {
        QuestStatus = Status;
        StatusChanged?.Invoke(Status);
    }
}
