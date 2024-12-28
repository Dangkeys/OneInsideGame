using UnityEngine;

public class PressK : QuestInfo
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            FinishQuest();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            BrokenQuest();
        }
    }
}
