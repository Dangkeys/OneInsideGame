using UnityEngine;

[CreateAssetMenu(fileName = "NewWord", menuName = "ScriptableObjects/EmailWord")]
public class EmailWord : ScriptableObject
{
    [SerializeField] private string word;

    public string GetWord()
    {
        return word;
    }

    public int GetLength()
    {
        return word.Length;
    }
}
