using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "ScriptableObjects/Quest")]
public class Quest : ScriptableObject
{
    [SerializeField] private string questName;
    [SerializeField] private string questDescription;
    [SerializeField] private string questObjectName;
    private QuestInfo questInfo;
    public event System.Action<bool> questStatus;

    private void OnEnable()
    {
        if (questInfo != null)
        {
            questInfo.OnQuestStatusChanged += HandleQuestStatus;
        }
    }

    private void OnDisable()
    {
        if (questInfo != null)
        {
            questInfo.OnQuestStatusChanged -= HandleQuestStatus;
        }
    }

    public string GetGameObjectName()
    {
        return questObjectName;
    }

    public void SetQuestInfo(QuestInfo newQuestInfo)
    {
        questInfo = newQuestInfo;
        questInfo.OnQuestStatusChanged += HandleQuestStatus;
    }

    private void HandleQuestStatus(bool status)
    {
        questStatus?.Invoke(status);
    }
}
