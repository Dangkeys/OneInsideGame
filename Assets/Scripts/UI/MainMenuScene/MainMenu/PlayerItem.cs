using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerItem : MonoBehaviour
{
    [field: SerializeField] public TextMeshProUGUI PlayerNameText { get; private set; }

    public void Initialize(string authId)
    {
        PlayerNameText.text = authId;
    }

}
