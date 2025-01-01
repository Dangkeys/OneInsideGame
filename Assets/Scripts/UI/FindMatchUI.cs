using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FindMatchUI : MonoBehaviour
{
    [SerializeField] private Transform lobbyItemParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;
    [field: SerializeField] public Button RefreshLobbiesButton { get; private set; }
    [field: SerializeField] public Button NextPageButton { get; private set; }
    [field: SerializeField] public Button PreviousPageButton { get; private set; }
    [field: SerializeField] public TMP_Text PageNumberText { get; private set; }
    [field: SerializeField] public TMP_InputField SearchInputField { get; private set; }

    private const int LOBBIES_PER_PAGE = 10;
    private int currentPage = 1;
    private bool hasMorePages;
    private bool isJoining;
    private bool isRefreshing;
    private string currentSearchTerm = "";
    public event Action<Lobby> OnLobbyJoined;

    private void Start()
    {
        RefreshLobbiesButton.onClick.AddListener(RefreshList);
        NextPageButton.onClick.AddListener(NextPage);
        PreviousPageButton.onClick.AddListener(PreviousPage);
        SearchInputField.onValueChanged.AddListener(OnSearchValueChanged);
        UpdatePageUI();
    }

    private void OnSearchValueChanged(string searchTerm)
    {
        currentSearchTerm = searchTerm.Trim();
        currentPage = 1;
        RefreshList();
    }

    private void OnEnable()
    {
        currentPage = 1;
        currentSearchTerm = "";
        if (SearchInputField != null)
        {
            SearchInputField.text = "";
        }
        RefreshList();
    }

    private void NextPage()
    {
        if (!hasMorePages) return;
        currentPage++;
        RefreshList();
    }

    private void PreviousPage()
    {
        if (currentPage <= 1) return;
        currentPage--;
        RefreshList();
    }

    private void UpdatePageUI()
    {
        PageNumberText.text = $"Page {currentPage}";
        PreviousPageButton.interactable = currentPage > 1;
        NextPageButton.interactable = hasMorePages;
    }

    public async void RefreshList()
    {
        if (isRefreshing) { return; }

        isRefreshing = true;
        UpdateButtonsInteractable(false);

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = LOBBIES_PER_PAGE + 1,
                Skip = (currentPage - 1) * LOBBIES_PER_PAGE,
                Filters = new List<QueryFilter>()
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"),
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.IsLocked,
                        op: QueryFilter.OpOptions.EQ,
                        value: "0")
                }
            };

            if (!string.IsNullOrWhiteSpace(currentSearchTerm))
            {
                options.Filters.Add(new QueryFilter(
                    field: QueryFilter.FieldOptions.Name,
                    op: QueryFilter.OpOptions.CONTAINS,
                    value: currentSearchTerm
                ));
            }

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            ClearCurrentLobbies();

            hasMorePages = lobbies.Results.Count > LOBBIES_PER_PAGE;
            
            int displayCount = Mathf.Min(lobbies.Results.Count, LOBBIES_PER_PAGE);
            for (int i = 0; i < displayCount; i++)
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Initialise(this, lobbies.Results[i]);
            }

            if (displayCount == 0)
            {
                // Optionally, show a "No lobbies found" message
                CreateNoLobbiesFoundMessage();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            hasMorePages = false;
            CreateNoLobbiesFoundMessage();
        }

        UpdatePageUI();
        UpdateButtonsInteractable(true);
        isRefreshing = false;
    }

    private void CreateNoLobbiesFoundMessage()
    {
        Debug.Log("No lobbies found");
    }

    private void ClearCurrentLobbies()
    {
        foreach (Transform child in lobbyItemParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void UpdateButtonsInteractable(bool interactable)
    {
        RefreshLobbiesButton.interactable = interactable;
        NextPageButton.interactable = interactable && hasMorePages;
        PreviousPageButton.interactable = interactable && currentPage > 1;
        SearchInputField.interactable = interactable;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isJoining) { return; }

        isJoining = true;

        try
        {
            Lobby joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
            OnLobbyJoined?.Invoke(joiningLobby);
            gameObject.SetActive(false);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }

        isJoining = false;
    }
}