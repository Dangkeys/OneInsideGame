using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "ScriptableObjects/Quest")]
public class Quest : ScriptableObject
{
    [SerializeField] private string Name;
    [SerializeField] private string Description;
    [SerializeField] private string GameObjectName;
    private QuestInfo QuestInfo;
    public event System.Action<bool> StatusChanged;

    private void OnEnable()
    {
        if (QuestInfo != null)
        {
            QuestInfo.OnQuestStatusChanged += HandleQuestStatusChanged;
        }
    }

    private void OnDisable()
    {
        if (QuestInfo != null)
        {
            QuestInfo.OnQuestStatusChanged -= HandleQuestStatusChanged;
        }
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
        StatusChanged?.Invoke(Status);
    }
}
