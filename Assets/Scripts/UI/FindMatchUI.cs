
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class FindMatchUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private FilteringUI filteringUI;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button loadMoreButton;

    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private Button joinCodeButton;

    [SerializeField] private Transform contentContainer;

    [SerializeField] private LobbyListItemUI lobbyItemPrefab;
    [SerializeField] private TextMeshProUGUI loadingText;

    private string? currentContinuationToken;
    private bool isLoading;
    private List<LobbyListItemUI> currentItems = new();

    private void Start()
    {
        SetupUIListeners();
        loadingText.gameObject.SetActive(false);
        LoadInitialPage();
    }

    private void SetupUIListeners()
    {
        loadMoreButton.onClick.AddListener(LoadNextPage);
        refreshButton.onClick.AddListener(LoadInitialPage);
        joinCodeButton.onClick.AddListener(HandleJoinCodeButtonClick);
        
        // Add input validation for join code
        joinCodeInputField.onValidateInput += (string text, int charIndex, char addedChar) =>
        {
            // Only allow alphanumeric characters
            return char.IsLetterOrDigit(addedChar) ? addedChar : '\0';
        };
    }

    private async void HandleJoinCodeButtonClick()
    {
        if (string.IsNullOrWhiteSpace(joinCodeInputField.text))
        {
            OneInsideGameManager.Instance.UIManager.ShowMessage("Please enter a valid join code");
            return;
        }

        try
        {
            UpdateUIState(true);
            await OneInsideGameManager.Instance.JoinMatchByCodeAsync(joinCodeInputField.text);
        }
        catch (Exception e)
        {
            OneInsideGameManager.Instance.UIManager.ShowMessage($"Failed to join lobby by code: {e.Message}");
            Debug.LogWarning($"Failed to join lobby by code: {e.Message}");
        }
        finally
        {
            UpdateUIState(false);
        }
    }

    private async void LoadInitialPage()
    {
        ClearCurrentItems();
        await LoadLobbies(null);
    }

    private async void LoadNextPage()
    {
        if (isLoading || string.IsNullOrEmpty(currentContinuationToken))
            return;
        await LoadLobbies(currentContinuationToken);
    }

    private async Task LoadLobbies(string? continuationToken)
    {
        if (isLoading) return;

        try
        {
            isLoading = true;
            UpdateUIState(true);

            var query = filteringUI.LobbyQueryDto;
            query.ContinuationToken = continuationToken;

            var result = await OneInsideGameManager.Instance.LobbyManager.GetAllLobbyAsync(query);
            if (result != null)
            {
                foreach (var lobby in result.Items)
                {
                    CreateLobbyListItem(lobby);
                }

                currentContinuationToken = result.ContinuationToken;
                loadMoreButton.gameObject.SetActive(result.HasNextPage);
            }
        }
        catch (Exception e)
        {
            OneInsideGameManager.Instance.UIManager.ShowMessage($"Failed to load lobbies: {e.Message}");
            Debug.LogWarning($"Failed to load lobbies: {e.Message}");
        }
        finally
        {
            isLoading = false;
            UpdateUIState(false);
        }
    }

    private void CreateLobbyListItem(Lobby lobby)
    {
        var item = Instantiate(lobbyItemPrefab, contentContainer);
        item.Initialize(lobby);
        currentItems.Add(item);
    }

    private void ClearCurrentItems()
    {
        foreach (var item in currentItems)
        {
            Destroy(item.gameObject);
        }
        currentItems.Clear();
        currentContinuationToken = null;
    }

    private void UpdateUIState(bool loading)
    {
        loadingText.gameObject.SetActive(loading);
        loadMoreButton.interactable = !loading;
        refreshButton.interactable = !loading;
        joinCodeButton.interactable = !loading;
        joinCodeInputField.interactable = !loading;
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        loadMoreButton.onClick.RemoveListener(LoadNextPage);
        refreshButton.onClick.RemoveListener(LoadInitialPage);
        joinCodeButton.onClick.RemoveListener(HandleJoinCodeButtonClick);
    }
}