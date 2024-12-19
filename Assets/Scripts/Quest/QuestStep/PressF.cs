using UnityEngine;

public class PressF : QuestInfo
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Select F");
            FinishQuest();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Select E");
            BrokenQuest();
        }
    }
}
