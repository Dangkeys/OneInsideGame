using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "ScriptableObjects/Quest")]
public class Quest : ScriptableObject
{
    [SerializeField] private string Name;
    [SerializeField] private string Description;
    [SerializeField] private string GameObjectName;
    private QuestInfo QuestInfo = null;
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

    public QuestInfo GetQuestInfo()
    {
        return QuestInfo;
    }

    public string GetGameObjectName()
    {
        return GameObjectName;
    }

    public void SetQuestInfo(QuestInfo NewQuestInfo)
    {
        QuestInfo = NewQuestInfo;
        QuestInfo.OnQuestStatusChanged += HandleQuestStatusChanged;
    }

    private void HandleQuestStatusChanged(bool Status)
    {
        QuestStatus = Status;
        StatusChanged?.Invoke(Status);
    }
}
