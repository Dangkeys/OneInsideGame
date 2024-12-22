using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class QuestSystem : NetworkBehaviour
{

    private Scrollbar ScrollBar;
    [SerializeField] private Quest[] Quests;
    private NetworkVariable<int> QuestFinished = new NetworkVariable<int>(0);
    private int AllQuest;

    private void Awake()
    {
        ScrollBar = GetComponent<Scrollbar>();
        AllQuest = Quests.Length;
        UpdateProgressBar();
    }

    private void OnEnable()
    {
        foreach (Quest Quest in Quests)
        {
            if (Quest != null)
            {
                if(Quest.GetQuestInfo() == null)
                {
                    Quest.SetQuestInfo(GameObject.Find(Quest.GetGameObjectName()).GetComponent<QuestInfo>());
                }
                Quest.StatusChanged += HandleQuestStatusChanged;
            }
        }
        QuestFinished.OnValueChanged += HandleQuestFinishedChanged;

    }

    private void HandleQuestFinishedChanged(int oldValue, int newValue)
    {
        UpdateProgressBar();
    }

    private void OnDisable()
    {
        foreach (Quest Quest in Quests)
        {
            if (Quest != null)
            {
                Quest.StatusChanged += HandleQuestStatusChanged;
            }
        }
        QuestFinished.OnValueChanged -= HandleQuestFinishedChanged;
    }

    private void HandleQuestStatusChanged(bool Status)
    {
        if (IsServer)
        {
            UpdateQuestFinished(Status);
        }
    }

    private void UpdateQuestFinished(bool Status)
    {
        if (Status)
        {
            QuestFinished.Value += 1;
        }
        else
        {
            QuestFinished.Value -= 1;
        }
    }

    private void UpdateProgressBar()
    {
        if (ScrollBar != null)
        {
            ScrollBar.size = (float)QuestFinished.Value / AllQuest;
        }
        else
        {
            Debug.LogWarning("ScrollBar is not assigned.");
        }
    }
}
