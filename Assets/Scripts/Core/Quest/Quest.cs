using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "ScriptableObjects/Quest")]
public class Quest : ScriptableObject
{
    [SerializeField] private string questName;
    [SerializeField] private string questDescription;
    [SerializeField] private string questObjectName;
    private QuestInfo questInfo;
    public event System.Action<int, bool> questStatus;

    public string GetGameObjectName()
    {
        return questObjectName;
    }

    public void ChangeQuestInfo(QuestInfo newQuestInfo)
    {
        if (questInfo != null)
        {
            questInfo.questInfoStatus -= HandleQuestStatus;
        }

        questInfo = newQuestInfo;

        if (questInfo != null)
        {
            questInfo.questInfoStatus += HandleQuestStatus;
        }
    }

    private void HandleQuestStatus(int index, bool status)
    {
        questStatus?.Invoke(index, status);
    }
}
