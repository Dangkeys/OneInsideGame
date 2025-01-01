using Unity.Netcode;
using UnityEngine;

public class PressK : QuestInfo
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Finished(true);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            BreakQuest();
        }
    }

    private void Finished(bool finished)
    {
        FinishQuest();
        CancelQuest();
    }
    public override void CancelQuest()
    {
        UpdateDoQuest(false);
    }
}
