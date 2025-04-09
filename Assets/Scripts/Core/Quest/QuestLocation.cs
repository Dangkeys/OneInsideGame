using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestLocation", menuName = "ScriptableObjects/QuestLocation")]
public class QuestLocation : ScriptableObject
{
    [SerializeField] private Vector3 location;

    public Vector3 GetLocation()
    {
        return location;
    }
}
