using System.Collections.Generic;
using UnityEngine;

public class RandomPattern : MonoBehaviour
{
    public enum CharacterPattern
    {
        Requesting = 0,
        Showing = 1,
        Waiting = 2,
        Checking = 3,
        Adding = 4,
    }

    private List<char> allCharacter = new List<char>();
    private List<char> pattern = new List<char>();
    private float currentTime = 0f;
    private uint index = 0;
    private bool isPlaying = true;
    [SerializeField] private float renderingRate = 0.5f;
    [SerializeField] private uint minimum = 2;
    [SerializeField] private uint step = 3;
    private uint current = 0;
    public event System.Action<char> OnCharacterUsed;
    private CharacterPattern state = CharacterPattern.Requesting;
    private List<char> check = new List<char>();
    [SerializeField] private QuestMemorizingManager questMemorizingManager;

    private void Awake()
    {
        questMemorizingManager.SetMaxScore(step);
    }

    private void OnEnable()
    {
        current = minimum;
        pattern.Clear();
        RandomNewPattern(current);
    }

    public void ChangeStateToShow()
    {
        state = CharacterPattern.Showing;
        index = 0;
        isPlaying = true;
    }

    public void AddCharacter(char character)
    {
        allCharacter.Add(character);
    }

    public void RandomNewPattern(uint round)
    {
        for (int i = 0; i < round; i++)
        {
            int index = Random.Range(0, allCharacter.Count);
            char ch = allCharacter[index];
            pattern.Add(ch);
        }
        state = CharacterPattern.Requesting;
        check.Clear();
    }

    private void Update()
    {
        switch (state)
        {
            case CharacterPattern.Showing:
                ShowPattern();
                break;
            case CharacterPattern.Checking:
                CheckPattern();
                break;
            case CharacterPattern.Adding:
                RandomNewPattern(1);
                current++;
                break;
            default:
                break;
        }
    }

    private void ShowPattern()
    {
        if (currentTime > renderingRate)
        {
            currentTime = 0f;
            if(isPlaying)
            {
                OnCharacterUsed?.Invoke('\0');
                isPlaying = false;
                if(index >= pattern.Count)
                {
                    check.Clear();
                    state = CharacterPattern.Waiting;
                }
            }
            else
            {
                OnCharacterUsed?.Invoke(pattern[(int)index]);
                isPlaying = true;
                index++;
            }
        }
        else
        {
            currentTime += Time.deltaTime;
        }
    }

    public void AddCheck(char ch)
    {
        if(state == CharacterPattern.Waiting)
        {
            check.Add(ch);
            if(check.Count == pattern.Count)
            {
                state = CharacterPattern.Checking;
            }
        }
    }

    private void CheckPattern()
    {
        for (int i = 0; i < pattern.Count; i++)
        {
            if (check[i] != pattern[i])
            {
                OnCharacterUsed?.Invoke('-');
                state = CharacterPattern.Requesting;
                return;
            }
        }
        if(current < minimum + step)
        {
            OnCharacterUsed?.Invoke('+');
            questMemorizingManager.AddScore(1);
            state = CharacterPattern.Adding;
        }
    }

    public bool CanSent()
    {
        if(state != CharacterPattern.Waiting)
        {
            return false;
        }
        return true;
    }
}
